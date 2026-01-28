using System;
using UnityEngine;

[Serializable]
public sealed class GameSettingsData
{
    public const int CurrentVersion = 2;

    [Header("Meta")]
    public int Version = CurrentVersion;

    [Header("Display")]
    public int ResolutionWidth = 1920;
    public int ResolutionHeight = 1080;
    public int RefreshRateNumerator = 60;
    public int RefreshRateDenominator = 1;
    public FullScreenMode FullScreenMode = FullScreenMode.FullScreenWindow;

    [Header("Performance")]
    public int VSyncCount = 1;
    public int TargetFps = -1;

    [Header("Quality")]
    public int QualityLevel = 0;

    [Header("Audio")]
    public float MasterVolumeDb = 0.0f;
    public float MusicVolumeDb = 0.0f;
    public float SfxVolumeDb = 0.0f;

    public bool MasterMuted = false;
    public bool MusicMuted = false;
    public bool SfxMuted = false;

    [Header("URP Rendering")]
    [Range(0.5f, 2.0f)]
    public float RenderScale = 1.0f;
    public int MsaaSamples = 1;
    public bool HdrEnabled = true;

    [Header("URP Effects")]
    public bool PostProcessingEnabled = true;
    public int AntiAliasingMode = 1;

    [Header("URP Shadows")]
    public bool ShadowsEnabled = true;
    [Range(0.0f, 200.0f)]
    public float ShadowDistance = 50.0f;

    [Header("Gameplay / Scene Effects")]
    public bool VolumetricFogEnabled = true;

    public static GameSettingsData CreateDefault()
    {
        GameSettingsData data = new GameSettingsData();
        data.Version = CurrentVersion;

        Resolution current = Screen.currentResolution;
        data.ResolutionWidth = current.width;
        data.ResolutionHeight = current.height;

        if (current.refreshRateRatio.denominator != 0)
        {
            data.RefreshRateNumerator = (int)current.refreshRateRatio.numerator;
            data.RefreshRateDenominator = (int)current.refreshRateRatio.denominator;
        }
        else
        {
            data.RefreshRateNumerator = 60;
            data.RefreshRateDenominator = 1;
        }

        data.FullScreenMode = FullScreenMode.FullScreenWindow;

        data.VSyncCount = 1;
        data.TargetFps = -1;

        data.QualityLevel = QualitySettings.GetQualityLevel();

        data.MasterVolumeDb = 0.0f;
        data.MusicVolumeDb = 0.0f;
        data.SfxVolumeDb = 0.0f;

        data.MasterMuted = false;
        data.MusicMuted = false;
        data.SfxMuted = false;

        data.RenderScale = 1.0f;
        data.MsaaSamples = 1;
        data.HdrEnabled = true;

        data.PostProcessingEnabled = true;
        data.AntiAliasingMode = 1;

        data.ShadowsEnabled = true;
        data.ShadowDistance = 50.0f;

        data.VolumetricFogEnabled = true;

        return data;
    }
}