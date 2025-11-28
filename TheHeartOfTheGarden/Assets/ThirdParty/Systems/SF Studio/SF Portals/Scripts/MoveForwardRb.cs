using UnityEngine;

namespace SpaceFusion.SF_Portals.Scripts {

    [RequireComponent(typeof(Rigidbody))]
    public class MoveForwardRb : MonoBehaviour {

        public float speed =10f;
        private Rigidbody _rb;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        private void LateUpdate() {
            _rb.linearVelocity = transform.forward * (speed * Time.deltaTime);
        }
    }
}