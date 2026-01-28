using UnityEngine;

public static class GameSettingsStorage
{
    private const string Key = "GAME_SETTINGS_JSON";

    public static void Save(GameSettingsData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    public static GameSettingsData LoadOrDefault()
    {
        if (!PlayerPrefs.HasKey(Key))
        {
            return GameSettingsData.CreateDefault();
        }

        string json = PlayerPrefs.GetString(Key);
        if (string.IsNullOrEmpty(json))
        {
            return GameSettingsData.CreateDefault();
        }

        GameSettingsData data = JsonUtility.FromJson<GameSettingsData>(json);
        if (data == null)
        {
            return GameSettingsData.CreateDefault();
        }

        if (data.Version != GameSettingsData.CurrentVersion)
        {
            if (data.Version < 2)
            {
                data.VolumetricFogEnabled = true;
            }

            data.Version = GameSettingsData.CurrentVersion;
        }

        return data;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
