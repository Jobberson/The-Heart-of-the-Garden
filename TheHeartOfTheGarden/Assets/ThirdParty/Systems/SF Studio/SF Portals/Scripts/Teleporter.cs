using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFusion.SF_Portals.Scripts {
    public class Teleporter : MonoBehaviour {

        public Teleporter linkedPortal;

        
        [SerializeField]
        protected MeshRenderer screen;
        
        protected List<Portable> _trackedPortables;

        
        // shader properties
        private static readonly int IsClipable = Shader.PropertyToID("_IsClipable");
        private static readonly int Normal = Shader.PropertyToID("_Normal");
        private static readonly int ClipPlane = Shader.PropertyToID("_ClipPlane");
        protected static readonly int MainTexture = Shader.PropertyToID("_MainTexture");

        private void Awake() {
            _trackedPortables = new List<Portable>();

        }

        public MeshRenderer GetScreen() {
            return screen;
        }
        
        /// <summary>
        /// do NOT use Update for HandlePortables, we need LateUpdate to fix some flickering on teleport
        /// </summary>
        private void LateUpdate() {
            HandlePortables();
            foreach (var portable in _trackedPortables) {
                SlicePortables(portable);
            }
        }


        /// <summary>
        /// Keeps track of all portable that are in the portals area of effect and
        /// handles teleporting all the portable objects that are going through the portal
        /// </summary>
        protected void HandlePortables() {
            for (var i = 0; i < _trackedPortables.Count; i++) {
                var portable = _trackedPortables[i];
                var portableObjT = portable.transform;
                var portalT = transform; // transform of the portal where this script is attached
                // calculate the position and rotation where the object should be teleported
                var tMatrix = linkedPortal.transform.localToWorldMatrix * portalT.worldToLocalMatrix *
                              portableObjT.localToWorldMatrix;
                var currentOffset = GetOffsetFromPortal(portable);
                var switchedSide = HasSwitchedPortalSide(currentOffset, portable.previousPortalOffset);
                var cloneT = portable.clone.transform;
                if (switchedSide) {
                    // Teleport object if it has crossed from one side of the portal to the other
                    portable.Teleport(portalT, linkedPortal.transform, tMatrix.GetColumn(3), tMatrix.rotation);
                    cloneT.SetPositionAndRotation(portableObjT.position,
                        portableObjT.rotation);
                    linkedPortal.EnterPortal(portable);
                    _trackedPortables.RemoveAt(i);
                    i--;
                } else {
                    cloneT.SetPositionAndRotation(tMatrix.GetColumn(3), tMatrix.rotation);
                    portable.previousPortalOffset = currentOffset;
                }
            }
        }
        
        

        /// <summary>
        /// some collider entered portal threshold
        /// </summary>
        protected void OnTriggerEnter(Collider other) {
            var portable = other.GetComponent<Portable>();
            if (portable) {
                EnterPortal(portable);
            }
        }

        /// <summary>
        /// Handles the portal exit of a portable object
        /// </summary>
        protected void OnTriggerExit(Collider other) {
            var portable = other.GetComponent<Portable>();
            if (!portable || !_trackedPortables.Contains(portable)) {
                return;
            }

            portable.ExitPortalArea();
            _trackedPortables.Remove(portable);
        }

        /// <summary>
        /// Handles the portal enter of a portable object
        /// adds the portable to the tracked list and updates the previousOffsetFromPortal
        /// </summary>
        private void EnterPortal(Portable portable) {
            if (_trackedPortables.Contains(portable)) {
                return;
            }

            portable.EnterPortalArea();
            portable.previousPortalOffset = GetOffsetFromPortal(portable);
            _trackedPortables.Add(portable);
        }

        /// <summary>
        /// calculates angle between portals forward vector and the offset of traveler to the portal
        /// If obj still on the same portal side, both angles should be the same
        /// if current and previous offset differs, the object has changed the portal sides
        /// </summary>
        /// <param name="portalOffset"> offset from portal to the portable object</param>
        /// <param name="previousPortalOffset">same offset but from the previous frame </param>
        /// <returns>if object crossed the portal sides</returns>
        private bool HasSwitchedPortalSide(Vector3 portalOffset, Vector3 previousPortalOffset) {
            var current = Math.Sign(Vector3.Dot(portalOffset, transform.forward));
            var previous = Math.Sign(Vector3.Dot(previousPortalOffset, transform.forward));
            return current != previous;
        }

        /// <summary>
        /// calculates the offset of the portable object from the portal center
        /// </summary>
        private Vector3 GetOffsetFromPortal(Component portable) {
            return portable.transform.position - transform.position;
        }

        /// <summary>
        /// calculates the on which side of the portal we are
        /// -1 is same direction as the portal (e.g. staying at the back of the portal and looking towards the portal)
        /// 1 is on the other side of the portal (front of the portal)
        /// </summary>
        protected int GetPortalSide(Vector3 position) {
            return Math.Sign(Vector3.Dot(position - transform.position, transform.forward));
        }


        /// <summary>
        /// Updates the properties of the SF Slice shader material(if this shader is used)
        /// to create a seamless transition from the third person view of portables going through portal
        /// </summary>
        private void SlicePortables(Portable portable) {
            var portalSide = GetPortalSide(portable.transform.position);
            var normal = transform.forward * portalSide;
            var clipPlane = transform.position;

            // clone normal should be inverse of the default normal --> multiply by negative portalSide
            var cloneNormal = linkedPortal.transform.forward * -portalSide;
            var cloneClipPlane = linkedPortal.transform.position;
            foreach (var mat in portable.originalMaterials) {
                UpdateSliceShaderMaterial(mat, clipPlane, normal, 1);
            }

            foreach (var mat in portable.cloneMaterials) {
                UpdateSliceShaderMaterial(mat, cloneClipPlane, cloneNormal, 1);
            }
        }

        private static void UpdateSliceShaderMaterial(Material mat, Vector3 clip, Vector3 normal, int clipable) {
            mat.SetVector(ClipPlane, clip);
            mat.SetVector(Normal, normal);
            mat.SetFloat(IsClipable, clipable);
        }
    }
}