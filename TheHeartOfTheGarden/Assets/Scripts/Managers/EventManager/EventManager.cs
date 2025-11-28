using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent> eventDictionary = new();
    private Dictionary<string, UnityEvent<float>> floatEventDictionary = new();
    private Dictionary<string, UnityEvent<string>> stringEventDictionary = new();

    public static EventManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Basic event
    public void StartListening(string eventName, UnityAction listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            eventDictionary.Add(eventName, thisEvent);
        }
    }

    public void StopListening(string eventName, UnityAction listener)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public void TriggerEvent(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke();
        }
    }

    // Float event
    public void StartListeningFloat(string eventName, UnityAction<float> listener)
    {
        if (floatEventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent<float>();
            thisEvent.AddListener(listener);
            floatEventDictionary.Add(eventName, thisEvent);
        }
    }

    public void TriggerEventFloat(string eventName, float value)
    {
        if (floatEventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke(value);
        }
    }

    // String event
    public void StartListeningString(string eventName, UnityAction<string> listener)
    {
        if (stringEventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent<string>();
            thisEvent.AddListener(listener);
            stringEventDictionary.Add(eventName, thisEvent);
        }
    }

    public void StopListeningString(string eventName, UnityAction<string> listener)
    {
        if (stringEventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public void TriggerEventString(string eventName, string value)
    {
        if (stringEventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke(value);
        }
    }
}