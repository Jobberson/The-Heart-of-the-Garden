using UnityEngine;
using UnityEngine.SceneManagement;
using AASave;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    public SaveSystem saveSystem;

    private void Start()
    {
        bool hasSave = saveSystem.DoesDataExists("CurrentScene");
        MainMenuController.Instance.SetContinueButtonActive(hasSave);
    }

    public void NewGame()
    {
        if (saveSystem.DoesDataExists("CurrentScene"))
        {
            saveSystem.Delete("CurrentScene");
        }
    }

    public void ContinueGame()
    {
        if (saveSystem.DoesDataExists("CurrentScene"))
        {
            string sceneToLoad = saveSystem.Load("CurrentScene", "Scene_01").AsString();
            SceneController.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            NewGame();
        }
    }

    public void SaveCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        saveSystem.Save("CurrentScene", currentScene);
    }

    public string GetSavedScene()
    {
        return saveSystem.Load("CurrentScene", "Scene_01").AsString();
    }

    public bool HasSave()
    {
        return saveSystem.DoesDataExists("CurrentScene");
    }

    public void DeleteSave()
    {
        if (HasSave())
        {
            saveSystem.Delete("CurrentScene");
        }
    }
}