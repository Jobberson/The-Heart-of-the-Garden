using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BloomingMirrorsPuzzleManager : PuzzleManagerBase
{
    [Tooltip("Reference to the BeamBounceTracer (assign or auto-find)")]
    [SerializeField] private BeamBounceTracer beamTracer;

    [Tooltip("The tree GameObject that must be hit at the end")]
    [SerializeField] private Collider treeCollider; // assign the tree collider in inspector

    [Tooltip("How many unique mirrors are required")]
    [SerializeField] private int requiredUniqueMirrors = 6;

    [Tooltip("How long the correct state must hold")]
    [SerializeField] private float holdTime = 2f;

    [SerializeField] private UnityEvent onPuzzleSolved;
    [SerializeField] private GameObject mirrorPuzzleTrigger;

    bool _isComplete = false;
    bool _checking = false;

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        if (beamTracer == null) beamTracer = FindFirstObjectByType<BeamBounceTracer>();
#else
        if (beamTracer == null) beamTracer = FindObjectOfType<BeamBounceTracer>();
#endif
        // optional: listen to goal hit (fast reaction)
        if (beamTracer != null)
            beamTracer.onGoalHit?.AddListener(OnTracerGoalHit);

        if (mirrorPuzzleTrigger != null)
            mirrorPuzzleTrigger.SetActive(false);
    }

    void Update()
    {
        if (_isComplete || beamTracer == null) return;

        // quick check: do we have enough unique mirrors AND is last non-mirror hit the tree?
        if (!_checking && ConditionsMetNow())
            StartCoroutine(VerifyStableState());
    }

    // called when tracer reports any goal hit; this gives us a faster reaction
    void OnTracerGoalHit(Collider c)
    {
        if (_isComplete || beamTracer == null) return;
        if (_checking) return;

        if (ConditionsMetNow())
            StartCoroutine(VerifyStableState());
    }

    bool ConditionsMetNow()
    {
        // 1) need last non-mirror to be the tree collider
        if (beamTracer.lastNonMirrorCollider == null) return false;
        if (treeCollider == null) return false;
        if (beamTracer.lastNonMirrorCollider != treeCollider) return false;

        // 2) count unique mirror GameObjects
        var uniqueMirrors = new HashSet<int>();
        foreach (var col in beamTracer.GetHitColliders())
        {
            if (col == null) continue;
            uniqueMirrors.Add(col.gameObject.GetInstanceID());
        }

        return uniqueMirrors.Count >= requiredUniqueMirrors;
    }

    IEnumerator VerifyStableState()
    {
        _checking = true;
        float t = 0f;

        while (t < holdTime)
        {
            if (!ConditionsMetNow())
            {
                _checking = false;
                yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }

        FinalizePuzzle();
    }

    void FinalizePuzzle()
    {
        if (_isComplete) return;
        _isComplete = true;
        Debug.Log("[Puzzle] Blooming mirrors COMPLETE (6 mirrors + tree)");
        onPuzzleSolved?.Invoke();
    }

    // PuzzleManagerBase interface
    public override void InitializePuzzles()
    {
        // ensure awake logic ran / tracer found
        Awake();
    }

    public override void CheckPuzzleStates()
    {
        // external callers can use this
        if (!_isComplete) Update();
    }

    public override bool IsPuzzleComplete()
    {
        return _isComplete;
    }

    public void EnableTrigger()
    {
        if (mirrorPuzzleTrigger != null)
            mirrorPuzzleTrigger.SetActive(true);
    }
    
}
