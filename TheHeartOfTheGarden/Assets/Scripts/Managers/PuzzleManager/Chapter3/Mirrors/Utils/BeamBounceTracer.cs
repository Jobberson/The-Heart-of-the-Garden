using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class BeamBounceTracer : MonoBehaviour
{
    [Header("Setup")]
    public Transform origin;
    public float maxDistance = 100f;
    public int maxBounces = 5;
    public LayerMask mirrorMask;
    public LayerMask obstacleMask;
    public LayerMask goalMask;
    public float surfaceOffset = 0.01f;
    public bool countUniqueMirrors = false;

    [Header("Visuals")]
    public float lineWidth = 0.02f;
    [HideInInspector] public LineRenderer lr;

    [Header("Events (optional)")]
    public UnityEvent<int> onBounce;               // bounce count
    public ColliderEvent onBounceWithCollider;     // fired for mirror bounces
    public ColliderEvent onGoalHit;                // fired when non-mirror (goal) is hit
    public UnityEvent onPuzzleComplete;            // legacy
    [Serializable] public class ColliderEvent : UnityEvent<Collider> {}

    // runtime state
    public int bounceCount { get; private set; }
    public bool puzzleComplete { get; private set; }

    // exposed hit data for other systems to query
    public List<Collider> hitColliders { get; private set; } = new(); // mirrors only
    public Collider lastMirrorCollider => hitColliders.Count > 0 ? hitColliders[^1] : null;

    // new: last collider that was hit which is NOT a mirror (the final target)
    public Collider lastNonMirrorCollider { get; private set; }

    // C# event
    public event Action<Collider,int> OnBounce; // collider, currentBounceCount

    private HashSet<int> _hitMirrorIds;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        bounceCount = 0;
        puzzleComplete = false;
        _hitMirrorIds = new HashSet<int>();
    }

    void Update()
    {
        DrawLaser();
    }

    void DrawLaser()
    {
        // reset state each frame
        List<Vector3> points = new();
        Vector3 originPos = origin.position;
        Vector3 direction = origin.forward;
        points.Add(originPos);

        bounceCount = 0;
        puzzleComplete = false;
        _hitMirrorIds.Clear();
        hitColliders.Clear();
        lastNonMirrorCollider = null;

        for (int i = 0; i < maxBounces; i++)
        {
            Ray ray = new(originPos, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, obstacleMask | mirrorMask))
            {
                points.Add(hit.point);

                bool isMirror = (mirrorMask.value & (1 << hit.collider.gameObject.layer)) != 0;

                if (isMirror)
                {
                    int id = hit.collider.GetInstanceID();
                    if (!countUniqueMirrors || !_hitMirrorIds.Contains(id))
                    {
                        _hitMirrorIds.Add(id);
                        bounceCount++;
                        hitColliders.Add(hit.collider);
                        onBounce?.Invoke(bounceCount);
                        onBounceWithCollider?.Invoke(hit.collider);
                        OnBounce?.Invoke(hit.collider, bounceCount);
                    }

                    // ensure normal faces incoming ray
                    Vector3 normal = hit.normal;
                    if (Vector3.Dot(direction, normal) > 0f) normal = -normal;

                    direction = Vector3.Reflect(direction, normal);
                    originPos = hit.point + direction * surfaceOffset;

                    if (bounceCount >= maxBounces) break;
                    continue;
                }
                else
                {
                    // non-mirror hit: register as potential goal/target
                    lastNonMirrorCollider = hit.collider;

                    bool isGoal = (goalMask.value & (1 << hit.collider.gameObject.layer)) != 0;
                    if (isGoal)
                    {
                        puzzleComplete = true;
                        onGoalHit?.Invoke(hit.collider);
                        onPuzzleComplete?.Invoke();
                    }
                    // stop regardless of whether it's goal or just a wall
                    break;
                }
            }
            else
            {
                points.Add(originPos + direction * maxDistance);
                break;
            }
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
    }

    // helpers
    public bool IsBouncingOff(Collider c) => hitColliders.Contains(c);
    public IEnumerable<Collider> GetHitColliders() => hitColliders.AsReadOnly();
}
