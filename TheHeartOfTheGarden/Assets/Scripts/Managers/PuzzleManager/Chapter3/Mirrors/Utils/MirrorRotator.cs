using UnityEngine;

public class MirrorRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;
    public Vector3 rotationAxis = Vector3.up; 

    [Header("Input Settings")]
    public KeyCode rotateClockwiseKey = KeyCode.E;
    public KeyCode rotateCounterClockwiseKey = KeyCode.Q;

    [HideInInspector] public bool interacting = false;

    void Update()
    {
        if(interacting)
            HandleRotationInput();
    }

    void HandleRotationInput()
    {
        if (Input.GetKey(rotateClockwiseKey))
        {
            RotateMirror(rotationSpeed);
        }

        if (Input.GetKey(rotateCounterClockwiseKey))
        {
            RotateMirror(-rotationSpeed);
        }
    }

    void RotateMirror(float speed)
    {
        transform.Rotate(speed * Time.deltaTime * rotationAxis);
    }
}