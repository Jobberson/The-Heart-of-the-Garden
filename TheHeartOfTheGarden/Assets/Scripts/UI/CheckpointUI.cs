using UnityEngine;
using TMPro;
using System.Collections;

public class CheckpointUI : Singleton<CheckpointUI>
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI popupText;
    public float fadeDuration = 0.5f;
    public float displayDuration = 2f;

    private Coroutine currentRoutine;

    public void ShowCheckpointMessage(string message = "Checkpoint Saved")
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        popupText.text = message;
        currentRoutine = StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        canvasGroup.gameObject.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));

        canvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}