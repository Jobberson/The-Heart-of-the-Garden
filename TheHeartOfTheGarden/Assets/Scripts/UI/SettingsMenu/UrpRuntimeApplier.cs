using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class UrpRuntimeApplier
{
    private UniversalRenderPipelineAsset _runtimeAsset;
    private UniversalRenderPipelineAsset _sourceAsset;

    public UniversalRenderPipelineAsset RuntimeAsset
    {
        get
        {
            return _runtimeAsset;
        }
    }

    public void Initialize()
    {
        RenderPipelineAsset current = GraphicsSettings.currentRenderPipeline;
        _sourceAsset = current as UniversalRenderPipelineAsset;

        if (_sourceAsset == null)
        {
            Debug.LogWarning("URP Runtime Applier: Current Render Pipeline is not URP. URP settings will be ignored.");
            return;
        }

        _runtimeAsset = Object.Instantiate(_sourceAsset);
        _runtimeAsset.name = _sourceAsset.name + " (Runtime Instance)";

        GraphicsSettings.renderPipelineAsset = _runtimeAsset;
        QualitySettings.renderPipeline = _runtimeAsset;
    }

    public void ApplyToPipeline(GameSettingsData data)
    {
        if (_runtimeAsset == null)
        {
            return;
        }

        _runtimeAsset.renderScale = Mathf.Clamp(data.RenderScale, 0.5f, 2.0f);
        _runtimeAsset.msaaSampleCount = SanitizeMsaa(data.MsaaSamples);

        _runtimeAsset.supportsHDR = data.HdrEnabled;

        _runtimeAsset.shadowDistance = data.ShadowsEnabled ? Mathf.Max(0.0f, data.ShadowDistance) : 0.0f;
        _runtimeAsset.supportsMainLightShadows = data.ShadowsEnabled;
        _runtimeAsset.supportsAdditionalLightShadows = data.ShadowsEnabled;
    }

    public void ApplyToCamera(Camera camera, GameSettingsData data)
    {
        if (camera == null)
        {
            return;
        }

        UniversalAdditionalCameraData additional = camera.GetComponent<UniversalAdditionalCameraData>();
        if (additional == null)
        {
            additional = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }

        additional.renderPostProcessing = data.PostProcessingEnabled;

        ApplyCameraAntiAliasing(additional, data.AntiAliasingMode);
    }

    private int SanitizeMsaa(int samples)
    {
        if (samples <= 1)
        {
            return 1;
        }

        if (samples <= 2)
        {
            return 2;
        }

        if (samples <= 4)
        {
            return 4;
        }

        return 8;
    }

    private void ApplyCameraAntiAliasing(UniversalAdditionalCameraData additional, int mode)
    {
        // 0 None
        // 1 FXAA
        // 2 SMAA
        // 3 TAA (if your URP version supports it)
        //
        // Older URP versions might not have TAA. We safely clamp.

        int clamped = Mathf.Clamp(mode, 0, 3);

        if (clamped == 0)
        {
            additional.antialiasing = AntialiasingMode.None;
            return;
        }

        if (clamped == 1)
        {
            additional.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            return;
        }

        if (clamped == 2)
        {
            additional.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            return;
        }

        // TAA support depends on URP version. If not available in your version,
        // this line may not compile. If that happens, remove this case and clamp max to 2.
        additional.antialiasing = AntialiasingMode.TemporalAntiAliasing;
    }
}
