using UnityEngine;

public class SceneTransitionListener : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Instance.StartListeningString("FadeToScene", OnSceneChangeRequested);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListeningString("FadeToScene", OnSceneChangeRequested);
    }

    private void OnSceneChangeRequested(string sceneName)
    {
        SceneTransitionSystem.Instance.FadeToScene(sceneName);
    }
}