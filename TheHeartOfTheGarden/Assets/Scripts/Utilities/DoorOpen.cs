using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DoorTrigger : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Transform that rotates/moves to open. If null, this GameObject's transform is used.")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Transform door2Transform;

    [Header("Rotation (preferred)")]
    [SerializeField] private bool useRotation = true;
    [SerializeField] private Vector3 closedEuler = Vector3.zero;
    [SerializeField] private Vector3 openEuler = new(0f, 90f, 0f);

    [Header("Movement (alternative)")]
    [SerializeField] private bool useTranslation = false;
    [SerializeField] private Vector3 closedPosition = default;
    [SerializeField] private Vector3 openPosition = new(0f, 1.5f, 0f);

    [Header("Timing")]
    [SerializeField] private float speed = 6f;            // how fast the door moves/rotates
    [SerializeField] private float closeDelay = 1.2f;     // delay before auto-closing
    [SerializeField] private bool stayOpenWhilePlayerInside = true;

    [Header("Player filter")]
    [SerializeField] private string playerTag = "Player"; // tag to identify player

    [Header("Events")]
    [SerializeField] private UnityEvent onOpen;
    [SerializeField] private UnityEvent onClose;

    // runtime
    bool _playerInside = false;
    bool _isOpen = false;
    Coroutine _moveCoroutine;
    Coroutine _moveCoroutine2;

    void Reset()
    {
        doorTransform = transform;
        closedPosition = transform.localPosition;
        openPosition = closedPosition + new Vector3(0f, 1.5f, 0f);
        closedEuler = transform.localEulerAngles;
    }

    void Awake()
    {
        if (doorTransform == null) doorTransform = transform;
        // store defaults if user didn't set them
        if (closedPosition == default) closedPosition = doorTransform.localPosition;
        closedEuler = doorTransform.localEulerAngles;
        
        if (door2Transform == null) door2Transform = transform;
        // store defaults if user didn't set them
        if (closedPosition == default) closedPosition = door2Transform.localPosition;
        closedEuler = door2Transform.localEulerAngles;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInside = true;
        Open();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerInside = false;
        if (!stayOpenWhilePlayerInside) return;
        // start close timer
        if (_isOpen) StartCoroutine(DelayedClose());
    }

    IEnumerator DelayedClose()
    {
        yield return new WaitForSeconds(closeDelay);
        // only close if player still absent
        if (!_playerInside) Close();
    }

    void Open()
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        if (_moveCoroutine2 != null) StopCoroutine(_moveCoroutine2);
        _moveCoroutine = StartCoroutine(MoveDoor(open: true));
        _moveCoroutine2 = StartCoroutine(MoveDoor2(open: true));
        if (!_isOpen)
        {
            _isOpen = true;
            onOpen?.Invoke();
        }
    }

    void Close()
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        if (_moveCoroutine2 != null) StopCoroutine(_moveCoroutine2);
        _moveCoroutine = StartCoroutine(MoveDoor(open: false));
        _moveCoroutine2 = StartCoroutine(MoveDoor2(open: false));
        if (_isOpen)
        {
            _isOpen = false;
            onClose?.Invoke();
        }
    }

    IEnumerator MoveDoor(bool open)
    {
        float t = 0f;
        if (useRotation)
        {
            Quaternion from = doorTransform.localRotation;
            Quaternion to = Quaternion.Euler(open ? openEuler : closedEuler);
            // animate using a normalized parameter based on speed
            while (t < 1f)
            {
                t += Time.deltaTime * (speed * 0.5f);
                doorTransform.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            doorTransform.localRotation = to;
        }
        else if (useTranslation)
        {
            Vector3 from = doorTransform.localPosition;
            Vector3 to = open ? openPosition : closedPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * (speed * 0.5f);
                doorTransform.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            doorTransform.localPosition = to;
        }
        _moveCoroutine = null;
    }

    IEnumerator MoveDoor2(bool open)
    {
        float t = 0f;
        if (useRotation)
        {
            Quaternion from = door2Transform.localRotation;
            Quaternion to = Quaternion.Euler(open ? openEuler : closedEuler);
            // animate using a normalized parameter based on speed
            while (t < 1f)
            {
                t += Time.deltaTime * (speed * 0.5f);
                door2Transform.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            door2Transform.localRotation = to;
        }
        else if (useTranslation)
        {
            Vector3 from = door2Transform.localPosition;
            Vector3 to = open ? openPosition : closedPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * (speed * 0.5f);
                door2Transform.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            door2Transform.localPosition = to;
        }
        _moveCoroutine2 = null;
    }
}
