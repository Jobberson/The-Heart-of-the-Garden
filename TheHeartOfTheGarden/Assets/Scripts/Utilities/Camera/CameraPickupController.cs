using UnityEngine;

/// <summary>
/// Flexible camera pickup/drop controller.
/// Handles case where cameraModel may start dropped in the world (not parented to player).
/// Supports DetectionMode: LastDropped, Layer (raycast by layer), Tag (raycast by tag).
/// </summary>
public class CameraPickupController : MonoBehaviour {
    public enum DetectionMode { LastDropped = 0, Layer = 1, Tag = 2 }

    [Header("References")]
    [Tooltip("The camera model GameObject (visual). May start parented to holdPoint or placed in world.")]
    public GameObject cameraModel;

    [Tooltip("Transform where the model should be parented when picked up (e.g. a 'hand' transform under the camera).")]
    public Transform holdPoint;

    [Header("Input")]
    public KeyCode pickupKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.G;

    [Header("Drop / Pickup Settings")]
    public float pickupRange = 2.0f;
    public float pickupMaxAngle = 50f;
    public float dropForwardOffset = 0.6f;
    public float throwForce = 0f;

    [Header("Detection")]
    public DetectionMode detectionMode = DetectionMode.LastDropped;
    public LayerMask cameraLayerMask;
    public string cameraTag = "CameraItem";
    public float raycastMaxDistance = 3.0f;

    // internal
    Rigidbody cachedRb;
    Collider[] colliders;
    Vector3 heldLocalPos;
    Quaternion heldLocalRot;
    Vector3 heldLocalScale;
    bool isHolding;
    GameObject lastDroppedInstance;

    void Awake() {
        if (cameraModel == null) {
            Debug.LogError("[CameraPickupController] cameraModel not assigned.", this);
            enabled = false;
            return;
        }

        // fallback holdPoint: if not set, try to use cameraModel's parent or this transform
        if (holdPoint == null) {
            holdPoint = cameraModel.transform.parent ? cameraModel.transform.parent : transform;
        }

        // cache original local transform so reattachment matches perfectly
        heldLocalPos = cameraModel.transform.localPosition;
        heldLocalRot = cameraModel.transform.localRotation;
        heldLocalScale = cameraModel.transform.localScale;

        // gather colliders on the cameraModel
        colliders = cameraModel.GetComponentsInChildren<Collider>(true);

        // ensure rigidbody exists
        cachedRb = cameraModel.GetComponent<Rigidbody>();
        if (cachedRb == null) {
            cachedRb = cameraModel.AddComponent<Rigidbody>();
            cachedRb.isKinematic = true; // assume held until we check
            cachedRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // ensure there is at least one collider
        if (colliders == null || colliders.Length == 0) {
            var renderer = cameraModel.GetComponentInChildren<Renderer>();
            if (renderer != null) {
                var bc = cameraModel.AddComponent<BoxCollider>();
                bc.center = cameraModel.transform.InverseTransformPoint(renderer.bounds.center);
                bc.size = cameraModel.transform.InverseTransformVector(renderer.bounds.size);
                colliders = new Collider[] { bc };
            } else {
                var bc = cameraModel.AddComponent<BoxCollider>();
                bc.size = Vector3.one * 0.2f;
                colliders = new Collider[] { bc };
            }
        }

        // Determine initial state: is the cameraModel parented to the holdPoint?
        if (cameraModel.transform.IsChildOf(holdPoint)) {
            // starts held
            isHolding = true;
            SetCollidersEnabled(false);
            if (cachedRb != null) cachedRb.isKinematic = true;
            lastDroppedInstance = cameraModel;
        } else {
            // starts dropped in the world
            isHolding = false;
            SetCollidersEnabled(true);
            if (cachedRb != null) cachedRb.isKinematic = false;
            lastDroppedInstance = cameraModel;
        }
    }

    void Update() {
        // drop
        if (isHolding && Input.GetKeyDown(dropKey)) {
            Drop();
            return;
        }

        // pickup
        if (!isHolding && Input.GetKeyDown(pickupKey)) {
            switch (detectionMode) {
                case DetectionMode.LastDropped:
                    TryPickupLastDropped();
                    break;
                case DetectionMode.Layer:
                    TryPickupByLayer();
                    break;
                case DetectionMode.Tag:
                    TryPickupByTag();
                    break;
            }
        }
    }

    void Drop() {
        // detach the model into world
        cameraModel.transform.SetParent(null, true);

        // position slightly in front of the holdPoint to avoid overlapping
        Vector3 spawnPos = holdPoint.position + holdPoint.forward * dropForwardOffset;
        cameraModel.transform.position = spawnPos;
        cameraModel.transform.rotation = holdPoint.rotation;

        // enable physics
        if (cachedRb == null) cachedRb = cameraModel.AddComponent<Rigidbody>();
        cachedRb.isKinematic = false;
        if (throwForce != 0f) cachedRb.AddForce(holdPoint.forward * throwForce, ForceMode.Impulse);

        SetCollidersEnabled(true);

        isHolding = false;
        lastDroppedInstance = cameraModel;
    }

    void TryPickupLastDropped() {
        if (lastDroppedInstance == null) return;

        float dist = Vector3.Distance(lastDroppedInstance.transform.position, holdPoint.position);
        if (dist > pickupRange) return;

        Vector3 toDropped = (lastDroppedInstance.transform.position - holdPoint.position).normalized;
        float angle = Vector3.Angle(holdPoint.forward, toDropped);
        if (angle > pickupMaxAngle) return;

        Pickup(lastDroppedInstance);
    }

    void TryPickupByLayer() {
        // raycast from the player's viewpoint (use holdPoint.forward for consistency)
        Ray ray = new Ray(holdPoint.position, holdPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance, cameraLayerMask)) {
            var candidate = hit.collider.gameObject;
            float dist = Vector3.Distance(candidate.transform.position, holdPoint.position);
            if (dist > pickupRange) return;

            Vector3 toDropped = (candidate.transform.position - holdPoint.position).normalized;
            float angle = Vector3.Angle(holdPoint.forward, toDropped);
            if (angle > pickupMaxAngle) return;

            Pickup(candidate);
        }
    }

