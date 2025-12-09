
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives a lub–dub heartbeat pattern:
/// - Audio scheduled on the DSP timeline for tight sync
/// - Two ParticleSystems burst on each beat (Emit())
/// - Optional LightPulse and EmissionPulse triggered per beat
/// Unity 6 (6000.0.62f2), URP, PC
/// </summary>
public class HeartBeatConductor : MonoBehaviour
{
    [Header("Tempo")]
    [Tooltip("Heart cycles per minute. One cycle = lub-dub.")]
    [Range(20f, 180f)]
    [SerializeField] private float bpm = 72f;

    [Tooltip("Second beat (dub) offset inside the cycle: cycleDuration * dubFraction.")]
    [Range(0.1f, 0.6f)]
    [SerializeField] private float dubFraction = 0.32f;

    [Header("Scheduling")]
    [Tooltip("Lead time for audio scheduling on the DSP timeline.")]
    [Range(0.01f, 0.20f)]
    [SerializeField] private double audioLeadTime = 0.05;

    [Tooltip("Warmup delay before first cycle starts—helps scheduling.")]
    [Range(0.0, 1.0)]
    [SerializeField] private double startDelay = 0.20;

    [Header("Visuals (Particles)")]
    [Tooltip("Particles for the first beat (lub). Emission should be disabled; we will Emit().")]
    [SerializeField] private ParticleSystem beatA;

    [Tooltip("Particles for the second beat (dub). Emission should be disabled; we will Emit().")]
    [SerializeField] private ParticleSystem beatB;

    [Tooltip("How many particles to emit on lub.")]
    [Range(1, 500)]
    [SerializeField] private int particlesA = 80;

    [Tooltip("How many particles to emit on dub.")]
    [Range(1, 500)]
    [SerializeField] private int particlesB = 55;

