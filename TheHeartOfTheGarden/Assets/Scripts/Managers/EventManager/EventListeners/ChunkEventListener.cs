using UnityEngine;

public class ChunkEventListener : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Instance.StartListeningString("ChunkLoaded", OnChunkLoaded);
        EventManager.Instance.StartListeningString("ChunkUnloaded", OnChunkUnloaded);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListeningString("ChunkLoaded", OnChunkLoaded);
        EventManager.Instance.StopListeningString("ChunkUnloaded", OnChunkUnloaded);
    }

    private void OnChunkLoaded(string chunkName)
    {
        Debug.Log($"Chunk loaded: {chunkName}");
        // Trigger music, UI, etc.
    }

    private void OnChunkUnloaded(string chunkName)
    {
        Debug.Log($"Chunk unloaded: {chunkName}");
        // Fade out audio, hide UI, etc.
    }
}