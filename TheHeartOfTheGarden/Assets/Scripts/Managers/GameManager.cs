using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    private void Start() 
    {
        IgnoreLayerCollisions();
    }

    public void OnCheckpointReached()
    {
        SaveLoadManager.Instance.SaveCurrentScene();
    }

    private void IgnoreLayerCollisions()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Interactible"), true);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneController.Instance.LoadScene("SampleScene");
        }
    }
}