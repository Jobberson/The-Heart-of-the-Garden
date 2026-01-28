using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class SettingsMenuUI : MonoBehaviour
{
    [Header("Display")]
    [SerializeField]
    private TMP_Dropdown _resolutionDropdown;
    [SerializeField]
    private TMP_Dropdown _fullscreenDropdown;

    [Header("Performance")]
    [SerializeField]
    private TMP_Dropdown _vsyncDropdown;
    [SerializeField]
    private TMP_InputField _targetFpsInput;

    [Header("Quality")]
    [SerializeField]
    private TMP_Dropdown _qualityDropdown;

    [Header("Audio")]
    [SerializeField]
    private Slider _masterSlider;
    [SerializeField]
    private Slider _musicSlider;
    [SerializeField]
    private Slider _sfxSlider;

    [Header("URP")]
    [SerializeField]
    private Slider _renderScaleSlider;
    [SerializeField]
    private TMP_Dropdown _msaaDropdown;
    [SerializeField]
    private Toggle _hdrToggle;

    [Header("URP Effects")]
    [SerializeField]
    private Toggle _postProcessingToggle;
    [SerializeField]
    private TMP_Dropdown _aaDropdown;

    [Header("URP Shadows")]
    [SerializeField]
    private Toggle _shadowsToggle;
    [SerializeField]
    private Slider _shadowDistanceSlider;

    [Header("Buttons")]
    [SerializeField]
    private Button _applyButton;
    [SerializeField]
    private Button _revertButton;
    [SerializeField]
    private Button _defaultsButton;

    [Header("Scene Effects")]
    [SerializeField] private Toggle _volumetricFogToggle;

    private readonly List<Resolution> _resolutions = new List<Resolution>();
    private GameSettingsData _working;

    private void Start()
    {
        _working = Clone(GameSettingsManager.Instance.Data);

        BuildResolutionOptions();
        BuildFullscreenOptions();
        BuildVsyncOptions();
        BuildQualityOptions();
        BuildMsaaOptions();
        BuildAaOptions();

        SyncUIFromWorking();

        HookupEvents();
    }

    private void HookupEvents()
    {
        _applyButton.onClick.AddListener(OnApply);
        _revertButton.onClick.AddListener(OnRevert);
        _defaultsButton.onClick.AddListener(OnDefaults);

        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        _fullscreenDropdown.onValueChanged.AddListener(OnFullscreenChanged);

        _vsyncDropdown.onValueChanged.AddListener(OnVsyncChanged);
        _targetFpsInput.onEndEdit.AddListener(OnTargetFpsChanged);

        _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        _masterSlider.onValueChanged.AddListener(OnMasterChanged);
        _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        _renderScaleSlider.onValueChanged.AddListener(OnRenderScaleChanged);
        _msaaDropdown.onValueChanged.AddListener(OnMsaaChanged);
        _hdrToggle.onValueChanged.AddListener(OnHdrChanged);

        _postProcessingToggle.onValueChanged.AddListener(OnPostProcessingChanged);
        _aaDropdown.onValueChanged.AddListener(OnAaChanged);

        _shadowsToggle.onValueChanged.AddListener(OnShadowsChanged);
        _shadowDistanceSlider.onValueChanged.AddListener(OnShadowDistanceChanged);

        _volumetricFogToggle.onValueChanged.AddListener(OnVolumetricFogChanged);
    }

    private void BuildResolutionOptions()
    {
        _resolutions.Clear();
        _resolutions.AddRange(Screen.resolutions);

        _resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < _resolutions.Count; i++)
        {
            Resolution r = _resolutions[i];

            int hz = 60;
            if (r.refreshRateRatio.denominator != 0)
            {
                hz = Mathf.RoundToInt((float)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator);
            }

            options.Add(r.width + "x" + r.height + " @" + hz + "Hz");
        }

        _resolutionDropdown.AddOptions(options);
    }

    private void BuildFullscreenOptions()
    {
        _fullscreenDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("Exclusive Fullscreen");
        options.Add("Fullscreen Window");
        options.Add("Maximized Window");
        options.Add("Windowed");

        _fullscreenDropdown.AddOptions(options);
    }

    private void BuildVsyncOptions()
    {
        _vsyncDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("Off");
        options.Add("On");
        options.Add("2");
        options.Add("3");
        options.Add("4");

        _vsyncDropdown.AddOptions(options);
    }

    private void BuildQualityOptions()
    {
        _qualityDropdown.ClearOptions();

        List<string> options = new List<string>(QualitySettings.names);
        _qualityDropdown.AddOptions(options);
    }

    private void BuildMsaaOptions()
    {
        _msaaDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("Off (1x)");
        options.Add("2x");
        options.Add("4x");
        options.Add("8x");

        _msaaDropdown.AddOptions(options);
    }

    private void BuildAaOptions()
    {
        _aaDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("None");
        options.Add("FXAA");
        options.Add("SMAA");
        options.Add("TAA");

        _aaDropdown.AddOptions(options);
    }

    private void SyncUIFromWorking()
    {
        _resolutionDropdown.value = FindClosestResolutionIndex(_working);
        _resolutionDropdown.RefreshShownValue();

        _fullscreenDropdown.value = FullscreenModeToIndex(_working.FullScreenMode);
        _fullscreenDropdown.RefreshShownValue();

        _vsyncDropdown.value = Mathf.Clamp(_working.VSyncCount, 0, 4);
        _vsyncDropdown.RefreshShownValue();

        _targetFpsInput.text = _working.TargetFps.ToString();

        _qualityDropdown.value = Mathf.Clamp(_working.QualityLevel, 0, QualitySettings.names.Length - 1);
        _qualityDropdown.RefreshShownValue();

        _masterSlider.value = DbToSlider(_working.MasterVolumeDb);
        _musicSlider.value = DbToSlider(_working.MusicVolumeDb);
        _sfxSlider.value = DbToSlider(_working.SfxVolumeDb);

        _renderScaleSlider.value = _working.RenderScale;

        _msaaDropdown.value = MsaaToIndex(_working.MsaaSamples);
        _msaaDropdown.RefreshShownValue();

        _hdrToggle.isOn = _working.HdrEnabled;

        _postProcessingToggle.isOn = _working.PostProcessingEnabled;

        _aaDropdown.value = Mathf.Clamp(_working.AntiAliasingMode, 0, 3);
        _aaDropdown.RefreshShownValue();

        _shadowsToggle.isOn = _working.ShadowsEnabled;
        _shadowDistanceSlider.value = _working.ShadowDistance;

        _volumetricFogToggle.isOn = _working.VolumetricFogEnabled;
    }

    private void OnApply()
    {
        GameSettingsManager.Instance.ApplyAll(_working, true);
    }

    private void OnRevert()
    {
        _working = Clone(GameSettingsManager.Instance.Data);
        SyncUIFromWorking();
    }

    private void OnDefaults()
    {
        _working = GameSettingsData.CreateDefault();
        SyncUIFromWorking();
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= _resolutions.Count)
        {
            return;
        }

        Resolution r = _resolutions[index];
        _working.ResolutionWidth = r.width;
        _working.ResolutionHeight = r.height;

        if (r.refreshRateRatio.denominator != 0)
        {
            _working.RefreshRateNumerator = (int)r.refreshRateRatio.numerator;
            _working.RefreshRateDenominator = (int)r.refreshRateRatio.denominator;
        }
    }

    private void OnFullscreenChanged(int index)
    {
        _working.FullScreenMode = IndexToFullscreenMode(index);
    }

    private void OnVsyncChanged(int index)
    {
        _working.VSyncCount = Mathf.Clamp(index, 0, 4);
    }

    private void OnTargetFpsChanged(string value)
    {
        int fps;
        if (int.TryParse(value, out fps))
        {
            _working.TargetFps = fps;
        }
    }

    private void OnQualityChanged(int index)
    {
        _working.QualityLevel = index;
    }

    private void OnMasterChanged(float sliderValue)
    {
        _working.MasterVolumeDb = SliderToDb(sliderValue);
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnMusicChanged(float sliderValue)
    {
        _working.MusicVolumeDb = SliderToDb(sliderValue);
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnSfxChanged(float sliderValue)
    {
        _working.SfxVolumeDb = SliderToDb(sliderValue);
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnRenderScaleChanged(float value)
    {
        _working.RenderScale = value;
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnMsaaChanged(int index)
    {
        _working.MsaaSamples = IndexToMsaa(index);
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnHdrChanged(bool value)
    {
        _working.HdrEnabled = value;
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnPostProcessingChanged(bool value)
    {
        _working.PostProcessingEnabled = value;
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnAaChanged(int index)
    {
        _working.AntiAliasingMode = index;
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnShadowsChanged(bool value)
    {
        _working.ShadowsEnabled = value;
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnShadowDistanceChanged(float value)
    {
        _working.ShadowDistance = value;
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private void OnVolumetricFogChanged(bool value)
    {
        _working.VolumetricFogEnabled = value;
        GameSettingsManager.Instance.ApplyAll(_working, false);
    }

    private int FindClosestResolutionIndex(GameSettingsData data)
    {
        int bestIndex = 0;
        int bestScore = int.MaxValue;

        for (int i = 0; i < _resolutions.Count; i++)
        {
            Resolution r = _resolutions[i];

            int score = Mathf.Abs(r.width - data.ResolutionWidth) + Mathf.Abs(r.height - data.ResolutionHeight);
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private int FullscreenModeToIndex(FullScreenMode mode)
    {
        if (mode == FullScreenMode.ExclusiveFullScreen)
        {
            return 0;
        }

        if (mode == FullScreenMode.FullScreenWindow)
        {
            return 1;
        }

        if (mode == FullScreenMode.MaximizedWindow)
        {
            return 2;
        }

        return 3;
    }

    private FullScreenMode IndexToFullscreenMode(int index)
    {
        if (index == 0)
        {
            return FullScreenMode.ExclusiveFullScreen;
        }

        if (index == 1)
        {
            return FullScreenMode.FullScreenWindow;
        }

        if (index == 2)
        {
            return FullScreenMode.MaximizedWindow;
        }

        return FullScreenMode.Windowed;
    }

    private int MsaaToIndex(int msaa)
    {
        if (msaa <= 1)
        {
            return 0;
        }

        if (msaa <= 2)
        {
            return 1;
        }

        if (msaa <= 4)
        {
            return 2;
        }

        return 3;
    }

    private int IndexToMsaa(int index)
    {
        if (index == 0)
        {
            return 1;
        }

        if (index == 1)
        {
            return 2;
        }

        if (index == 2)
        {
            return 4;
        }

        return 8;
    }

    private float SliderToDb(float t)
    {
        // Slider expected 0..1
        // Map to -80dB..0dB (typical mixer range)
        t = Mathf.Clamp01(t);
        return Mathf.Lerp(-80.0f, 0.0f, t);
    }

    private float DbToSlider(float db)
    {
        return Mathf.InverseLerp(-80.0f, 0.0f, db);
    }

    private GameSettingsData Clone(GameSettingsData src)
    {
        string json = JsonUtility.ToJson(src);
        return JsonUtility.FromJson<GameSettingsData>(json);
    }
}
