using UnityEngine;

namespace Snog.InteractionSystem.Demo
{
[RequireComponent(typeof(CharacterController))]
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;

        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float upLimit = 80.0f;
        [SerializeField] private float downLimit = 80.0f;

        private CharacterController characterController;
        private Camera playerCamera;
        private float rotationX = 0;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            playerCamera = Camera.main;

            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;
        }

        private void Update()
        {
            HandleMovement();
            HandleMouseLook();
        }

        private void HandleMovement()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            rotationX -= mouseY;

            rotationX = Mathf.Clamp(rotationX, -upLimit, downLimit);

            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }
}