
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A runtime-safe reference to a Scene:
/// - Stores the scene path (string) for runtime load.
/// - In the Editor, exposes a SceneAsset picker and keeps the path in sync.
/// </summary>
[System.Serializable]
public class SceneReference
{
    [SerializeField] private string scenePath;

    // Editor-only: lets you pick the scene from the project
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif

    /// <summary>
    /// Returns the stored scene path (e.g., "Assets/Scenes/PersistentUI.unity").
    /// </summary>
    public string Path => scenePath;

    /// <summary>
    /// Returns the scene name (derived from the path), or empty if not available.
    /// </summary>
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return string.Empty;
            }
            int slash = scenePath.LastIndexOf('/');
            int dot = scenePath.LastIndexOf('.');
            if (slash >= 0 && dot > slash)
            {
                return scenePath.Substring(slash + 1, dot - slash - 1);
            }
            return scenePath;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Keeps scenePath in sync with sceneAsset when changed in the Inspector.
    /// Call from OnValidate().
    /// </summary>
    public void SyncEditorAsset()
    {
        if (sceneAsset == null)
        {
            scenePath = string.Empty;
            return;
        }

        string path = AssetDatabase.GetAssetPath(sceneAsset);
        if (!string.IsNullOrEmpty(path))
        {
            scenePath = path;
        }
    }
#endif

    /// <summary>
    /// True if this reference has a valid path.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(scenePath);
    }
}
