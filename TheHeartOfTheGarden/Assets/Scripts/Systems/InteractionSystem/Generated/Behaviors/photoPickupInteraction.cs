using UnityEngine;
using Snog.InteractionSystem.Core.Interfaces;

namespace Snog.InteractionSystem.Behaviors
{
    /// <summary>
    /// Interaction that picks up / drops a camera model (or any "camera-like" object).
    /// Works entirely through code: finds/creates a HoldPoint on Camera.main, ensures physics components exist,
    /// stores local transform for perfect reattachment, and toggles pickup/drop.
    /// </summary>
    public class photoPickupInteraction : MonoBehaviour, IInteractionBehavior
    {
        // Tunables — change in code or expose later if you want to inspectorize.
        const float defaultPickupRange = 3.0f;
        const float defaultPickupMaxAngle = 360f;
        const float defaultDropForwardOffset = 0f;
        const float defaultThrowForce = 0f;

        private void Update() 
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.G))
            {
                // For testing: drop the currently held camera if any
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    Transform holdPoint = mainCam.transform.Find("CameraHoldPoint");
                    if (holdPoint != null && holdPoint.childCount > 0)
                    {
                        GameObject heldObj = holdPoint.GetChild(0).gameObject;
                        DropTarget(heldObj, holdPoint, defaultDropForwardOffset, defaultThrowForce);
                        Debug.Log($"photoPickupInteraction: Dropped {heldObj.name}");
                        mainCam.transform.GetComponent<PhotoCamera>().enabled = false;
                    }
                }
            }
            
        }
        // Execute is called by your interaction system with the target gameobject (the dropped camera model).
        public void Execute(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("[photoPickupInteraction] Execute called with null target.");
                return;
            }

            // Find player view (main camera)
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogWarning("[photoPickupInteraction] No main camera found. Cannot pick up camera.");
                return;
            }

            // Ensure a holdPoint exists under the main camera (create if missing).
            Transform holdPoint = EnsureHoldPoint(mainCam.transform);

            // Ensure the target has the physics/colliders needed for dropping/picking up
            EnsurePhysicsAndState(target);

            // Otherwise attempt pickup (distance + facing checks)
            float dist = Vector3.Distance(target.transform.position, holdPoint.position);
            if (dist > defaultPickupRange)
            {
                Debug.Log($"photoPickupInteraction: {target.name} is too far to pick up (dist {dist:F2} > {defaultPickupRange}).");
                return;
            }

            Vector3 toTarget = (target.transform.position - holdPoint.position).normalized;
            float angle = Vector3.Angle(holdPoint.forward, toTarget);
            if (angle > defaultPickupMaxAngle)
            {
                Debug.Log($"photoPickupInteraction: Not facing camera enough (angle {angle:F1}° > {defaultPickupMaxAngle}°).");
                return;
            }

            // All good: pick up
            PickupTarget(target, holdPoint);
            Debug.Log($"photoPickupInteraction: Picked up {target.name}");
        }

        // ---- Helpers ----

        // Find or create a HoldPoint under the main camera. Uses a default offset that looks sensible for a handheld camera.
        Transform EnsureHoldPoint(Transform camTransform)
        {
            const string holdName = "CameraHoldPoint";
            Transform hp = camTransform.Find(holdName);
            if (hp != null) return hp;

            GameObject go = new(holdName);
            go.transform.SetParent(camTransform, false);

            // sensible default local transform for a handheld camera; tweak if needed
            go.transform.localPosition = new Vector3(0.863f, -0.341f, 0.889f);
            go.transform.localRotation = Quaternion.Euler(10f, 180f, 0f);
            go.transform.localScale = Vector3.one;

            return go.transform;
        }

        // Make sure the dropped camera model has colliders and a rigidbody, and attach / preserve original local transform via helper
        void EnsurePhysicsAndState(GameObject target)
        {
            if (target.GetComponent<CameraPickupState>() == null)
            {
                // store original local transform relative to its current parent (if any)
                var state = target.AddComponent<CameraPickupState>();
                state.RecordLocalTransform(target.transform);
            }

            // add a collider if none found
            Collider[] col = target.GetComponentsInChildren<Collider>(true);
            if (col == null || col.Length == 0)
            {
                var renderer = target.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    BoxCollider bc = target.AddComponent<BoxCollider>();
                    // approximate box using renderer bounds (in world) -> convert to local
                    bc.center = target.transform.InverseTransformPoint(renderer.bounds.center);
                    bc.size = target.transform.InverseTransformVector(renderer.bounds.size);
                }
                else
                {
                    // fallback small collider
                    var bc = target.AddComponent<BoxCollider>();
                    bc.size = Vector3.one * 0.2f;
                }
            }

            // add Rigidbody if missing
            if (!target.TryGetComponent<Rigidbody>(out _))
            {
                Rigidbody rb = target.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }

            // When present in the scene as dropped object, physics should be enabled; when parented to hand we'll set kinematic.
            // Do not force a specific isKinematic state here; that is handled on pickup/drop steps.
        }

        // Pickup: parent to holdPoint and disable physics/colliders, restore recorded local transform
        void PickupTarget(GameObject target, Transform holdPoint)
        {
            // stop rigidbody movement and make kinematic
            if (target.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // disable child colliders
            SetCollidersEnabled(target, false);

            // parent to holdPoint and restore the stored local transform if available
            target.transform.SetParent(holdPoint, true);
            if (target.TryGetComponent<CameraPickupState>(out _))
            {
                //state.ApplyLocalTransform(target.transform);
                var localPosition = new Vector3(0.094f, -0.068f, 0.047f);
                var localRotation = Quaternion.Euler(-10f, 180f, 0f);
                target.transform.SetLocalPositionAndRotation(localPosition, localRotation);
                var cameraSnap = Camera.main.transform.GetComponent<PhotoCamera>();
                cameraSnap.enabled = true;
            }
            else
            {
                // fallback: a neutral local transform
                target.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        // Drop: unparent, place a little in front of the player, enable physics & colliders
        void DropTarget(GameObject target, Transform holdPoint, float forwardOffset, float throwForce)
        {
            // unparent to world
            target.transform.SetParent(null, true);

            // position in front of the holdPoint so it doesn't clip into the player
            Vector3 spawnPos = holdPoint.position + holdPoint.forward * forwardOffset;
            target.transform.SetPositionAndRotation(spawnPos, holdPoint.rotation);

            // enable physics
            if (target.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
                if (throwForce != 0f)
                    rb.AddForce(holdPoint.forward * throwForce, ForceMode.Impulse);
            }

            // re-enable colliders so the dropped object interacts with world
            SetCollidersEnabled(target, true);
        }

        void SetCollidersEnabled(GameObject root, bool enabled)
        {
            var cols = root.GetComponentsInChildren<Collider>(true);
            if (cols == null) return;
            foreach (var c in cols) if (c != null) c.enabled = enabled;
        }

        // small helper component to save/restore local transform (stored on the target object)
        class CameraPickupState : MonoBehaviour
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;

            public void RecordLocalTransform(Transform t)
            {
                localPosition = t.localPosition;
                localRotation = t.localRotation;
                localScale = t.localScale;
            }

            public void ApplyLocalTransform(Transform t)
            {
                t.SetLocalPositionAndRotation(localPosition, localRotation);
                t.localScale = localScale;
            }
        }
    }
}
