using System.Collections.Generic;
using Dynamite3D.RealIvy;
using UnityEngine;

public class GrowVines : MonoBehaviour
{
    [SerializeField] private List<IvyController> ivyControllers = new();
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player") && TryGetComponent(out PlayerTriggerDetector _)) return;

        triggered = true;

        for (int i = 0; i < ivyControllers.Count; i++)
        {
            var ivy = ivyControllers[i];
            if (ivy == null) continue;

            int idx = i;                         // local copy if you need the index
            float delay = idx;              // stagger example
            string timerId = $"GrowVine_{idx}_{GetInstanceID()}"; // unique ID for this instance

            TimerManager.Instance.CreateTimer(
                timerId,
                delay,
                false, // one-shot (probably what you want)
                () =>
                {
                    Debug.Log($"[{name}] Timer fired: {timerId} -> StartGrowth on ivy #{idx}");
                    ivy.StartGrowth();
                }
            );
        }
    }
}
