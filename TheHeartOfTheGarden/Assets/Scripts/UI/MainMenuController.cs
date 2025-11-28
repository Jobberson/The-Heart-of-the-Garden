using System;
using System.Collections;
using Snog.Audio;
using SpaceFusion.SF_Portals.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : Singleton<MainMenuController>
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    
    [Header("UI Elements")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button settingsButton;

    [Header("Menu Canvas")]
    [SerializeField] private GameObject MenuCanvas;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup menuCanvasGroup;

    [Tooltip("Seconds to fade out the menu")]
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("HUD Canvas")]
    [SerializeField] private GameObject HudCanvas;
    bool isFading = false;

    private void Start()
    {
        HudCanvas.SetActive(false);
        playerController = FindAnyObjectByType<PlayerController>();
        DisablePlayerControl();

        // Basic safety: ensure buttons are assigned
        if (newGameButton == null || continueButton == null || quitButton == null || settingsButton == null)
            return;

        StartCoroutine(AudioManager.Instance.PlayMusicFade("MixdownLong", 1f));

        // Continue button state from save (safe-call)
        bool hasSave = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSave();
        SetContinueButtonActive(hasSave);

        // Remove previous listeners to avoid duplication
        newGameButton.onClick.RemoveAllListeners();
        continueButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();

        // Add listeners
        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    public void SetContinueButtonActive(bool isActive)
    {
        if (continueButton == null) return;
        continueButton.interactable = isActive;
    }

    private void DisableAllButtons()
    {
        if (newGameButton) newGameButton.interactable = false;
        if (continueButton) continueButton.interactable = false;
        if (quitButton) quitButton.interactable = false;
        if (settingsButton) settingsButton.interactable = false;
    }

    private void OnNewGameClicked()
    {
        if (isFading) return;
        DisableAllButtons();

        StartCoroutine(AudioManager.Instance.StopMusicFade(fadeDuration));

        if (menuCanvasGroup != null)
        {
            // Disable interaction during fade
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
            StartCoroutine(FadeOutAndThen(menuCanvasGroup, fadeDuration, () =>
            {
                // After fade completes, hide the menu panel and start the game
                menuCanvasGroup.gameObject.SetActive(false);

                if(SaveLoadManager.Instance != null)
                {
                    SaveLoadManager.Instance.NewGame();
                    EnablePlayerControl();
                }
            }));
        }
        else
        {
            // Fallback: hide immediately
            if (menuCanvasGroup != null)
                menuCanvasGroup.gameObject.SetActive(false);

            if(SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.NewGame();
                EnablePlayerControl();
            }
        }
    }

    private void OnContinueClicked()
    {
        if (isFading) return;
        DisableAllButtons();

        // If you want Continue to also fade the menu, reuse the same coroutine:
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
            StartCoroutine(FadeOutAndThen(menuCanvasGroup, fadeDuration, () =>
            {
                menuCanvasGroup.gameObject.SetActive(false);
                
                if(SaveLoadManager.Instance != null)
                {   
                    SaveLoadManager.Instance.ContinueGame();
                    EnablePlayerControl();
                }
                    
            }));
        }
        else
        {
            if(SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.ContinueGame();
                EnablePlayerControl();
            }
        }
    }

    private void OnQuitClicked()
    {
        if (isFading) return;
        DisableAllButtons();
        // You can fade out then quit as well; for now quit immediately
        Application.Quit();
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // Generic fade coroutine with callback on completion
    private IEnumerator FadeOutAndThen(CanvasGroup cg, float duration, Action onComplete)
    {
        if (cg == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        isFading = true;
        float startAlpha = cg.alpha;
        float time = 0f;

        // While fading, make sure it won't receive input
        cg.interactable = false;
        cg.blocksRaycasts = false;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime; // use unscaled so UI still fades if timeScale is 0
            cg.alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(time / duration));
            yield return null;
        }

        cg.alpha = 0f;
        isFading = false;
        onComplete?.Invoke();
    }

    // Optional: public method to open/close settings from inspector buttons
    public void CloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void EnablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.EnableControls();
        }

        MenuCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        HudCanvas.SetActive(true);
    }

    public void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.DisableControls();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        HudCanvas.SetActive(false);
    }
}
