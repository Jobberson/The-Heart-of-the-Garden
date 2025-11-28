using UnityEngine;

[CreateAssetMenu(menuName = "SceneStreamer/SceneChunk")]
public class SceneChunk : ScriptableObject
{
    public string sceneAddress; // Addressable scene name
    public Vector3 triggerPosition; // changed to Vector3 for 3D worlds
    public float triggerRadius = 10f;
    public string description;
}
