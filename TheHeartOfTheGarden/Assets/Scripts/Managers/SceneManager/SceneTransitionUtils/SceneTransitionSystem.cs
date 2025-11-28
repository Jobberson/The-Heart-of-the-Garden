using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionSystem : Singleton<SceneTransitionSystem>
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Optional Loading Screen")]
    [SerializeField] private GameObject loadingScreen;

    private bool isTransitioning = false;

    private void Start()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("Fade Canvas Group is not assigned.");
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeAndLoadScene(sceneName));
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isTransitioning = true;

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        yield return StartCoroutine(Fade(1));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return StartCoroutine(Fade(0));
        asyncLoad.allowSceneActivation = true;

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}