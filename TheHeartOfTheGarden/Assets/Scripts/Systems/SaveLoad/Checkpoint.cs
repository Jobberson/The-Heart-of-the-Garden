using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SaveLoadManager.Instance.SaveCurrentScene();
            CheckpointUI.Instance.ShowCheckpointMessage();
        }
    }
}