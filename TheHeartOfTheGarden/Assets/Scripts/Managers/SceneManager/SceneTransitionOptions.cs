
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SceneTransitionOptions
{
    [SerializeField] private LoadSceneMode mode = LoadSceneMode.Single;

    [SerializeField] [Range(0.05f, 5f)] private float fadeOutSeconds = 0.5f;
    [SerializeField] [Range(0.05f, 5f)] private float fadeInSeconds = 0.5f;
    [SerializeField] [Range(0f, 3f)] private float minimumBlackSeconds = 0.1f;

    [SerializeField] private bool setActiveOnLoad = true;
    [SerializeField] private bool unloadPrevious = true;
    [SerializeField] private bool blockRaycasts = true;
    [SerializeField] private bool skipFade = false;

    public LoadSceneMode Mode => mode;
    public float FadeOutSeconds => fadeOutSeconds;
    public float FadeInSeconds => fadeInSeconds;
    public float MinimumBlackSeconds => minimumBlackSeconds;
    public bool SetActiveOnLoad => setActiveOnLoad;
    public bool UnloadPrevious => unloadPrevious;
    public bool BlockRaycasts => blockRaycasts;
    public bool SkipFade => skipFade;

    public static SceneTransitionOptions DefaultSingle()
    {
        return new SceneTransitionOptions
        {
            mode = LoadSceneMode.Single,
            setActiveOnLoad = true,
            unloadPrevious = true,
            blockRaycasts = true,
            skipFade = false,
            fadeOutSeconds = 0.5f,
            fadeInSeconds = 0.5f,
            minimumBlackSeconds = 0.1f
        };
    }

    public static SceneTransitionOptions DefaultAdditive()
    {
        return new SceneTransitionOptions
        {
            mode = LoadSceneMode.Additive,
            setActiveOnLoad = true,
            unloadPrevious = true,
            blockRaycasts = true,
            skipFade = false,
            fadeOutSeconds = 0.5f,
            fadeInSeconds = 0.5f,
            minimumBlackSeconds = 0.1f
        };
    }
}
