using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(WorldGenerationManager), typeof(MyGridRenderer))]
[CustomEditor(typeof(WorldGenerationManager))]
public class WFCEditor : Editor {
    private bool isGridRendererActive = true;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        WorldGenerationManager genManager = (WorldGenerationManager) target;

        MyGridRenderer gridRenderer = genManager.GetComponent<MyGridRenderer>();

        //GUI
        if (GUILayout.Button("Initilise Grid")) {
            genManager.initWorldGen();
        }

        if(GUILayout.Button("Collapse Next Node")) {
            genManager.collapseNextNode();
        }

        if (GUILayout.Button("Generate Next Layer")) {
            genManager.genNextLayer();
        }

        if (GUILayout.Button("Generate All Layers")) {
            genManager.genAll();
        }


        isGridRendererActive = EditorGUILayout.Toggle("Activate Grid Renderer", isGridRendererActive);
        gridRenderer.toggledGridRenderer(isGridRendererActive);

    }

}
