using UnityEngine;
using Snog.Audio;

public class SfxSoundsListeners : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Instance.StartListening("PhoneNotification", PlayPhoneSound);
        EventManager.Instance.StartListening("PuzzleSolved", PlayPuzzleSolvedSound);
        EventManager.Instance.StartListening("MirrorEntered", PlayMirrorSound);
    }

    private void OnDisable()
    {
        EventManager.Instance.StopListening("PhoneNotification", PlayPhoneSound);
        EventManager.Instance.StopListening("PuzzleSolved", PlayPuzzleSolvedSound);
        EventManager.Instance.StopListening("MirrorEntered", PlayMirrorSound);
    }

    private void PlayPhoneSound()
    {
        AudioManager.Instance.PlaySound2D("phone_notification");
    }

    private void PlayPuzzleSolvedSound()
    {
        AudioManager.Instance.PlaySound2D("puzzle_solved_chime");
    }

    private void PlayMirrorSound()
    {
        AudioManager.Instance.PlaySound2D("mirror_enter");
    }
}