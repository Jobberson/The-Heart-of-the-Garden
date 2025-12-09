
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Persistent Scene Controller with fade, async load, additive layering, preloading, and safe API.
/// Uses SceneReference instead of raw strings for persistent scenes.
/// Unity 6 (6000.0.62f2), URP, PC.
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Fader")]
    [SerializeField] private ScreenFader fader;

    [Header("Persistent Scenes")]
    [Tooltip("Scenes loaded once and kept alive (e.g., PersistentUI, PersistentAudio).")]
    [SerializeField] private SceneReference[] persistentScenes;

    [SerializeField] private bool loadPersistentsOnAwake = true;

    [Header("Defaults")]
    [SerializeField] private SceneTransitionOptions defaultSingle = SceneTransitionOptions.DefaultSingle();
    [SerializeField] private SceneTransitionOptions defaultAdditive = SceneTransitionOptions.DefaultAdditive();

    [Header("Busy Guard")]
    [SerializeField] private bool blockIfBusy = true;

    public bool IsBusy => _busy;
    public string ActiveSceneName => SceneManager.GetActiveScene().name;
    public SceneTransitionOptions DefaultSingle => defaultSingle;
    public SceneTransitionOptions DefaultAdditive => defaultAdditive;

    public event Action<string> OnFadeOutStarted;
    public event Action<string, float> OnLoadProgress;
    public event Action<string> OnSceneActivated;
    public event Action<string> OnFadeInCompleted;

    private bool _busy;
    private readonly HashSet<string> _loadedScenes = new HashSet<string>();   // track by scene name
    private readonly HashSet<string> _persistentSet = new HashSet<string>();  // names of persistent scenes
    private readonly Queue<IEnumerator> _transitionQueue = new Queue<IEnumerator>();

    private class PreloadedScene
    {
        public string sceneName;
        public AsyncOperation op;
        public LoadSceneMode mode;
        public bool isReady;
    }

    private readonly Dictionary<string, PreloadedScene> _preloads = new Dictionary<string, PreloadedScene>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Keep SceneReference paths in sync when edited in the Inspector
        if (persistentScenes != null)
        {
            foreach (var sr in persistentScenes)
            {
                sr?.SyncEditorAsset();
            }
        }
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build the persistent set (by scene name)
        _persistentSet.Clear();
        if (persistentScenes != null)
        {
            foreach (var sr in persistentScenes)
            {
                if (sr != null && sr.IsValid())
                {
                    string name = sr.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        _persistentSet.Add(name);
                    }
                }
            }
        }

        if (loadPersistentsOnAwake && _persistentSet.Count > 0)
        {
            StartCoroutine(LoadPersistentScenesRoutine());
        }
    }

    private IEnumerator LoadPersistentScenesRoutine()
    {
        foreach (var name in _persistentSet)
        {
            if (_loadedScenes.Contains(name))
            {
                continue;
            }

            AsyncOperation op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            while (op != null && !op.isDone)
            {
                OnLoadProgress?.Invoke(name, op.progress);
                yield return null;
            }

            Scene sc = SceneManager.GetSceneByName(name);
            if (sc.IsValid())
            {
                _loadedScenes.Add(name);
            }
        }
    }

    public void TransitionTo(string sceneName, SceneTransitionOptions options = null, Action onCompleted = null)
    {
        if (_busy && blockIfBusy)
        {
            Debug.LogWarning("[SceneController] Transition requested while busy. Ignored.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneController] TransitionTo called with empty scene name.");
            return;
        }

        options ??= defaultSingle;
        IEnumerator routine = TransitionRoutine(sceneName, options, onCompleted);
        EnqueueOrRun(routine);
    }

    public void TransitionTo(SceneReference sceneRef, SceneTransitionOptions options = null, Action onCompleted = null)
    {
        if (sceneRef == null || !sceneRef.IsValid())
        {
            Debug.LogWarning("[SceneController] TransitionTo called with invalid SceneReference.");
            return;
        }
        TransitionTo(sceneRef.Name, options, onCompleted);
    }

    public void QueueTransition(string sceneName, SceneTransitionOptions options = null, Action onCompleted = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneController] QueueTransition called with empty scene name.");
            return;
        }

        options ??= defaultSingle;
        EnqueueOrRun(TransitionRoutine(sceneName, options, onCompleted));
    }

    public void QueueTransition(SceneReference sceneRef, SceneTransitionOptions options = null, Action onCompleted = null)
    {
        if (sceneRef == null || !sceneRef.IsValid())
        {
            Debug.LogWarning("[SceneController] QueueTransition called with invalid SceneReference.");
            return;
        }
        QueueTransition(sceneRef.Name, options, onCompleted);
    }

    public void PreloadAdditive(string sceneName, Action<float> onProgress = null, Action onReady = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneController] PreloadAdditive called with empty scene name.");
            return;
        }

        if (_preloads.ContainsKey(sceneName))
        {
            Debug.Log("[SceneController] Scene already preloading/preloaded: " + sceneName);
            return;
        }

        StartCoroutine(PreloadRoutine(sceneName, onProgress, onReady));
    }

    public void PreloadAdditive(SceneReference sceneRef, Action<float> onProgress = null, Action onReady = null)
    {
        if (sceneRef == null || !sceneRef.IsValid())
        {
            Debug.LogWarning("[SceneController] PreloadAdditive called with invalid SceneReference.");
            return;
        }
        PreloadAdditive(sceneRef.Name, onProgress, onReady);
    }

    public void ActivatePreloaded(string sceneName, bool setActive = true, bool unloadPrevious = true, Action onCompleted = null)
    {
        if (!_preloads.TryGetValue(sceneName, out var pre))
        {
            Debug.LogWarning("[SceneController] No preloaded scene found: " + sceneName);
            return;
        }

        if (pre.op == null)
        {
            Debug.LogWarning("[SceneController] Preload operation missing for: " + sceneName);
            return;
        }

        IEnumerator routine = ActivatePreloadedRoutine(pre, setActive, unloadPrevious, onCompleted);
        EnqueueOrRun(routine);
    }

    public void ActivatePreloaded(SceneReference sceneRef, bool setActive = true, bool unloadPrevious = true, Action onCompleted = null)
    {
        if (sceneRef == null || !sceneRef.IsValid())
        {
            Debug.LogWarning("[SceneController] ActivatePreloaded called with invalid SceneReference.");
            return;
        }
        ActivatePreloaded(sceneRef.Name, setActive, unloadPrevious, onCompleted);
    }

    public void UnloadAdditive(string sceneName, bool force = false, Action onCompleted = null)
    {
        if (_persistentSet.Contains(sceneName) && !force)
        {
            Debug.Log("[SceneController] Not unloading persistent scene: " + sceneName);
            return;
        }

        IEnumerator routine = UnloadRoutine(sceneName, onCompleted);
        EnqueueOrRun(routine);
    }

    public void UnloadAdditive(SceneReference sceneRef, bool force = false, Action onCompleted = null)
    {
        if (sceneRef == null || !sceneRef.IsValid())
        {
            Debug.LogWarning("[SceneController] UnloadAdditive called with invalid SceneReference.");
            return;
        }
        UnloadAdditive(sceneRef.Name, force, onCompleted);
    }

    private void EnqueueOrRun(IEnumerator routine)
    {
        if (_busy)
        {
            _transitionQueue.Enqueue(routine);
        }
        else
        {
            StartCoroutine(RunWithQueue(routine));
        }
    }

    private IEnumerator RunWithQueue(IEnumerator routine)
    {
        _busy = true;
        yield return StartCoroutine(routine);

        while (_transitionQueue.Count > 0)
        {
            IEnumerator next = _transitionQueue.Dequeue();
            yield return StartCoroutine(next);
        }

        _busy = false;
    }

    private IEnumerator TransitionRoutine(string sceneName, SceneTransitionOptions options, Action onCompleted)
    {
        Scene current = SceneManager.GetActiveScene();

        if (!options.SkipFade && fader != null)
        {
            OnFadeOutStarted?.Invoke(sceneName);
            fader.FadeOut(options.FadeOutSeconds);
            yield return new WaitForSecondsRealtime(options.FadeOutSeconds);
        }

        float blackStart = Time.unscaledTime;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, options.Mode);
        if (op == null)
        {
            Debug.LogError($"[SceneController] Failed to start async load for scene '{sceneName}'.");
            yield break;
        }

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            OnLoadProgress?.Invoke(sceneName, op.progress);
            yield return null;
        }

        if (options.Mode == LoadSceneMode.Additive)
        {
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && options.SetActiveOnLoad)
            {
                SceneManager.SetActiveScene(newScene);
                OnSceneActivated?.Invoke(sceneName);
            }

            if (options.UnloadPrevious && current.IsValid() && !_persistentSet.Contains(current.name))
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(current);
                while (unload != null && !unload.isDone)
                {
                    yield return null;
                }

                _loadedScenes.Remove(current.name);
            }

            _loadedScenes.Add(sceneName);
        }
        else
        {
            OnSceneActivated?.Invoke(sceneName);
        }

        if (!options.SkipFade)
        {
            float elapsedBlack = Time.unscaledTime - blackStart;
            float remaining = options.MinimumBlackSeconds - elapsedBlack;
            if (remaining > 0f)
            {
                yield return new WaitForSecondsRealtime(remaining);
            }
        }

        if (!options.SkipFade && fader != null)
        {
            fader.FadeIn(options.FadeInSeconds);
            yield return new WaitForSecondsRealtime(options.FadeInSeconds);
        }

        OnFadeInCompleted?.Invoke(sceneName);
        onCompleted?.Invoke();
    }

    private IEnumerator PreloadRoutine(string sceneName, Action<float> onProgress, Action onReady)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            Debug.LogError($"[SceneController] Failed to start preload for scene '{sceneName}'.");
            yield break;
        }

        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            onProgress?.Invoke(op.progress);
            yield return null;
        }

        onProgress?.Invoke(0.9f);

        _preloads[sceneName] = new PreloadedScene
        {
            sceneName = sceneName,
            op = op,
            mode = LoadSceneMode.Additive,
            isReady = true
        };

        onReady?.Invoke();
    }

    private IEnumerator ActivatePreloadedRoutine(PreloadedScene pre, bool setActive, bool unloadPrevious, Action onCompleted)
    {
        Scene current = SceneManager.GetActiveScene();

        if (fader != null)
        {
            OnFadeOutStarted?.Invoke(pre.sceneName);
            fader.FadeOut(defaultAdditive.FadeOutSeconds);
            yield return new WaitForSecondsRealtime(defaultAdditive.FadeOutSeconds);
        }

        float blackStart = Time.unscaledTime;

        pre.op.allowSceneActivation = true;
        while (!pre.op.isDone)
        {
            OnLoadProgress?.Invoke(pre.sceneName, Mathf.Lerp(0.9f, 1f, pre.op.progress));
            yield return null;
        }

        Scene newScene = SceneManager.GetSceneByName(pre.sceneName);
        if (newScene.IsValid() && setActive)
        {
            SceneManager.SetActiveScene(newScene);
            OnSceneActivated?.Invoke(pre.sceneName);
        }

        if (unloadPrevious && current.IsValid() && !_persistentSet.Contains(current.name))
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(current);
            while (unload != null && !unload.isDone)
            {
                yield return null;
            }

            _loadedScenes.Remove(current.name);
        }

        _loadedScenes.Add(pre.sceneName);
        _preloads.Remove(pre.sceneName);

        float elapsedBlack = Time.unscaledTime - blackStart;
        float remaining = defaultAdditive.MinimumBlackSeconds - elapsedBlack;
        if (remaining > 0f)
        {
            yield return new WaitForSecondsRealtime(remaining);
        }

        if (fader != null)
        {
            fader.FadeIn(defaultAdditive.FadeInSeconds);
            yield return new WaitForSecondsRealtime(defaultAdditive.FadeInSeconds);
        }

        OnFadeInCompleted?.Invoke(pre.sceneName);
        onCompleted?.Invoke();
    }

    private IEnumerator UnloadRoutine(string sceneName, Action onCompleted)
    {
        Scene sc = SceneManager.GetSceneByName(sceneName);
        if (!sc.IsValid())
        {
            Debug.LogWarning("[SceneController] Unload requested for invalid scene: " + sceneName);
            yield break;
        }

        AsyncOperation op = SceneManager.UnloadSceneAsync(sc);
        while (op != null && !op.isDone)
        {
            yield return null;
        }

        _loadedScenes.Remove(sceneName);
        onCompleted?.Invoke();
    }
}
