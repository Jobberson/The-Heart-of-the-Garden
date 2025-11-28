using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneChunkManager))]
public class SceneChunkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneChunkManager manager = (SceneChunkManager)target;

        // Begin change check for undo/dirty support
        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(manager, "Edit SceneChunkManager");

        EditorGUILayout.LabelField("Scene Chunk Manager", EditorStyles.boldLabel);

        manager.player = (Transform)EditorGUILayout.ObjectField("Player", manager.player, typeof(Transform), true);
        manager.autoDiscoverChunks = EditorGUILayout.Toggle("Auto Discover Chunks", manager.autoDiscoverChunks);
        manager.useEvents = EditorGUILayout.Toggle("Use Events (OnChunkLoaded/OnChunkUnloaded)", manager.useEvents);

        EditorGUILayout.Space();

        // Auto-discover helper (Editor-only)
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Discover SceneChunk Assets"))
        {
#if UNITY_EDITOR
            DiscoverChunks(manager);
#endif
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Chunks", EditorStyles.boldLabel);

        if (manager.chunks == null)
            manager.chunks = new System.Collections.Generic.List<SceneChunk>();

        // Display list
        for (int i = 0; i < manager.chunks.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            manager.chunks[i] = (SceneChunk)EditorGUILayout.ObjectField("Chunk", manager.chunks[i], typeof(SceneChunk), false);

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                manager.chunks.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break; // list changed, break to avoid iterator issues
            }

            EditorGUILayout.EndHorizontal();

            if (manager.chunks[i] != null)
            {
                // Show key fields from the ScriptableObject
                EditorGUILayout.LabelField("Scene Address", manager.chunks[i].sceneAddress);
                EditorGUILayout.LabelField("Trigger Position", manager.chunks[i].triggerPosition.ToString("F3"));
                EditorGUILayout.LabelField("Trigger Radius", manager.chunks[i].triggerRadius.ToString("F3"));
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a SceneChunk ScriptableObject here.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Chunk Slot"))
        {
            manager.chunks.Add(null);
        }
        if (GUILayout.Button("Remove Null Slots"))
        {
            manager.chunks.RemoveAll(c => c == null);
        }
        EditorGUILayout.EndHorizontal();

        // Mark scene dirty if changed
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(manager);
        }
    }

#if UNITY_EDITOR
    private void DiscoverChunks(SceneChunkManager manager)
    {
        // Editor-only: find all SceneChunk assets and populate the list
        string[] guids = AssetDatabase.FindAssets("t:SceneChunk");
        System.Collections.Generic.List<SceneChunk> found = new System.Collections.Generic.List<SceneChunk>(guids.Length);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            SceneChunk sc = AssetDatabase.LoadAssetAtPath<SceneChunk>(path);
            if (sc != null) found.Add(sc);
        }

        if (found.Count == 0)
        {
            EditorUtility.DisplayDialog("Discover SceneChunks", "No SceneChunk assets found in project.", "OK");
            return;
        }

        Undo.RecordObject(manager, "Discover SceneChunks");
        manager.chunks = found;
        EditorUtility.SetDirty(manager);
        EditorUtility.DisplayDialog("Discover SceneChunks", $"Found and assigned {found.Count} SceneChunk(s).", "OK");
    }
#endif
}
