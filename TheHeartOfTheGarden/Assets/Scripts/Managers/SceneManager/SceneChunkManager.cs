using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneChunkManager : Singleton<SceneChunkManager>
{
    public List<SceneChunk> chunks = new();
    public Transform player;
    public bool useEvents = false;
    public bool autoDiscoverChunks = false;
    public static event System.Action<SceneChunk> OnChunkLoaded;
    public static event System.Action<SceneChunk> OnChunkUnloaded;

    // store handle, not SceneInstance
    private Dictionary<SceneChunk, AsyncOperationHandle<SceneInstance>> loadedScenes = new();
    // track chunks currently loading
    private HashSet<SceneChunk> loadingChunks = new();


    private void Update()
    {
        if (player == null || chunks == null) return;

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var toPlayer = player.position - chunk.triggerPosition;
            float sqrDistance = toPlayer.sqrMagnitude;
            float sqrRadius = chunk.triggerRadius * chunk.triggerRadius;

            if (sqrDistance < sqrRadius)
            {
                if (!loadedScenes.ContainsKey(chunk) && !loadingChunks.Contains(chunk))
                {
                    loadingChunks.Add(chunk);
                    var handle = Addressables.LoadSceneAsync(chunk.sceneAddress, LoadSceneMode.Additive);
                    handle.Completed += op =>
                    {
                        loadingChunks.Remove(chunk);
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            loadedScenes[chunk] = op;
                            if (useEvents) OnChunkLoaded?.Invoke(chunk);
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load scene: {chunk.sceneAddress}");
                        }
                    };
                }
            }
            else
            {
                if (loadedScenes.TryGetValue(chunk, out var handle))
                {
                    // unload and only remove after unload completes
                    var unloadOp = Addressables.UnloadSceneAsync(handle);
                    unloadOp.Completed += _ =>
                    {
                        if (useEvents) OnChunkUnloaded?.Invoke(chunk);
                    };
                    loadedScenes.Remove(chunk);
                }
            }
        }
    }
}
