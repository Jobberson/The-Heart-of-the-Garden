using UnityEngine;

namespace SpaceFusion.SF_Portals.Scripts {
    [RequireComponent(typeof(Rigidbody))]
    public class PortableRigidBody : Portable {

        private Rigidbody _rb;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// the previous velocity and angular velocity that the object had before it entered the portal
        /// will be recalculated based on the exit point of the second portal
        /// </summary>
        public override void Teleport(Transform from, Transform to, Vector3 pos, Quaternion rot) {
            base.Teleport(from, to, pos, rot);
            _rb.linearVelocity = to.TransformVector(from.InverseTransformVector(_rb.linearVelocity));
            _rb.angularVelocity = to.TransformVector(from.InverseTransformVector(_rb.angularVelocity));
        }

    }
}