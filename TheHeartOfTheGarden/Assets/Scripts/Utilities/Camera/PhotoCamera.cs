using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class PhotoCamera : MonoBehaviour {
    public KeyCode shutterKey = KeyCode.Mouse0;

    public LayerMask photoableLayer;
    public float maxCaptureDistance = 12f;
    public float captureAngleTolerance = 15f;

    public Transform holdPoint;

    public Image screenFlash;
    public AudioSource shutterSfx;
    public float flashIn = 0.06f, flashOut = 0.5f;

    public float cooldown = 1f;
    float cooldownTimer;

    Camera cam;

    void Awake() {
        cam = GetComponent<Camera>();
    }

    void Update() {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(shutterKey) && cooldownTimer <= 0f) {
            cooldownTimer = cooldown;
            StartCoroutine(Capture());
        }
    }

    IEnumerator Capture() {
        if (shutterSfx) shutterSfx.Play();
        if (screenFlash) StartCoroutine(FlashScreen());

        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxCaptureDistance, photoableLayer)) {
            float angle = Vector3.Angle(cam.transform.forward,
                                        (hit.point - cam.transform.position).normalized);

            if (angle <= captureAngleTolerance) {
                var p = hit.collider.GetComponentInParent<Photoable>();
                if (p != null)
                    p.OnCaptured(holdPoint);
            }
        }

        yield return null;
    }

    IEnumerator FlashScreen() {
        screenFlash.color = new Color(1,1,1,0);

        float t = 0f;
        while (t < flashIn) {
            t += Time.deltaTime;
            screenFlash.color = new Color(1,1,1, Mathf.Lerp(0, 1, t/flashIn));
            yield return null;
        }

        t = 0f;
        while (t < flashOut) {
            t += Time.deltaTime;
            screenFlash.color = new Color(1,1,1, Mathf.Lerp(1, 0, t/flashOut));
            yield return null;
        }

        screenFlash.color = new Color(1,1,1,0);
    }
}
