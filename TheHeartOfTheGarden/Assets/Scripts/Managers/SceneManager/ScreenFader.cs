
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image tintImage;

    [Header("Defaults")]
    [SerializeField] [Range(0.05f, 5f)] private float defaultFadeOut = 0.5f;
    [SerializeField] [Range(0.05f, 5f)] private float defaultFadeIn = 0.5f;
    [SerializeField] private Color tintColor = new Color(0f, 0f, 0f, 1f);

    private Coroutine _routine;

    public float DefaultFadeOut => defaultFadeOut;
    public float DefaultFadeIn => defaultFadeIn;
    public float CurrentAlpha => canvasGroup != null ? canvasGroup.alpha : 0f;

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha);

        if (tintImage != null)
        {
            tintImage.color = tintColor;
        }
    }

    public void SetAlpha(float a, bool blockRaycasts = false)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = Mathf.Clamp01(a);
        canvasGroup.blocksRaycasts = blockRaycasts;
        canvasGroup.interactable = blockRaycasts;
    }

    public Coroutine FadeOut(float duration = -1f)
    {
        if (duration <= 0f)
        {
            duration = defaultFadeOut;
        }
        return StartFade(1f, duration, true);
    }

    public Coroutine FadeIn(float duration = -1f)
    {
        if (duration <= 0f)
        {
            duration = defaultFadeIn;
        }
        return StartFade(0f, duration, false);
    }

    private Coroutine StartFade(float target, float duration, bool block)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
        }
        _routine = StartCoroutine(FadeRoutine(target, duration, block));
        return _routine;
    }

    private IEnumerator FadeRoutine(float target, float duration, bool block)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.blocksRaycasts = block;
        canvasGroup.interactable = block;

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            float u = t / Mathf.Max(0.0001f, duration);
            float s = u * u * (3f - 2f * u); // smoothstep
            canvasGroup.alpha = Mathf.Lerp(start, target, s);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = target;

        if (!block)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        _routine = null;
    }
}
