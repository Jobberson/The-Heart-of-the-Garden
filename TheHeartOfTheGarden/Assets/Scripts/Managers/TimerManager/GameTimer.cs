using System;

public class GameTimer
{
    public string ID;
    public float Duration;
    public float Elapsed;
    public bool IsLooping;
    public bool IsRunning;
    public Action OnTimerComplete;

    public GameTimer(string id, float duration, bool isLooping, Action onComplete)
    {
        ID = id;
        Duration = duration;
        IsLooping = isLooping;
        OnTimerComplete = onComplete;
        Elapsed = 0f;
        IsRunning = true;
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning) return;

        Elapsed += deltaTime;
        if (Elapsed >= Duration)
        {
            OnTimerComplete?.Invoke();

            if (IsLooping)
            {
                Elapsed = 0f;
            }
            else
            {
                IsRunning = false;
            }
        }
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Resume()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
        Elapsed = 0f;
    }

    /*
    ** Example Usage:TimerManager.Instance.CreateTimer(
    "PulseEffect",
    2f,
    true,
    () => Debug.Log("Pulse triggered!")
);

TimerManager.Instance.CreateTimer(
    "PuzzleUnlock",
    10f,
    false,
    () => Debug.Log("Puzzle unlocked!")
);*/

}
