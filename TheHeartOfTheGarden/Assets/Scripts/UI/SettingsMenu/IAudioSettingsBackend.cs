using UnityEngine;

public enum AudioBus
{
    Master,
    Music,
    Sfx
}

public interface IAudioSettingsBackend
{
    void Initialize();

    void SetVolumeDb(AudioBus bus, float volumeDb);

    float GetVolumeDb(AudioBus bus);

    void SetMuted(AudioBus bus, bool muted);

    bool GetMuted(AudioBus bus);
}
