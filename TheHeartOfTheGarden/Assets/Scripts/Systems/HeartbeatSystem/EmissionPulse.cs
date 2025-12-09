
using UnityEngine;
using System.Collections;

/// <summary>
/// Pulses a material's emissive color in sync with lub/dub.
/// Works with URP materials that support _EmissionColor.
/// </summary>
public class EmissionPulse : MonoBehaviour
{
    [Header("Target")]
    public Renderer targetRenderer;

    [Tooltip("Emission color tint.")]
    public Color emissionColor = new Color(1f, 0.2f, 0.2f);

    [Tooltip("Base emissive intensity (multiplied by color).")]
    public float baseEmission = 0f;

    [Header("Lub Envelope")]
    public float lubPeak = 2f;
    [Range(0.05f, 1.0f)]
    public float lubDuration = 0.20f;
    public AnimationCurve lubCurve = AnimationCurve.EaseInOut(0, 0, 0.15f, 1);

    [Header("Dub Envelope")]
    public float dubPeak = 1.3f;
    [Range(0.05f, 1.0f)]
    public float dubDuration = 0.16f;
    public AnimationCurve dubCurve = AnimationCurve.EaseInOut(0, 0, 0.10f, 1);

    [Header("Options")]
    [Tooltip("Multiply peak by per-cycle intensity from the conductor.")]
    public bool useCycleIntensity = true;

    private Material _matInstance;
    private Coroutine _routine;

    private void Awake()
    {
        if (targetRenderer != null)
        {
            _matInstance = new Material(targetRenderer.sharedMaterial);
            targetRenderer.material = _matInstance;

            _matInstance.EnableKeyword("_EMISSION");
            _matInstance.SetColor("_EmissionColor", emissionColor * baseEmission);
        }
    }

    public void TriggerLub(float cycleIntensity = 1f)
    {
        float peak = lubPeak * (useCycleIntensity ? Mathf.Clamp01(cycleIntensity) : 1f);
        TriggerEnvelope(lubCurve, lubDuration, peak);
    }

    public void TriggerDub(float cycleIntensity = 1f)
    {
        float peak = dubPeak * (useCycleIntensity ? Mathf.Clamp01(cycleIntensity) : 1f);
        TriggerEnvelope(dubCurve, dubDuration, peak);
    }

    private void TriggerEnvelope(AnimationCurve curve, float duration, float peak)
    {
        if (_matInstance == null)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }
        _routine = StartCoroutine(PulseRoutine(curve, duration, peak));
    }

    private IEnumerator PulseRoutine(AnimationCurve curve, float duration, float peak)
    {
        float t = 0f;
        while (t < duration)
        {
            float u = t / Mathf.Max(0.0001f, duration);
            float env = Mathf.Max(0f, curve.Evaluate(u)) * peak;
            float value = baseEmission + env;
            _matInstance.SetColor("_EmissionColor", emissionColor * value);

            t += Time.deltaTime;
            yield return null;
        }

        _matInstance.SetColor("_EmissionColor", emissionColor * baseEmission);
        _routine = null;
    }
}
