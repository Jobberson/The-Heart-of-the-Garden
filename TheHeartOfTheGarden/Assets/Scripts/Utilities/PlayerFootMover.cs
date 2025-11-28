using UnityEngine;

/// <summary>
/// PlayerFootMover (improved sync)
/// - Single basePhase drives both feet so they keep a stable offset (no drifting).
/// - Uses edge-cross detection (prev->curr) to fire footstep events exactly once per cycle,
///   including correct wrap-around handling.
/// - Keeps movement start-delay / stop-grace filtering from previous version.
/// - Options:
///     * perfectOpposition: forces right foot = left + 0.5 (opposite).
///     * leadWithLeft: when starting from idle, choose which foot leads.
/// </summary>
[DisallowMultipleComponent]
public class PlayerFootMover : MonoBehaviour
{
    [Header("Feet")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Footstep System")]
    public GameObject footstepsTarget;
    public UnityEngine.Object leftFootType;
    public UnityEngine.Object rightFootType;

    [Header("Gait")]
    [Tooltip("Base steps per second at 'speedForMaxStep' movement speed.")]
    public float stepSpeed = 1.6f;
    [Tooltip("How high the foot lifts at the peak (local units)")]
    public float stepHeight = 0.15f;
    [Tooltip("Forward/backward travel of the foot (local units)")]
    public float stepLength = 0.12f;
    [Range(1f, 30f)] public float smoothing = 12f;

    [Header("Phasing")]
    [Tooltip("Phase offset for right foot relative to left (0..1). Default 0.5 = opposite.")]
    [Range(0f, 1f)] public float oppositePhase = 0.5f;
    [Tooltip("Force exact opposition (right = left + 0.5) even if oppositePhase changed")]
    public bool perfectOpposition = true;
    [Tooltip("When starting to step from idle, which foot leads?")]
    public bool leadWithLeft = true;

    [Header("Event timing (normalized 0..1)")]
    [Range(0f, 1f)] public float startTrigger = 0.05f;
    [Range(0f, 1f)] public float middleTrigger = 0.5f;
    [Range(0f, 1f)] public float endTrigger = 0.95f;

    [Header("Movement Detection")]
    public Rigidbody playerRigidbody;
    public CharacterController playerController;
    public bool useTransformDeltaFallback = true;
    public float minMoveSpeed = 0.05f;
    public float speedForMaxStep = 3f;

    [Header("Tap/filtering (prevents a single immediate step on a tap)")]
    public float movementStartDelay = 0.15f;
    public float movementStopDelay = 0.10f;

    // internal: single base phase for both feet (0..1)
    float basePhase = 0f;
    float prevBasePhase = 0f;

    // derived phases
    float leftPhase => Mathf.Repeat(basePhase, 1f);
    float rightPhase => Mathf.Repeat(basePhase + (perfectOpposition ? 0.5f : oppositePhase), 1f);

    Vector3 leftRestLocal;
    Vector3 rightRestLocal;

    Vector3 lastPosition;

    float movementTimer = 0f;
    bool steppingActive = false;

    void Start()
    {
        if (leftFoot == null || rightFoot == null)
        {
            Debug.LogError("[PlayerFootMover] Assign leftFoot and rightFoot transforms in inspector.");
            enabled = false;
            return;
        }

        leftRestLocal = leftFoot.localPosition;
        rightRestLocal = rightFoot.localPosition;
        lastPosition = transform.position;

        movementStartDelay = Mathf.Max(0f, movementStartDelay);
        movementStopDelay = Mathf.Max(0.001f, movementStopDelay);
    }

    void Update()
    {
        float currentSpeed = GetPlayerSpeed();
        bool rawMoving = currentSpeed >= minMoveSpeed;

        UpdateMovementTimer(rawMoving);

        bool wasStepping = steppingActive;
        steppingActive = movementTimer >= movementStartDelay;

        // if we just became active, optionally set basePhase so we don't trigger both immediately
        if (!wasStepping && steppingActive)
        {
            InitializeBasePhaseOnStart();
        }

        prevBasePhase = basePhase;

        if (steppingActive)
        {
            // scale phase advance by movement speed (so running speeds up steps)
            float speedScale = Mathf.Clamp01(currentSpeed / Mathf.Max(0.0001f, speedForMaxStep));
            float delta = stepSpeed * Mathf.Lerp(0.6f, 1.6f, speedScale) * Time.deltaTime;
            basePhase = Mathf.Repeat(basePhase + delta, 1f);

            // update feet transforms from derived phases
            UpdateFootTransform(leftFoot, leftRestLocal, leftPhase);
            UpdateFootTransform(rightFoot, rightRestLocal, rightPhase);

            // detect crossings for both feet using prevDerivedPhase -> derivedPhase
            HandleFootEventsEdge(leftPhaseFromBase(prevBasePhase), leftPhase, leftFootType);
            HandleFootEventsEdge(rightPhaseFromBase(prevBasePhase), rightPhase, rightFootType);
        }
        else
        {
            // smoothly return to rest and reset triggers (no active stepping)
            ReturnFootToRest(leftFoot, leftRestLocal);
            ReturnFootToRest(rightFoot, rightRestLocal);
            // Resetting event state is not necessary with edge-based detection, but keep basePhase stable.
        }

        lastPosition = transform.position;
    }

    // compute derived phases from a basePhase value (used for prevBasePhase -> prevDerivedPhase)
    float leftPhaseFromBase(float basePhaseValue) => Mathf.Repeat(basePhaseValue, 1f);
    float rightPhaseFromBase(float basePhaseValue) => Mathf.Repeat(basePhaseValue + (perfectOpposition ? 0.5f : oppositePhase), 1f);

    void HandleFootEventsEdge(float prevPhase, float currPhase, UnityEngine.Object footType)
    {
        // For each named trigger, check if we crossed it going forward around the circle.
        if (PhaseCrossed(prevPhase, currPhase, startTrigger))
            SendFootEvent("FootstepStart", footType);

        if (PhaseCrossed(prevPhase, currPhase, middleTrigger))
            SendFootEvent("FootstepMiddle", footType);

        if (PhaseCrossed(prevPhase, currPhase, endTrigger))
            SendFootEvent("FootstepEnd", footType);
    }

    // returns true if the phase passed the trigger between prev->curr (handles wrap)
    bool PhaseCrossed(float prev, float curr, float trigger)
    {
        // Normalize all values (0..1)
        prev = Mathf.Repeat(prev, 1f);
        curr = Mathf.Repeat(curr, 1f);
        trigger = Mathf.Repeat(trigger, 1f);

        if (curr >= prev)
        {
            // normal forward motion without wrap
            return prev < trigger && curr >= trigger;
        }
        else
        {
            // wrapped around (e.g., prev=0.95 -> curr=0.02)
            // crossing occurs if trigger is > prev (later in old cycle) OR <= curr (early in new cycle)
            return (prev < trigger && trigger <= 1f) || (0f <= trigger && curr >= trigger);
        }
    }

    void InitializeBasePhaseOnStart()
    {
        // Align basePhase so the chosen leading foot is just before its startTrigger,
        // avoiding both feet emitting at once. This gives a consistent initial step.
        // We'll place the leading foot slightly before startTrigger so it naturally reaches startTrigger soon.

        // Determine desired left base phase such that leftPhase ~= startTrigger * 0.0..1.0
        float leadOffset = 0.1f * (1f / Mathf.Max(0.0001f, stepSpeed)); // small offset scaled to stepSpeed

        if (leadWithLeft)
        {
            // set basePhase so left is slightly before its startTrigger
            basePhase = Mathf.Repeat(startTrigger - leadOffset, 1f);
        }
        else
        {
            // set basePhase so right is slightly before its startTrigger -> compute base needed
            float desiredRight = Mathf.Repeat(startTrigger - leadOffset, 1f);
            // basePhase + rightOffset = desiredRight (mod 1)
            float rightOffset = (perfectOpposition ? 0.5f : oppositePhase);
            basePhase = Mathf.Repeat(desiredRight - rightOffset, 1f);
        }

        prevBasePhase = basePhase; // avoid accidental huge jumps
    }

    void UpdateMovementTimer(bool rawMoving)
    {
        float dt = Time.deltaTime;
        if (rawMoving)
        {
            movementTimer += dt;
        }
        else
        {
            float decayMultiplier = movementStartDelay / movementStopDelay;
            movementTimer -= dt * decayMultiplier;
        }

        movementTimer = Mathf.Clamp(movementTimer, 0f, movementStartDelay);
    }

    float GetPlayerSpeed()
    {
        if (playerRigidbody != null) return playerRigidbody.linearVelocity.magnitude;
        if (playerController != null) return playerController.velocity.magnitude;
        if (useTransformDeltaFallback) return (transform.position - lastPosition).magnitude / Mathf.Max(1e-6f, Time.deltaTime);
        return 0f;
    }

    void UpdateFootTransform(Transform foot, Vector3 restLocal, float phase)
    {
        float forwardOffset = (Mathf.Cos(phase * Mathf.PI * 2f) - 1f) * 0.5f; // -1..0
        float lift = Mathf.Abs(Mathf.Sin(phase * Mathf.PI)); // 0..1..0

        Vector3 localForward = Vector3.forward * (forwardOffset * stepLength);
        Vector3 localUp = Vector3.up * (lift * stepHeight);

        Vector3 targetLocal = restLocal + localForward + localUp;
        foot.localPosition = Vector3.Lerp(foot.localPosition, targetLocal, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

        // keep neutral local rotation
        foot.localRotation = Quaternion.Slerp(foot.localRotation, Quaternion.identity, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
    }

    void ReturnFootToRest(Transform foot, Vector3 restLocal)
    {
        foot.localPosition = Vector3.Lerp(foot.localPosition, restLocal, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        foot.localRotation = Quaternion.Slerp(foot.localRotation, Quaternion.identity, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
    }

    void SendFootEvent(string methodName, UnityEngine.Object footType)
    {
        if (footstepsTarget != null)
        {
            footstepsTarget.SendMessage(methodName, footType, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (leftFoot != null) { Gizmos.color = Color.cyan; Gizmos.DrawSphere(leftFoot.position, 0.02f); }
        if (rightFoot != null) { Gizmos.color = Color.magenta; Gizmos.DrawSphere(rightFoot.position, 0.02f); }
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f,
            $"Stepping: {steppingActive}\nMoveTimer: {movementTimer:0.000}/{movementStartDelay:0.000}\nBasePhase: {basePhase:0.000}");
#endif
    }
}
