using UnityEngine;

namespace SpaceFusion.SF_Portals.Scripts {

    /// <summary>
    /// This is a helper script that allows you to easily integrate the
    /// teleport functionality to your own PlayerController, so you don't have to use mine (if you dont want to)
    /// 
    /// </summary>
    public class PlayerTeleporter : Portable {

        private Rigidbody _rb;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        public override void Teleport(Transform from, Transform to, Vector3 pos, Quaternion rot) {
            transform.position = pos;
            var yRotation = transform.eulerAngles.y;
            yRotation += Mathf.DeltaAngle(yRotation, rot.eulerAngles.y);
            transform.rotation = Quaternion.Euler(transform.rotation.x, yRotation, transform.rotation.z);
            Physics.SyncTransforms();
            if (_rb) {
                _rb.linearVelocity = to.TransformVector(from.InverseTransformVector(_rb.linearVelocity));
                _rb.angularVelocity = to.TransformVector(from.InverseTransformVector(_rb.angularVelocity));
            }
        }
    }
}