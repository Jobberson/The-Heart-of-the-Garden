using UnityEngine;

public abstract class PuzzleManagerBase : MonoBehaviour
{
    public abstract void InitializePuzzles();
    public abstract void CheckPuzzleStates();
    public abstract bool IsPuzzleComplete();
}