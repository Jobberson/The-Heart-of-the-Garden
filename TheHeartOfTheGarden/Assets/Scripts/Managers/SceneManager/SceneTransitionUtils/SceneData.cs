using UnityEngine;
using System.Collections.Generic;

public static class SceneData
{
    private static Dictionary<string, object> data = new();

    /// <summary>
    /// Sets a value for the given key.
    /// </summary>
    public static void Set(string key, object value)
    {
        if (data.ContainsKey(key))
        {
            data[key] = value;
        }
        else
        {
            data.Add(key, value);
        }

#if UNITY_EDITOR
        Debug.Log($"[SceneData] Set: {key} = {value}");
#endif
    }

    /// <summary>
    /// Gets a value of type T for the given key.
    /// </summary>
    public static T Get<T>(string key)
    {
        if (data.TryGetValue(key, out var value))
        {
            return (T)value;
        }

        return default;
    }

    /// <summary>
    /// Tries to get a value of type T safely.
    /// </summary>
    public static bool TryGet<T>(string key, out T value)
    {
        if (data.TryGetValue(key, out var obj) && obj is T castValue)
        {
            value = castValue;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Checks if the key exists.
    /// </summary>
    public static bool Has(string key) => data.ContainsKey(key);

    /// <summary>
    /// Removes a specific key.
    /// </summary>
    public static void Remove(string key)
    {
        if (data.ContainsKey(key))
        {
            data.Remove(key);
        }
    }

    /// <summary>
    /// Clears all stored data.
    /// </summary>
    public static void Clear() => data.Clear();

    /// <summary>
    /// Returns all current keys (read-only).
    /// </summary>
    public static IReadOnlyCollection<string> Keys => data.Keys;
}

/* 

EXAMPLE USAGE


1. Set Data Before Scene Transition

SceneData.Set("PlayerName", "Pedro");
SceneData.Set("PuzzleSolvedCount", 3);
SceneData.Set("HasKeyItem", true);

/// Then trigger the scene transition
SceneTransitionSystem.Instance.FadeToScene("Chapter2");



2. Retrieve Data in the New Scene
void Start()
{
    string playerName = SceneData.Get<string>("PlayerName");
    int puzzleCount = SceneData.Get<int>("PuzzleSolvedCount");
    bool hasKeyItem = SceneData.Get<bool>("HasKeyItem");

    Debug.Log($"Welcome back, {playerName}. You solved {puzzleCount} puzzles.");
    if (hasKeyItem)
    {
        Debug.Log("You have the key item!");
    }
}



3. Safe Retrieval with TryGet
if (SceneData.TryGet<int>("PuzzleSolvedCount", out int count))
{
    Debug.Log($"Puzzle count: {count}");
}
else
{
    Debug.Log("Puzzle count not found.");
}



4. Clear or Remove Data
SceneData.Remove("HasKeyItem"); // Remove one key
SceneData.Clear();              // Clear all data



5. Debugging All Keys
foreach (var key in SceneData.Keys)
{
    Debug.Log($"Stored key: {key}");
}

*/