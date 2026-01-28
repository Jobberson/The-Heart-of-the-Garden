using UnityEngine;
using UnityEngine.Audio;

public sealed class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance
    {
        get;
        private set;
    }

    [Header("Audio Backend (optional)")]
    [SerializeField] private MonoBehaviour _audioBackendBehaviour;
    private IAudioSettingsBackend _audioBackend;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Mixer parameter names")]
    [SerializeField] private string _masterParam = "MasterVolume";
    [SerializeField] private string _musicParam = "MusicVolume";
    [SerializeField] private string _sfxParam = "SfxVolume";

    [Header("Camera used for URP toggles")]
    [SerializeField] private Camera _targetCamera;
    
    [Header("Scene Toggles")]
    [SerializeField] private GameObject _volumetricFogObject;

    private GameSettingsData _data;
    private UrpRuntimeApplier _urp;

    public GameSettingsData Data
    {
        get
        {
            return _data;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _data = GameSettingsStorage.LoadOrDefault();

        _urp = new UrpRuntimeApplier();
        _urp.Initialize();

        _audioBackend = _audioBackendBehaviour as IAudioSettingsBackend;
        if (_audioBackend != null)
        {
            _audioBackend.Initialize();
        }
    }

    private void Start()
    {
        ApplyAll(_data, true);
    }

    public void ApplyAll(GameSettingsData data, bool save)
    {
        _data = data;

        ApplyDisplay(_data);
        ApplyPerformance(_data);
        ApplyQuality(_data);
        ApplyAudio(_data);
        ApplyUrp(_data);
        ApplySceneToggles(_data);

        if (save)
        {
            GameSettingsStorage.Save(_data);
        }
    }

    public void Save()
    {
        GameSettingsStorage.Save(_data);
    }

    private void ApplyDisplay(GameSettingsData data)
    {
        RefreshRate refreshRate = new RefreshRate();
        refreshRate.numerator = (uint)Mathf.Max(1, data.RefreshRateNumerator);
        refreshRate.denominator = (uint)Mathf.Max(1, data.RefreshRateDenominator);

        Screen.SetResolution(data.ResolutionWidth, data.ResolutionHeight, data.FullScreenMode, refreshRate);
    }

    private void ApplyPerformance(GameSettingsData data)
    {
        QualitySettings.vSyncCount = Mathf.Clamp(data.VSyncCount, 0, 4);
        Application.targetFrameRate = data.TargetFps;
    }

    private void ApplyQuality(GameSettingsData data)
    {
        int max = QualitySettings.names.Length - 1;
        int clamped = Mathf.Clamp(data.QualityLevel, 0, max);

        if (QualitySettings.GetQualityLevel() != clamped)
        {
            QualitySettings.SetQualityLevel(clamped, true);
        }
    }

    private void ApplyAudio(GameSettingsData data)
    {
        if (_audioBackend != null)
        {
            _audioBackend.SetVolumeDb(AudioBus.Master, data.MasterVolumeDb);
            _audioBackend.SetVolumeDb(AudioBus.Music, data.MusicVolumeDb);
            _audioBackend.SetVolumeDb(AudioBus.Sfx, data.SfxVolumeDb);

            _audioBackend.SetMuted(AudioBus.Master, data.MasterMuted);
            _audioBackend.SetMuted(AudioBus.Music, data.MusicMuted);
            _audioBackend.SetMuted(AudioBus.Sfx, data.SfxMuted);

            return;
        }

        if (_audioMixer == null)
        {
            return;
        }

        _audioMixer.SetFloat(_masterParam, data.MasterVolumeDb);
        _audioMixer.SetFloat(_musicParam, data.MusicVolumeDb);
        _audioMixer.SetFloat(_sfxParam, data.SfxVolumeDb);
    }


    private void ApplyUrp(GameSettingsData data)
    {
        if (_urp != null)
        {
            _urp.ApplyToPipeline(data);
        }

        if (_targetCamera != null)
        {
            _urp.ApplyToCamera(_targetCamera, data);
        }
    }

    public void SetTargetCamera(Camera camera)
    {
        _targetCamera = camera;
        if (_targetCamera != null)
        {
            _urp.ApplyToCamera(_targetCamera, _data);
        }
    }

    private void ApplySceneToggles(GameSettingsData data)
    {
        if (_volumetricFogObject == null)
        {
            return;
        }

        _volumetricFogObject.SetActive(data.VolumetricFogEnabled);
    }
}
