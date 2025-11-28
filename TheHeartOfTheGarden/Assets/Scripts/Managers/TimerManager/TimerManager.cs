using System.Collections.Generic;
using UnityEngine;

public class TimerManager : Singleton<TimerManager>
{
    private readonly Dictionary<string, GameTimer> timers = new();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        var keys = new List<string>(timers.Keys);
        foreach (var key in keys)
        {
            if (!timers.TryGetValue(key, out var timer)) continue;
            timer.Update(deltaTime);
            if (!timer.IsLooping && !timer.IsRunning)
                timers.Remove(key);
        }
    }

    public void CreateTimer(string id, float duration, bool isLooping, System.Action onComplete, bool overwrite = false)
    {
        if (timers.ContainsKey(id))
        {
            if (!overwrite) {
                Debug.LogWarning($"Timer with ID '{id}' already exists.");
                return;
            }
            timers.Remove(id);
        }
        timers.Add(id, new GameTimer(id, duration, isLooping, onComplete));
    }

    public void StopTimer(string id)
    {
        if (timers.TryGetValue(id, out var timer))
        {
            timer.Stop();
        }
    }

    public void PauseTimer(string id)
    {
        if (timers.TryGetValue(id, out var timer))
        {
            timer.Pause();
        }
    }

    public void ResumeTimer(string id)
    {
        if (timers.TryGetValue(id, out var timer))
        {
            timer.Resume();
        }
    }

    public bool TimerExists(string id) => timers.ContainsKey(id);

    public void RemoveTimer(string id)
    {
        timers.Remove(id);
    }

    public Dictionary<string, GameTimer> GetAllTimers() => timers;
}
