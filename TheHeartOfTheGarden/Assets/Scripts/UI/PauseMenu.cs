using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : Singleton<PauseMenu>
{
    public GameObject pausePanel;
    public Button resumeButton;
    public Button saveQuitButton;

    private bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        saveQuitButton.onClick.AddListener(SaveAndQuit);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        TogglePause();
    }

    public void SaveAndQuit()
    {
        SaveLoadManager.Instance.SaveCurrentScene();
        Time.timeScale = 1f;
        SceneController.Instance.LoadScene("MainMenu");
    }
}