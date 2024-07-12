using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WFS))]
public class WFCEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        WFS wfs = (WFS) target;

        if(GUILayout.Button("Initilise Grid")) {
            wfs.InitGrid();
        }

        if (GUILayout.Button("Collapse Next")) {
            wfs.WFC();
        }

        if (GUILayout.Button("Toggle Collapse all")) {
            wfs.collapseAll = !wfs.collapseAll;
        }

        if (GUILayout.Button("Toggle Debug")) {
            wfs.toggleDebugDisplay();
        }

        if (GUILayout.Button("Print entropy")) {
            wfs.printEntropy();
        }
    }

}
