using System.Linq;
using UnityEngine;

namespace SpaceFusion.SF_Portals.Scripts {
    /// <summary>
    /// Every Object that should be able to teleport through the portal should have this (or an inheritor).
    /// Integration notes:
    /// - portableObject: if left null, will default to this.gameObject.
    /// - When this Portable is picked by PickupInteraction, PickupInteraction should call OnPickedUp(this).
    /// - When dropped, PickupInteraction should call OnDropped().
    /// - When Teleport(...) is called by the portal system, Teleport will call NotifyTeleported() to let the holder reconnect.
    /// </summary>
    public class Portable : MonoBehaviour {

        [Header("Object that should be teleported. Clone of it will be created on the second portal")]
        public GameObject portableObject; // optional: leave null to use this.gameObject

        public GameObject clone { get; private set; }
        public Vector3 previousPortalOffset { get; set; }
        public Material[] originalMaterials { get; private set; }
        public Material[] cloneMaterials { get; private set; }

        // Holding state
        public bool IsHeld { get; private set; } = false;
        public object Holder { get; private set; } = null; // generic holder reference (PickupInteraction will pass itself)

        private void Awake()
        {
            if (portableObject == null) portableObject = gameObject;
        }

        public virtual void Teleport(Transform from, Transform to, Vector3 pos, Quaternion rot) {
            // The portal system will call this to move the real object.
            transform.position = pos;
            transform.rotation = rot;

            // force physics to sync immediately so any rigidbody reads are correct
            Physics.SyncTransforms();

            // If this object is currently held, notify the holder so it can re-parent / re-cache the Rigidbody.
            NotifyTeleported();
        }

        /// <summary>
        /// If Portable enters portal area, a clone will be activated (visual preview on the other side).
        /// </summary>
        public void EnterPortalArea() {
            if (clone != null) {
                clone.SetActive(true);
                return;
            }

            // instantiate clone as a visual only copy. Parent to original parent so hierarchy stays consistent.
            clone = Instantiate(portableObject);
            clone.transform.parent = portableObject.transform.parent;
            clone.transform.localScale = portableObject.transform.localScale;
            clone.transform.localRotation = portableObject.transform.localRotation;
            clone.transform.position = portableObject.transform.position;
            
            // Make clone safe: disable dynamics so it doesn't interfere with physics simulation.
            var rb = clone.GetComponentInChildren<Rigidbody>();
            if (rb != null) {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            foreach (var col in clone.GetComponentsInChildren<Collider>()) {
                col.isTrigger = true;
            }

            // only fetch materials if not already fetched on previous portal enter 
            originalMaterials ??= GetMaterials(portableObject);
            cloneMaterials ??= GetMaterials(clone);
        }

        /// <summary>
        /// portable exits portal area
        /// disable any portal-clipping flags on materials and hide the clone
        /// </summary>
        public void ExitPortalArea() {
            if (clone != null) clone.SetActive(false);

            if (originalMaterials != null) {
                foreach (var mat in originalMaterials) {
                    if (mat.HasProperty("_IsClipable")) mat.SetFloat("_IsClipable", 0);
                }
            }

            if (cloneMaterials != null) {
                foreach (var mat in cloneMaterials) {
                    if (mat.HasProperty("_IsClipable")) mat.SetFloat("_IsClipable", 0);
                }
            }
        }

        /// <summary>
        /// extracts all Materials of all the Children under the Portable GameObject
        /// </summary>
        private Material[] GetMaterials(GameObject g) {
            var renderers = g.GetComponentsInChildren<MeshRenderer>();
            return renderers.SelectMany(r => r.materials).ToArray();
        }

        // --- Integration API for pickup system ---

        /// <summary>
        /// Called by PickupInteraction when this object is picked.
        /// Pass the holder (usually the PickupInteraction instance) so we can notify it on teleport.
        /// </summary>
        public void OnPickedUp(object holder) {
            IsHeld = true;
            Holder = holder;
        }

        public void OnDropped() {
            IsHeld = false;
            Holder = null;
        }

        /// <summary>
        /// Called after Teleport() to let the holder reconnect to the teleported object.
        /// We expect the holder to implement a method like: void OnPortableTeleported(Portable p)
        /// We call it generically through reflection to avoid circular references; you can also cast to a known type.
        /// </summary>
        private void NotifyTeleported() {
            if (!IsHeld || Holder == null) return;

            // try common pattern: PickupInteraction has method OnPortableTeleported(Portable)
            var method = Holder.GetType().GetMethod("OnPortableTeleported");
            if (method != null) {
                method.Invoke(Holder, new object[] { this });
            }
            else {
                // fallback: try method OnTeleported(GameObject)
                var m2 = Holder.GetType().GetMethod("OnTeleported");
                if (m2 != null) m2.Invoke(Holder, new object[] { this.gameObject });
            }
        }
    }
}
