using UnityEngine;

public class PlayerBridgeMaskUpdater : MonoBehaviour
{
    public Transform player;
    public float radius = 5f;
    public float feather = 1f;
    public string globalPosName = "_PlayerPos";
    public string globalRadiusName = "_Radius";
    public string globalFeatherName = "_Feather";

    void LateUpdate()
    {
        if (player == null) return;
        Vector3 p = player.position;
        Shader.SetGlobalVector(globalPosName, new Vector3(p.x, p.y, p.z));
        Shader.SetGlobalFloat(globalRadiusName, radius);
        Shader.SetGlobalFloat(globalFeatherName, feather);
    }
}
