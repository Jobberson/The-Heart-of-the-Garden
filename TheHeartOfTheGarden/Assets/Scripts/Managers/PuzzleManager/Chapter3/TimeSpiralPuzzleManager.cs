using UnityEngine;

public class TimeSpiralPuzzleManager : MonoBehaviour
{
    public int loopCount = 0;
    public bool loopBroken = false;

    public void OnLoopTriggered()
    {
        if (!loopBroken)
        {
            loopCount++;
            ApplyLoopChanges(loopCount);
        }
    }

    public void BreakLoop()
    {
        loopBroken = true;
        // Trigger exit, open door, change scene, etc.
    }

    private void ApplyLoopChanges(int count)
    {
        // Change lighting, move objects, play sounds, etc.
    }
}