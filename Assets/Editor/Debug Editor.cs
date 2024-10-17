using System;   
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DebugManager))]
public class DebugEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        DebugManager genManager = (DebugManager)target;

        //NOTE CHATGPT
        // Get all enum names for the Layer enum
        string[] enumNames = Enum.GetNames(typeof(Layer));

        // Convert the current enum value to its corresponding index
        int selectedIndex = Array.IndexOf(enumNames, genManager.SelectedLayer.ToString());

        // Create a dropdown using EditorGUILayout.Popup
        selectedIndex = EditorGUILayout.Popup("Select Layer", selectedIndex, enumNames);

        // Set the selectedLayer field based on the dropdown selection
        genManager.SelectedLayer = (Layer)Enum.Parse(typeof(Layer), enumNames[selectedIndex]);

        // Use the serialized property in the DebugManager for the toggle state
        genManager.IsDebugging = EditorGUILayout.Toggle("Activate Debug Grid Renderer", genManager.IsDebugging);


        // Ensure changes are applied to the scene
        if (GUI.changed) {
            EditorUtility.SetDirty(genManager);
        }
    }
}
