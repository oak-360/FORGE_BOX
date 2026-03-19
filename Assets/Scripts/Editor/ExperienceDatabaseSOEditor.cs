using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// By Ofori Ankah Kofi


[CustomEditor(typeof(ExperienceDatabaseSO))]
public class ExperienceDatabaseSOEditor : Editor
{
    private SerializedProperty experiencesProperty;
    private Dictionary<string, List<int>> guidToIndicesMap = new Dictionary<string, List<int>>();
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        experiencesProperty = serializedObject.FindProperty("experiences");
        BuildGuidMap();
        
        // Subscribe to the serializedObject's update event
        Undo.undoRedoPerformed += OnUndoRedo;
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnUndoRedo()
    {
        // Rebuild the GUID map after undo/redo
        BuildGuidMap();
        Repaint();
    }

    private bool needsValidation = false;
    private void OnEditorUpdate()
    {
        if (needsValidation)
        {
            needsValidation = false;
            ValidateAndFixNewEntries();
        }
    }

    private void BuildGuidMap()
    {
        guidToIndicesMap.Clear();
        for (int i = 0; i < experiencesProperty.arraySize; i++)
        {
            var entry = experiencesProperty.GetArrayElementAtIndex(i);
            var guidProp = entry.FindPropertyRelative("imageReferenceGuid");
            
            if (string.IsNullOrEmpty(guidProp.stringValue))
            {
                // Generate GUID for new entries
                guidProp.stringValue = System.Guid.NewGuid().ToString();
                needsValidation = true;
                EditorUtility.SetDirty(target);
            }
            
            string guid = guidProp.stringValue;
            
            if (!guidToIndicesMap.ContainsKey(guid))
            {
                guidToIndicesMap[guid] = new List<int>();
            }
            guidToIndicesMap[guid].Add(i);
        }

        if (needsValidation)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void ValidateAndFixNewEntries()
    {
        bool modified = false;
        for (int i = 0; i < experiencesProperty.arraySize; i++)
        {
            var entry = experiencesProperty.GetArrayElementAtIndex(i);
            var guidProp = entry.FindPropertyRelative("imageReferenceGuid");
            
            if (string.IsNullOrEmpty(guidProp.stringValue))
            {
                guidProp.stringValue = System.Guid.NewGuid().ToString();
                modified = true;
            }
        }
        
        if (modified)
        {
            serializedObject.ApplyModifiedProperties();
            BuildGuidMap();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BuildGuidMap(); // Rebuild the GUID map to reflect any changes

        // Draw the default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        
        // Show duplicate GUIDs warning
        var duplicates = guidToIndicesMap.Where(pair => pair.Value.Count > 1).ToList();
        if (duplicates.Count > 0)
        {
            EditorGUILayout.HelpBox("Duplicate GUIDs detected!", MessageType.Warning);
            foreach (var dup in duplicates)
            {
                EditorGUILayout.HelpBox(
                    $"GUID: {dup.Key} is used by entries: {string.Join(", ", dup.Value)}",
                    MessageType.Warning);
            }
        }

        // Buttons layout
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Generate Missing GUIDs"))
            {
                GenerateMissingGUIDs();
            }

            if (GUILayout.Button("Fix Duplicate GUIDs"))
            {
                FixDuplicateGUIDs();
            }
        }
        EditorGUILayout.EndHorizontal();

        // Draw list with GUID validation
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < experiencesProperty.arraySize; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var entry = experiencesProperty.GetArrayElementAtIndex(i);
            var guidProp = entry.FindPropertyRelative("imageReferenceGuid");
            var nameProp = entry.FindPropertyRelative("experienceName");
            
            // Show entry header
            string entryName = string.IsNullOrEmpty(nameProp.stringValue) ? $"Entry {i}" : nameProp.stringValue;
            EditorGUILayout.LabelField(entryName, EditorStyles.boldLabel);
            
            // Show GUID field with color coding
            bool isDuplicate = guidToIndicesMap.TryGetValue(guidProp.stringValue, out var indices) && indices.Count > 1;
            Color originalColor = GUI.backgroundColor;
            
            if (isDuplicate)
            {
                GUI.backgroundColor = Color.red;
            }
            else if (string.IsNullOrEmpty(guidProp.stringValue))
            {
                GUI.backgroundColor = Color.yellow;
            }
            
            EditorGUILayout.PropertyField(guidProp, new GUIContent("GUID"));
            GUI.backgroundColor = originalColor;
            
            // Show fix button for this entry if needed
            if (isDuplicate || string.IsNullOrEmpty(guidProp.stringValue))
            {
                if (GUILayout.Button("Fix This GUID", GUILayout.Width(100)))
                {
                    guidProp.stringValue = System.Guid.NewGuid().ToString();
                    serializedObject.ApplyModifiedProperties();
                    BuildGuidMap();
                    GUIUtility.ExitGUI();
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        EditorGUILayout.EndScrollView();

        // Handle array modifications
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            BuildGuidMap();
        }
    }

    private void GenerateMissingGUIDs()
    {
        bool modified = false;
        for (int i = 0; i < experiencesProperty.arraySize; i++)
        {
            var entry = experiencesProperty.GetArrayElementAtIndex(i);
            var guidProp = entry.FindPropertyRelative("imageReferenceGuid");
            
            if (string.IsNullOrEmpty(guidProp.stringValue))
            {
                guidProp.stringValue = System.Guid.NewGuid().ToString();
                modified = true;
            }
        }
        
        if (modified)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    private void FixDuplicateGUIDs()
    {
        var modifiedIndices = new List<int>();
        
        foreach (var guidPair in guidToIndicesMap)
        {
            if (guidPair.Value.Count > 1) // Only process duplicates
            {
                // Keep the first occurrence, fix the rest
                for (int i = 1; i < guidPair.Value.Count; i++)
                {
                    int index = guidPair.Value[i];
                    var entry = experiencesProperty.GetArrayElementAtIndex(index);
                    var guidProp = entry.FindPropertyRelative("imageReferenceGuid");
                    guidProp.stringValue = System.Guid.NewGuid().ToString();
                    modifiedIndices.Add(index);
                }
            }
        }
        
        if (modifiedIndices.Count > 0)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            BuildGuidMap(); // Rebuild the map after changes
        }
    }
}