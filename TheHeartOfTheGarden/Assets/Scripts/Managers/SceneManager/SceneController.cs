public class SceneController : Singleton<SceneController>
{
    public void LoadScene(string sceneName)
    {
        SaveLoadManager.Instance.SaveCurrentScene();
        SceneTransitionSystem.Instance.FadeToScene(sceneName);
    }

    public void LoadSavedScene()
    {
        string sceneToLoad = SaveLoadManager.Instance.GetSavedScene();
        LoadScene(sceneToLoad);
    }
}