    [Header("Per-cycle Intensity")]
    [Tooltip("0..1 intensity per cycle; use to create slow 'breathing' swells. If empty, defaults to constant 1.")]
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Header("Audio (lub/dub)")]
    [SerializeField] private AudioSource lubSrc;
    [SerializeField] private AudioSource dubSrc;

    [Tooltip("Base volume multiplied by per-cycle intensity for lub.")]
    [Range(0f, 1f)]
    [SerializeField] private float baseVolumeLub = 0.8f;

    [Tooltip("Base volume multiplied by per-cycle intensity for dub.")]
    [Range(0f, 1f)]
    [SerializeField] private float baseVolumeDub = 0.6f;

    [Header("Light / Emission (optional)")]
    [Tooltip("Optional light pulse component; intensity lerps per beat.")]
    [SerializeField] private LightPulse lightPulse;

    [Tooltip("Optional material emission pulse; glows per beat.")]
    [SerializeField] private EmissionPulse emissionPulse;

    [Header("Debug")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool gizmoPulse = true;

    private double _nextCycleDSP;
    private double _cycleDuration;
    private int _cycleIndex = 0;

    private struct VisualEvent
    {
        public double dspTime;
        public bool isLub;
    }

    private readonly List<VisualEvent> _visualEvents = new List<VisualEvent>(64);
    private Queue<float> _tapTimes;

    private void OnValidate()
    {
        bpm = Mathf.Clamp(bpm, 20f, 180f);
        dubFraction = Mathf.Clamp(dubFraction, 0.1f, 0.6f);
        audioLeadTime = Mathf.Clamp(audioLeadTime, 0.01, 0.20);
        startDelay = Mathf.Clamp(startDelay, 0.0, 1.0);
    }

    private void Start()
    {
        _cycleDuration = 60.0 / bpm;
        _nextCycleDSP = AudioSettings.dspTime + startDelay;

        if (playOnStart)
        {
            ScheduleCycle(_nextCycleDSP, _cycleIndex);
        }
    }

    private void Update()
    {
        double newCycle = 60.0 / bpm;
        if (Mathf.Abs((float)(newCycle - _cycleDuration)) > 0.0001f)
        {
            _cycleDuration = newCycle;
        }

        double now = AudioSettings.dspTime;

        for (int i = _visualEvents.Count - 1; i >= 0; i--)
        {
            if (now >= _visualEvents[i].dspTime)
            {
                float cycleIntensity = EvaluateIntensity(_cycleIndex);

                if (_visualEvents[i].isLub)
                {
                    EmitBurst(beatA, particlesA, _cycleIndex);
                    TriggerLightAndEmissionLub(cycleIntensity);
                }
                else
                {
                    EmitBurst(beatB, particlesB, _cycleIndex);
                    TriggerLightAndEmissionDub(cycleIntensity);
                }

                _visualEvents.RemoveAt(i);
            }
        }

        while (now + audioLeadTime >= _nextCycleDSP)
        {
            _cycleIndex++;
            _nextCycleDSP += _cycleDuration;
            ScheduleCycle(_nextCycleDSP, _cycleIndex);
        }
    }

    private void ScheduleCycle(double cycleStartDSP, int cycleIndex)
    {
        double dubDSP = cycleStartDSP + (_cycleDuration * dubFraction);
        float intensity = EvaluateIntensity(cycleIndex);

        if (lubSrc != null && lubSrc.clip != null)
        {
            lubSrc.volume = baseVolumeLub * intensity;
            lubSrc.PlayScheduled(cycleStartDSP);
        }

        if (dubSrc != null && dubSrc.clip != null)
        {
            dubSrc.volume = baseVolumeDub * intensity;
            dubSrc.PlayScheduled(dubDSP);
        }

        _visualEvents.Add(new VisualEvent
        {
            dspTime = cycleStartDSP,
            isLub = true
        });

        _visualEvents.Add(new VisualEvent
        {
            dspTime = dubDSP,
            isLub = false
        });
    }

    private float EvaluateIntensity(int cycleIndex)
    {
        if (intensityCurve == null || intensityCurve.length == 0)
        {
            return 1f;
        }

        float t = (cycleIndex % 256) / 256f;
        return Mathf.Clamp01(intensityCurve.Evaluate(t));
    }

    private void EmitBurst(ParticleSystem system, int count, int cycleIndex)
    {
        if (system == null || count <= 0)
        {
            return;
        }

        float intensity = EvaluateIntensity(cycleIndex);
        int scaled = Mathf.Max(1, Mathf.RoundToInt(count * intensity));
        system.Emit(scaled);
    }

    private void TriggerLightAndEmissionLub(float cycleIntensity)
    {
        if (lightPulse != null)
        {
            lightPulse.TriggerLub(cycleIntensity);
        }

        if (emissionPulse != null)
        {
            emissionPulse.TriggerLub(cycleIntensity);
        }
    }

    private void TriggerLightAndEmissionDub(float cycleIntensity)
    {
        if (lightPulse != null)
        {
            lightPulse.TriggerDub(cycleIntensity);
        }

        if (emissionPulse != null)
        {
            emissionPulse.TriggerDub(cycleIntensity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!gizmoPulse)
        {
            return;
        }

        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        float r = 0.15f + 0.05f * Mathf.Sin(Time.time * (float)bpm / 60f * Mathf.PI * 2f);
        Gizmos.DrawWireSphere(pos, r);
    }

    /// <summary>
    /// Tap tempo helper: call from a UI button to set BPM by tapping.
    /// Stores last few taps and updates bpm.
    /// </summary>
    public void TapTempo()
    {
        const int buffer = 4;
        if (_tapTimes == null)
        {
            _tapTimes = new Queue<float>(buffer);
        }

        float now = Time.unscaledTime;
        if (_tapTimes.Count == buffer)
        {
            _tapTimes.Dequeue();
        }
        _tapTimes.Enqueue(now);

        if (_tapTimes.Count >= 2)
        {
            float[] arr = _tapTimes.ToArray();
            float sumIntervals = 0f;
            for (int i = 1; i < arr.Length; i++)
            {
                sumIntervals += arr[i] - arr[i - 1];
            }
            float avgInterval = sumIntervals / (arr.Length - 1);
            bpm = Mathf.Clamp(60f / Mathf.Max(0.1f, avgInterval), 20f, 180f);
        }
    }
}
