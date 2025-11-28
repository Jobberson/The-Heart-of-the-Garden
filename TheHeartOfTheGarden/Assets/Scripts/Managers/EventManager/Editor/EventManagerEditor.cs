using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System.Collections.Generic;

[CustomEditor(typeof(EventManager))]
public class EventManagerEditor : Editor
{
    private string newEventName = "NewEvent";
    private MonoBehaviour listenerTarget;
    private string listenerMethodName = "listenerMethodName";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EventManager manager = (EventManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add String Event Listener", EditorStyles.boldLabel);

        newEventName = EditorGUILayout.TextField("Event Name", newEventName);
        listenerTarget = (MonoBehaviour)EditorGUILayout.ObjectField("Listener Target", listenerTarget, typeof(MonoBehaviour), true);
        listenerMethodName = EditorGUILayout.TextField("Method Name", listenerMethodName);

        if (GUILayout.Button("Add Listener") && !string.IsNullOrEmpty(newEventName) && listenerTarget != null && !string.IsNullOrEmpty(listenerMethodName))
        {
            UnityAction<string> action = (string value) => {
                var method = listenerTarget.GetType().GetMethod(listenerMethodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(listenerTarget, new object[] { value });
                }
                else
                {
                    Debug.LogError($"Method '{listenerMethodName}' not found on {listenerTarget.name}.");
                }
            };

            manager.StartListeningString(newEventName, action);
            Debug.Log($"Added listener for event '{newEventName}' on {listenerTarget.name}.");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Registered String Events", EditorStyles.boldLabel);

        var stringEventsField = typeof(EventManager).GetField("stringEventDictionary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var stringEvents = stringEventsField?.GetValue(manager) as Dictionary<string, UnityEvent<string>>;

        if (stringEvents != null)
        {
            foreach (var kvp in stringEvents)
            {
                EditorGUILayout.LabelField($"• {kvp.Key} ({kvp.Value.GetPersistentEventCount()} listeners)");
            }
        }
        else
        {
            EditorGUILayout.LabelField("No string events registered.");
        }
    }
}
