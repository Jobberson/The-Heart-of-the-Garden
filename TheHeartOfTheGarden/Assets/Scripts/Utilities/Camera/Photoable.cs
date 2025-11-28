using UnityEngine;

public class Photoable : MonoBehaviour {
    public GameObject realPrefab;
    public bool spawnInHand = false;

    public GameObject realizeVfxPrefab;
    public AudioClip realizeSfx;

    AudioSource audioSrc;

    void Awake() {
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    public void OnCaptured(Transform holdPoint) {
        if (realPrefab == null) return;

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Transform parent = null;

        if (spawnInHand && holdPoint != null) {
            pos = holdPoint.position;
            rot = holdPoint.rotation;
            parent = holdPoint;
        }

        var real = Instantiate(realPrefab, pos, rot, parent);

        if (realizeVfxPrefab) {
            var vfx = Instantiate(realizeVfxPrefab, pos, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (realizeSfx)
            audioSrc.PlayOneShot(realizeSfx);

        Destroy(gameObject);
    }
}
