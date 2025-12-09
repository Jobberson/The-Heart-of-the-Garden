using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class LookReveal : MonoBehaviour
{
    public Camera cam;
    public float angleCosThreshold = 0.86f;
    public Material overlayMaterial; 
    public float fadeDuration = 0.25f;
    public bool revealOnlyIfOccluded = true;

    Renderer rend;
    Material overlayInstance;
    Color overlayBaseColor;
    bool isRevealed;
    Coroutine fadeCoroutine;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (cam == null) cam = Camera.main;
        if (overlayMaterial != null) {
            overlayInstance = new Material(overlayMaterial); 
            overlayBaseColor = overlayInstance.HasProperty("_Color") ? overlayInstance.GetColor("_Color") : Color.white;
        }
    }

    void Update()
    {
        if (cam == null || overlayInstance == null) return;

        Vector3 toObj = (transform.position - cam.transform.position);
        float dist = toObj.magnitude;
        Vector3 dir = toObj / Mathf.Max(dist, 0.0001f);
        float dot = Vector3.Dot(cam.transform.forward, dir);
        bool looking = dot >= angleCosThreshold;

        bool occluded = false;
        if (revealOnlyIfOccluded && looking) {
            if (Physics.Raycast(cam.transform.position, dir, out var hit, dist + 0.01f)) occluded = hit.collider != GetComponent<Collider>();
            else occluded = false;
        }

        bool shouldReveal = looking && (!revealOnlyIfOccluded || occluded);

        if (shouldReveal != isRevealed)
        {
            isRevealed = shouldReveal;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTo(isRevealed ? 1f : 0f));
        }
    }

    IEnumerator FadeTo(float target)
    {
        float start = 0f;
        if (overlayInstance.HasProperty("_Color")) start = overlayInstance.GetColor("_Color").a;
        float t = 0f;

        if (start == 0f && target > 0f) {
            var mats = rend.sharedMaterials;
            System.Array.Resize(ref mats, Mathf.Max(1, mats.Length) + 1);
            mats[mats.Length - 1] = overlayInstance;
            rend.materials = mats;
        }

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, fadeDuration);
            float a = Mathf.Lerp(start, target, t);
            if (overlayInstance.HasProperty("_Color"))
            {
                var c = overlayBaseColor; c.a = a;
                overlayInstance.SetColor("_Color", c);
            }
            yield return null;
        }

        if (target <= 0f)
        {
            var mats = rend.sharedMaterials;
            if (mats.Length > 1)
            {
                System.Array.Resize(ref mats, mats.Length - 1);
                rend.materials = mats;
            }
        }
    }
}
