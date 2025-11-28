using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessManager : Singleton<PostProcessManager>
{

    [Header("Assign your global Volume (or a local one you want to control)")]
    public Volume targetVolume;

    // Cached references to components we touch frequently
    private ColorAdjustments colorAdjustments;
    private Bloom bloom;
    private Vignette vignette;
    private ChromaticAberration chromatic;
    private FilmGrain filmGrain;
    private DepthOfField depthOfField;

    // Running coroutines so we can cancel if another request arrives
    private Dictionary<string, Coroutine> activeCoroutines = new();

    protected override void Awake()
    {
        base.Awake();
        CacheComponents();
    }

    private void OnValidate()
    {
        // keep components cached in editor when possible
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (targetVolume == null) return;

        // TryGet returns the override instances in the profile
        targetVolume.profile?.TryGet(out colorAdjustments);
        targetVolume.profile?.TryGet(out bloom);
        targetVolume.profile?.TryGet(out vignette);
        targetVolume.profile?.TryGet(out chromatic);
        targetVolume.profile?.TryGet(out filmGrain);
        targetVolume.profile?.TryGet(out depthOfField);
    }

    // ----------------------
    // Public API - color/tint
    // ----------------------
    /// <summary>
    /// Apply color tint + saturation change. t is blendAmount (0..1) applied to the target values.
    /// If duration > 0 the change will tween smoothly.
    /// </summary>
    public void ApplyColorTint(Color targetTint, float targetSaturation = -100f, float blendAmount = 1f, float duration = 0f)
    {
        if (targetVolume == null || colorAdjustments == null) { CacheComponents(); if (colorAdjustments == null) return; }

        float clampedBlend = Mathf.Clamp01(blendAmount);
        // desired final values
        float desiredSat = Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, clampedBlend);
        Color desiredCol = Color.Lerp(Color.white, targetTint, clampedBlend);

        string key = "ColorTint";
        StopActive(key);
        if (duration <= 0f)
        {
            colorAdjustments.saturation.value = desiredSat;
            colorAdjustments.colorFilter.value = desiredCol;
        }
        else
        {
            activeCoroutines[key] = StartCoroutine(BlendColorAdjustments(colorAdjustments.saturation.value, colorAdjustments.colorFilter.value, desiredSat, desiredCol, duration, key));
        }
    }

    private IEnumerator BlendColorAdjustments(float startSat, Color startCol, float endSat, Color endCol, float duration, string key)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            colorAdjustments.saturation.value = Mathf.Lerp(startSat, endSat, u);
            colorAdjustments.colorFilter.value = Color.Lerp(startCol, endCol, u);
            yield return null;
        }
        colorAdjustments.saturation.value = endSat;
        colorAdjustments.colorFilter.value = endCol;
        activeCoroutines.Remove(key);
    }

    // ----------------------
    // Public API - Bloom
    // ----------------------
    public void ApplyBloom(float targetIntensity, float targetThreshold, float blendAmount = 1f, float duration = 0f)
    {
        if (targetVolume == null || bloom == null) { CacheComponents(); if (bloom == null) return; }

        float finalIntensity = Mathf.Lerp(bloom.intensity.value, targetIntensity * Mathf.Clamp01(blendAmount), Mathf.Clamp01(blendAmount));
        float finalThreshold = Mathf.Lerp(bloom.threshold.value, targetThreshold, Mathf.Clamp01(blendAmount));

        string key = "Bloom";
        StopActive(key);
        if (duration <= 0f)
        {
            bloom.intensity.value = finalIntensity;
            bloom.threshold.value = finalThreshold;
        }
        else
        {
            activeCoroutines[key] = StartCoroutine(BlendBloom(bloom.intensity.value, bloom.threshold.value, finalIntensity, finalThreshold, duration, key));
        }
    }

    private IEnumerator BlendBloom(float startInt, float startThr, float endInt, float endThr, float duration, string key)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            bloom.intensity.value = Mathf.Lerp(startInt, endInt, u);
            bloom.threshold.value = Mathf.Lerp(startThr, endThr, u);
            yield return null;
        }
        bloom.intensity.value = endInt;
        bloom.threshold.value = endThr;
        activeCoroutines.Remove(key);
    }

    // ----------------------
    // Public API - Vignette
    // ----------------------
    public void ApplyVignette(float targetIntensity, float blendAmount = 1f, float duration = 0f)
    {
        if (targetVolume == null || vignette == null) { CacheComponents(); if (vignette == null) return; }

        float final = Mathf.Lerp(vignette.intensity.value, targetIntensity, Mathf.Clamp01(blendAmount));

        string key = "Vignette";
        StopActive(key);
        if (duration <= 0f)
        {
            vignette.intensity.value = final;
        }
        else
        {
            activeCoroutines[key] = StartCoroutine(BlendFloat(vignette.intensity.value, final, duration, (v) => vignette.intensity.value = v, key));
        }
    }

    // ----------------------
    // Public API - Chromatic Aberration
    // ----------------------
    public void ApplyChromatic(float targetIntensity, float blendAmount = 1f, float duration = 0f)
    {
        if (targetVolume == null || chromatic == null) { CacheComponents(); if (chromatic == null) return; }

        float final = Mathf.Lerp(chromatic.intensity.value, targetIntensity, Mathf.Clamp01(blendAmount));

        string key = "Chromatic";
        StopActive(key);
        if (duration <= 0f)
        {
            chromatic.intensity.value = final;
        }
        else
        {
            activeCoroutines[key] = StartCoroutine(BlendFloat(chromatic.intensity.value, final, duration, (v) => chromatic.intensity.value = v, key));
        }
    }

    // ----------------------
    // Public API - Film Grain
    // ----------------------
    public void ApplyFilmGrain(float targetIntensity, float blendAmount = 1f, float duration = 0f)
    {
        if (targetVolume == null || filmGrain == null) { CacheComponents(); if (filmGrain == null) return; }

        float final = Mathf.Lerp(filmGrain.intensity.value, targetIntensity, Mathf.Clamp01(blendAmount));

        string key = "FilmGrain";
        StopActive(key);
        if (duration <= 0f)
        {
            filmGrain.intensity.value = final;
        }
        else
        {
            activeCoroutines[key] = StartCoroutine(BlendFloat(filmGrain.intensity.value, final, duration, (v) => filmGrain.intensity.value = v, key));
        }
    }

    // ----------------------
    // Public API - Depth of Field
    // ----------------------
    public void ApplyDOFFocus(float targetFocusDistance, float blendAmount = 1f, float duration = 0f)
    {
        if (targetVolume == null || depthOfField == null) { CacheComponents(); if (depthOfField == null) return; }

        float final = Mathf.Lerp(depthOfField.focusDistance.value, targetFocusDistance, Mathf.Clamp01(blendAmount));

        string key = "DOF";
        StopActive(key);
        if (duration <= 0f)
        {
            depthOfField.focusDistance.value = final;
        }
        else
        {
            activeCoroutines[key] = StartCoroutine(BlendFloat(depthOfField.focusDistance.value, final, duration, (v) => depthOfField.focusDistance.value = v, key));
        }
    }

    // ----------------------
    // Generic modify helper
    // ----------------------
    /// <summary>
    /// Modify any VolumeComponent of type T. The modifier will run immediately or be tweened over duration
    /// by you implementing the tween logic inside the provided action (or call Set directly for instant).
    /// Example: Modify<ColorAdjustments>((c) => c.postExposure.value = 1.2f);
    /// </summary>
    public void Modify<T>(Action<T> modifier) where T : VolumeComponent
    {
        if (targetVolume == null) return;
        if (targetVolume.profile == null) return;

        if (targetVolume.profile.TryGet<T>(out var comp) && comp != null)
        {
            modifier.Invoke(comp);
        }
        else
        {
            Debug.LogWarning($"PostProcessManager: requested VolumeComponent of type {typeof(T)} is not present in the assigned Volume profile.");
        }
    }

    // ----------------------
    // Utilities
    // ----------------------
    private IEnumerator BlendFloat(float start, float end, float duration, Action<float> setter, string key)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            setter(Mathf.Lerp(start, end, u));
            yield return null;
        }
        setter(end);
        activeCoroutines.Remove(key);
    }

    private void StopActive(string key)
    {
        if (activeCoroutines.TryGetValue(key, out var c))
        {
            if (c != null) StopCoroutine(c);
            activeCoroutines.Remove(key);
        }
    }

    // small helper to refresh if profile changed at runtime
    public void RefreshProfile(Volume newVolume)
    {
        targetVolume = newVolume;
        CacheComponents();
    }
}
