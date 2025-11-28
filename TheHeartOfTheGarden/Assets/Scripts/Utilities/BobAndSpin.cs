using UnityEngine;

public class BobAndSpin : MonoBehaviour
{
    [Header("Bob Settings")]
    [Tooltip("How high the object moves up and down.")]
    public float bobAmount = 0.25f;

    [Tooltip("How fast the object bobs up and down.")]
    public float bobSpeed = 2f;

    [Header("Spin Settings")]
    [Tooltip("How fast the object rotates (degrees per second).")]
    public float spinSpeed = 45f;

    [Tooltip("Axis around which the object spins.")]
    public Vector3 rotationAxis = Vector3.up;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Bobbing movement
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // Rotation
        transform.Rotate(rotationAxis, spinSpeed * Time.deltaTime, Space.Self);
    }
}
