
using UnityEngine;
using System.Collections;

/// <summary>
/// Pulses a Light's intensity for lub and dub, each with its own envelope.
/// Hooked by HeartBeatConductor per beat.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightPulse : MonoBehaviour
{
    [Header("Target Light")]
    [SerializeField] private Light targetLight;

    [Tooltip("Base intensity the light returns to between pulses.")]
    [SerializeField] private float baseIntensity = 0.0f;

    [Header("Lub (first beat) envelope")]
    [Tooltip("Peak intensity added on top of base for lub.")]
    [SerializeField] private float lubPeak = 2.0f;

    [Tooltip("Duration of the lub envelope, seconds.")]
    [Range(0.05f, 1.0f)]
    [SerializeField] private float lubDuration = 0.20f;

    [Tooltip("Curve for intensity over time (0..1) for lub. Value multiplies lubPeak.")]
    [SerializeField] private AnimationCurve lubCurve = AnimationCurve.EaseInOut(0, 0, 0.15f, 1);

    [Header("Dub (second beat) envelope")]
    [Tooltip("Peak intensity added on top of base for dub.")]
    [SerializeField] private float dubPeak = 1.3f;

    [Tooltip("Duration of the dub envelope, seconds.")]
    [Range(0.05f, 1.0f)]
    [SerializeField] private float dubDuration = 0.16f;

    [Tooltip("Curve for intensity over time (0..1) for dub. Value multiplies dubPeak.")]
    [SerializeField] private AnimationCurve dubCurve = AnimationCurve.EaseInOut(0, 0, 0.1f, 1);

    [Header("Options")]
    [Tooltip("Multiply peak by per-cycle intensity from the conductor.")]
    [SerializeField] private bool useCycleIntensity = true;

    [Tooltip("If true, a new beat interrupts the current envelope; otherwise envelopes layer additively.")]
    [SerializeField] private bool interruptOnNewBeat = true;

    [Tooltip("Clamp final intensity to avoid blinding.")]
    [SerializeField] private float maxIntensityClamp = 10f;

    private Coroutine _pulseRoutine;
    private float _additiveEnvelope; 

    private void Reset()
    {
        targetLight = GetComponent<Light>();
        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
        }
    }

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }
        if (targetLight != null)
        {
            targetLight.intensity = baseIntensity;
        }
    }

    /// <summary>
    /// Trigger the lub pulse (first beat).
    /// cycleIntensity typically comes from the conductor's intensityCurve (0..1).
    /// </summary>
    public void TriggerLub(float cycleIntensity = 1f)
    {
        float peak = lubPeak * (useCycleIntensity ? Mathf.Clamp01(cycleIntensity) : 1f);
        TriggerEnvelope(lubCurve, lubDuration, peak);
    }

    /// <summary>
    /// Trigger the dub pulse (second beat).
    /// </summary>
    public void TriggerDub(float cycleIntensity = 1f)
    {
        float peak = dubPeak * (useCycleIntensity ? Mathf.Clamp01(cycleIntensity) : 1f);
        TriggerEnvelope(dubCurve, dubDuration, peak);
    }

    private void TriggerEnvelope(AnimationCurve curve, float duration, float peak)
    {
        if (targetLight == null)
        {
            return;
        }

        if (interruptOnNewBeat)
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
                _pulseRoutine = null;
            }
            _pulseRoutine = StartCoroutine(PulseRoutine(curve, duration, peak));
        }
        else
        {
            StartCoroutine(AdditivePulseRoutine(curve, duration, peak));
        }
    }

    private IEnumerator PulseRoutine(AnimationCurve curve, float duration, float peak)
    {
        float t = 0f;
        while (t < duration)
        {
            float u = t / Mathf.Max(0.0001f, duration);
            float env = Mathf.Max(0f, curve.Evaluate(u)) * peak;
            float final = Mathf.Min(maxIntensityClamp, baseIntensity + env + _additiveEnvelope);
            targetLight.intensity = final;

            t += Time.deltaTime;
            yield return null;
        }

        targetLight.intensity = Mathf.Min(maxIntensityClamp, baseIntensity + _additiveEnvelope);
        _pulseRoutine = null;
    }

    private IEnumerator AdditivePulseRoutine(AnimationCurve curve, float duration, float peak)
    {
        float t = 0f;
        while (t < duration)
        {
            float u = t / Mathf.Max(0.0001f, duration);
            float env = Mathf.Max(0f, curve.Evaluate(u)) * peak;
            _additiveEnvelope = env;

            if (_pulseRoutine == null && targetLight != null)
            {
                float final = Mathf.Min(maxIntensityClamp, baseIntensity + _additiveEnvelope);
                targetLight.intensity = final;
            }

            t += Time.deltaTime;
            yield return null;
        }

        _additiveEnvelope = 0f;
        if (_pulseRoutine == null && targetLight != null)
        {
            targetLight.intensity = baseIntensity;
        }
    }
}