    void TryPickupByTag() {
        Ray ray = new Ray(holdPoint.position, holdPoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastMaxDistance)) {
            var candidate = hit.collider.gameObject;
            if (!candidate.CompareTag(cameraTag)) return;

            float dist = Vector3.Distance(candidate.transform.position, holdPoint.position);
            if (dist > pickupRange) return;

            Vector3 toDropped = (candidate.transform.position - holdPoint.position).normalized;
            float angle = Vector3.Angle(holdPoint.forward, toDropped);
            if (angle > pickupMaxAngle) return;

            Pickup(candidate);
        }
    }

    void Pickup(GameObject dropped) {
        dropped.transform.SetParent(holdPoint, true);
        dropped.transform.localPosition = heldLocalPos;
        dropped.transform.localRotation = heldLocalRot;
        dropped.transform.localScale = heldLocalScale;

        var rb = dropped.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        SetCollidersEnabled(false);

        isHolding = true;
        lastDroppedInstance = dropped;
    }

    void SetCollidersEnabled(bool enabled) {
        if (colliders == null) return;
        foreach (var c in colliders) {
            if (c != null) c.enabled = enabled;
        }
    }

    /// <summary>
    /// Force attach camera to hand (scripted)
    /// </summary>
    public void ForceAttachToHand() {
        Pickup(cameraModel);
    }

    /// <summary>
    /// Force drop camera at player's feet (scripted)
    /// </summary>
    public void ForceDropAtFeet() {
        Drop();
    }

    void OnDrawGizmosSelected() {
        if (holdPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(holdPoint.position, pickupRange);

        if (detectionMode == DetectionMode.Layer || detectionMode == DetectionMode.Tag) {
            Gizmos.color = Color.yellow;
            Vector3 dir = holdPoint ? holdPoint.forward : transform.forward;
            Gizmos.DrawLine(holdPoint.position, holdPoint.position + dir * raycastMaxDistance);
        }
    }
}
