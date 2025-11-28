﻿using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceFusion.SF_Portals.Scripts {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : Portable {
        public Camera cam;
        public float speed = 5;
        public float jumpHeight = 1.0f;
        public float mouseSpeed = 10;

        // Vertical clamp limits (degrees)
        public float minPitch = -85f;
        public float maxPitch = 85f;

        public bool rotateOnlyOnRightMouseHold;
        private CharacterController controller;
        public float viewDirection;

        private readonly float _gravity = 9.81f;
        private float _jumpForce;
        private Vector3 _playerVelocity;
        private bool _grounded;

        // --- Added ---
        public bool _controlsEnabled = true;

        // current camera pitch (degrees, -180..180)
        private float camPitch;

        private void Start() {
            controller = GetComponent<CharacterController>();
            viewDirection = transform.eulerAngles.y;
            _jumpForce = Mathf.Sqrt(jumpHeight * 3.0f * _gravity);

            // Initialize camPitch from the camera's local X angle and normalize to [-180,180]
            if (cam != null) {
                camPitch = NormalizeAngle(cam.transform.localEulerAngles.x);
                // ensure the camera starts within the clamp
                camPitch = Mathf.Clamp(camPitch, minPitch, maxPitch);
                cam.transform.localRotation = Quaternion.Euler(camPitch, 0f, 0f);
            }
        }

        private void Update() {
            if (!_controlsEnabled) return; // Disable both movement and rotation

            _grounded = controller.isGrounded;
            if (_grounded && _playerVelocity.y < 0) {
                _playerVelocity.y = 0f;
            }

            // --- Movement ---
            Vector3 inputVector;
#if ENABLE_INPUT_SYSTEM
            var x = 0f;
            var y = 0f;

            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;

            inputVector = new Vector3(x, 0, y).normalized;
            var worldDirection = transform.TransformDirection(inputVector);
#elif ENABLE_LEGACY_INPUT_MANAGER
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            var worldDirection = transform.TransformDirection(inputVector);
#endif
            var moveWorldDirection = worldDirection * speed;

            // Apply gravity
            _playerVelocity.y -= _gravity * Time.deltaTime;
            _playerVelocity = new Vector3(moveWorldDirection.x, _playerVelocity.y, moveWorldDirection.z);
            controller.Move(_playerVelocity * Time.deltaTime);

            // --- Jump ---
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.spaceKey.isPressed && _grounded) {
                _playerVelocity.y = _jumpForce;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Space) && _grounded) {
                _playerVelocity.y = _jumpForce;
            }
#endif

            // --- Camera Rotation ---
            var mouseInput = Vector2.zero;
            if (!rotateOnlyOnRightMouseHold || IsRightMouseButtonDown()) {
#if ENABLE_INPUT_SYSTEM
                mouseInput = Mouse.current.delta.ReadValue() * 0.01f;
#elif ENABLE_LEGACY_INPUT_MANAGER
                mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#endif
            }

            // Yaw (player)
            viewDirection += mouseInput.x * mouseSpeed;
            transform.rotation = Quaternion.Euler(0, viewDirection, 0);

            // Pitch (camera) - accumulate and clamp
            if (cam != null) {
                // invert Y as before (mouse up should look up)
                camPitch += -mouseInput.y * mouseSpeed;
                camPitch = Mathf.Clamp(camPitch, minPitch, maxPitch);
                cam.transform.localRotation = Quaternion.Euler(camPitch, 0f, 0f);
            }
        }

        public override void Teleport(Transform from, Transform to, Vector3 pos, Quaternion rot) {
            transform.position = pos;
            viewDirection += Mathf.DeltaAngle(viewDirection, rot.eulerAngles.y);
            transform.rotation = Quaternion.Euler(0, viewDirection, 0);
            Physics.SyncTransforms();
            // note: camera pitch is preserved; yaw already applied to player transform
        }

        /// <summary>
        /// Normalize an Euler X angle into [-180, 180]
        /// </summary>
        private static float NormalizeAngle(float angle) {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        /// <summary>
        /// Checks if the right mouse button is being pressed or held.
        /// </summary>
        private bool IsRightMouseButtonDown() {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.rightButton.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(1);
#endif
        }

        /// <summary>
        /// Disables both player movement and camera rotation.
        /// </summary>
        public void DisableControls() {
            _controlsEnabled = false;
            _playerVelocity = Vector3.zero;
        }

        /// <summary>
        /// Enables player movement and camera rotation again.
        /// </summary>
        public void EnableControls() {
            _controlsEnabled = true;
        }
    }
}
