using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimerManager))]
public class TimerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TimerManager manager = (TimerManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Timers", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            foreach (var kvp in manager.GetAllTimers())
            {
                var timer = kvp.Value;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("ID", timer.ID);
                EditorGUILayout.LabelField("Duration", timer.Duration.ToString("F2") + "s");
                EditorGUILayout.LabelField("Elapsed", timer.Elapsed.ToString("F2") + "s");
                EditorGUILayout.LabelField("Looping", timer.IsLooping ? "Yes" : "No");
                EditorGUILayout.LabelField("Running", timer.IsRunning ? "Yes" : "No");
                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Timers will be visible during Play Mode.", MessageType.Info);
        }
    }
}
