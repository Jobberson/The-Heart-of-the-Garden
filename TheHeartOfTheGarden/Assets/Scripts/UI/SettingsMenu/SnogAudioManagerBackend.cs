using UnityEngine;
using Snog.Audio;

public sealed class SnogAudioManagerBackend : MonoBehaviour, IAudioSettingsBackend
{
    [SerializeField]
    private AudioManager _audioManager;

    private float _masterDb;
    private float _musicDb;
    private float _sfxDb;

    private bool _masterMuted;
    private bool _musicMuted;
    private bool _sfxMuted;

    public void Initialize()
    {
        if (_audioManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            _audioManager = FindFirstObjectByType<AudioManager>();
#else
            _audioManager = FindObjectOfType<AudioManager>();
#endif
        }

        if (_audioManager == null)
        {
            // If your Singleton< > creates/accesses an instance via AudioManager.Instance,
            // you can uncomment the line below:
            // _audioManager = AudioManager.Instance;
        }
    }

    public void SetVolumeDb(AudioBus bus, float volumeDb)
    {
        SetCachedDb(bus, volumeDb);
        ApplyBus(bus);
    }

    public float GetVolumeDb(AudioBus bus)
    {
        if (_audioManager == null)
        {
            return 0.0f;
        }

        // Your AudioManager exposes mixer params: MasterVolume, MusicVolume, AmbientVolume, FXVolume [1](https://cargillonline-my.sharepoint.com/personal/pedro_schenegoski_cargill_com/Documents/Microsoft%20Copilot%20Chat%20Files/AudioManager.cs)
        switch (bus)
        {
            case AudioBus.Master:
                return _audioManager.GetMixerVolumeDB("MasterVolume");
            case AudioBus.Music:
                return _audioManager.GetMixerVolumeDB("MusicVolume");
            case AudioBus.Sfx:
                return _audioManager.GetMixerVolumeDB("FXVolume");
        }

        return 0.0f;
    }

    public void SetMuted(AudioBus bus, bool muted)
    {
        SetCachedMute(bus, muted);
        ApplyBus(bus);
    }

    public bool GetMuted(AudioBus bus)
    {
        switch (bus)
        {
            case AudioBus.Master:
                return _masterMuted;
            case AudioBus.Music:
                return _musicMuted;
            case AudioBus.Sfx:
                return _sfxMuted;
        }

        return false;
    }

    private void ApplyBus(AudioBus bus)
    {
        if (_audioManager == null)
        {
            return;
        }

        float db = GetCachedDb(bus);
        bool muted = GetCachedMute(bus);

        float effectiveLinear = muted ? 0.0001f : DbToLinear(db);

        // Your AudioManager expects linear 0..1 and applies the mixer param internally [1](https://cargillonline-my.sharepoint.com/personal/pedro_schenegoski_cargill_com/Documents/Microsoft%20Copilot%20Chat%20Files/AudioManager.cs)
        switch (bus)
        {
            case AudioBus.Master:
                _audioManager.SetVolume(effectiveLinear, AudioManager.AudioChannel.Master);
                break;

            case AudioBus.Music:
                _audioManager.SetVolume(effectiveLinear, AudioManager.AudioChannel.Music);
                break;

            case AudioBus.Sfx:
                _audioManager.SetVolume(effectiveLinear, AudioManager.AudioChannel.FX);
                break;
        }
    }

    private void SetCachedDb(AudioBus bus, float db)
    {
        switch (bus)
        {
            case AudioBus.Master:
                _masterDb = db;
                break;

            case AudioBus.Music:
                _musicDb = db;
                break;

            case AudioBus.Sfx:
                _sfxDb = db;
                break;
        }
    }

    private float GetCachedDb(AudioBus bus)
    {
        switch (bus)
        {
            case AudioBus.Master:
                return _masterDb;

            case AudioBus.Music:
                return _musicDb;

            case AudioBus.Sfx:
                return _sfxDb;
        }

        return 0.0f;
    }

    private void SetCachedMute(AudioBus bus, bool muted)
    {
        switch (bus)
        {
            case AudioBus.Master:
                _masterMuted = muted;
                break;

            case AudioBus.Music:
                _musicMuted = muted;
                break;

            case AudioBus.Sfx:
                _sfxMuted = muted;
                break;
        }
    }

    private bool GetCachedMute(AudioBus bus)
    {
        switch (bus)
        {
            case AudioBus.Master:
                return _masterMuted;

            case AudioBus.Music:
                return _musicMuted;

            case AudioBus.Sfx:
                return _sfxMuted;
        }

        return false;
    }

    private float DbToLinear(float db)
    {
        // Match your AudioManager approach: clamp away from 0 to avoid -Infinity in log conversion [1](https://cargillonline-my.sharepoint.com/personal/pedro_schenegoski_cargill_com/Documents/Microsoft%20Copilot%20Chat%20Files/AudioManager.cs)
        if (db <= -80.0f)
        {
            return 0.0001f;
        }

        return Mathf.Pow(10.0f, db / 20.0f);
    }
}