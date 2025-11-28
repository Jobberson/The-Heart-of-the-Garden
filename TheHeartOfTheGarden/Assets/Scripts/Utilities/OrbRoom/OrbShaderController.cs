using UnityEngine;

[ExecuteAlways]
public class OrbShaderController : MonoBehaviour
{
    [Header("Orb Source")]
    public Transform orbTransform; // if null, uses this.transform

    [Header("Reveal params")]
    public float orbRange = 2.5f;
    public float orbFalloff = 0.8f;
    public float waterY = 0.25f;
    [Space]
    public bool coneEnabled = false;
    [Range(-1f,1f)]
    public float coneAngleCos = 0.7f; // cosine of half-angle
    public Transform orbForwardSource; // optional: direction actor (camera or orb forward)

    [Header("Debugging / Direct test")]
    [Tooltip("If true, script will also write the orbVector to the assigned renderers' materials (for testing).")]
    public bool setMaterialDirectly = false;
    public Renderer[] targetRenderers; // assign one of the wall renderers to test material property
    public float debugLogInterval = 1.0f;
    public bool debugDrawGlobalSphere = true;

    int _OrbPosId, _OrbForwardId, _OrbRangeId, _OrbFalloffId, _WaterYId, _ConeEnabledId, _ConeAngleCosId;
    float lastLogTime = 0f;

    void OnEnable()
    {
        CacheIds();
        UpdateShaderGlobals(true);
    }

    void OnDisable()
    {
        // optional: clear the globals if you want when script is disabled
    }

    void CacheIds()
    {
        _OrbPosId = Shader.PropertyToID("_OrbPos");
        _OrbForwardId = Shader.PropertyToID("_OrbForward");
        _OrbRangeId = Shader.PropertyToID("_OrbRange");
        _OrbFalloffId = Shader.PropertyToID("_OrbFalloff");
        _WaterYId = Shader.PropertyToID("_WaterY");
        _ConeEnabledId = Shader.PropertyToID("_ConeEnabled");
        _ConeAngleCosId = Shader.PropertyToID("_ConeAngleCos");
    }

    void Update()
    {
        if (orbTransform == null) orbTransform = this.transform;
        UpdateShaderGlobals(false);
        if (setMaterialDirectly && targetRenderers != null)
            WriteToMaterials();
    }

    void UpdateShaderGlobals(bool forceLog)
    {
        Vector3 pos = (orbTransform != null) ? orbTransform.position : Vector3.zero;
        Vector3 fwd = (orbForwardSource != null) ? orbForwardSource.forward : (orbTransform != null ? orbTransform.forward : Vector3.forward);

        // set globals
        Shader.SetGlobalVector(_OrbPosId, new Vector4(pos.x, pos.y, pos.z, 1f));
        Shader.SetGlobalVector(_OrbForwardId, new Vector4(fwd.x, fwd.y, fwd.z, 0f));
        Shader.SetGlobalFloat(_OrbRangeId, orbRange);
        Shader.SetGlobalFloat(_OrbFalloffId, orbFalloff);
        Shader.SetGlobalFloat(_WaterYId, waterY);
        Shader.SetGlobalFloat(_ConeEnabledId, coneEnabled ? 1f : 0f);
        Shader.SetGlobalFloat(_ConeAngleCosId, coneAngleCos);

        // occasional logging to verify
        if (forceLog || (debugLogInterval > 0f && Time.realtimeSinceStartup - lastLogTime > debugLogInterval))
        {
            lastLogTime = Time.realtimeSinceStartup;
            Vector4 g = Shader.GetGlobalVector(_OrbPosId);
            Debug.Log($"[OrbShaderController] _OrbPos global = {g}, orbTransform pos = {pos}");
        }
    }

    void WriteToMaterials()
    {
        // For testing only — modifies material instances (renderer.material) so it creates instances at runtime.
        foreach (var r in targetRenderers)
        {
            if (r == null) continue;
            var mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                Vector3 pos = orbTransform != null ? orbTransform.position : Vector3.zero;
                mats[i].SetVector(_OrbPosId, new Vector4(pos.x, pos.y, pos.z, 1f));
                mats[i].SetVector(_OrbForwardId, orbForwardSource ? (Vector4)orbForwardSource.forward : (Vector4)orbTransform.forward);
                mats[i].SetFloat(_OrbRangeId, orbRange);
                mats[i].SetFloat(_WaterYId, waterY);
            }
            r.materials = mats;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (orbTransform == null) orbTransform = this.transform;
        if (orbTransform == null) return;

        // Orb range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(orbTransform.position, orbRange);

        // forward line
        if (orbForwardSource != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(orbTransform.position, orbTransform.position + (orbForwardSource.forward * orbRange));
        }

        // water plane (centered at orb.xz)
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
        float planeSize = 12f;
        Vector3 center = new(orbTransform.position.x, waterY, orbTransform.position.z);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * planeSize);
        Gizmos.DrawCube(Vector3.zero, new Vector3(1f, 0.01f, 1f));
        Gizmos.matrix = old;

        // draw global orb pos from shader as a small sphere (useful to verify SetGlobal worked)
        if (debugDrawGlobalSphere)
        {
            Vector4 globalOrb = Shader.GetGlobalVector(_OrbPosId);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(new Vector3(globalOrb.x, globalOrb.y, globalOrb.z), 0.06f);
        }
    }
}
