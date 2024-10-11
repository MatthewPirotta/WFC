using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] WorldGenerationManager worldGenManager;
    [SerializeField] GameObject debugGridPrefab;

    GameObject[] debugGrids = new GameObject[3];

    bool isDebugging = false;
    public bool IsDebugging {
        get => isDebugging;
        set {
            isDebugging = value;
            toggleDebug(value);
        }
    }

    //Layer to be shown
    Layer selectedLayer;
    public Layer SelectedLayer {
        get => selectedLayer;
        set {
            if (debugGrids[(int)selectedLayer] != null) {
                debugGrids[(int)selectedLayer].SetActive(false);
            }
            selectedLayer = value;
            if (debugGrids[(int)selectedLayer] != null && isDebugging) {
                debugGrids[(int)selectedLayer].SetActive(true);
            }
        } 
    } 

    public DebugManager() {
        WFS.initWorld += initDebug;
    }

    void toggleDebug(bool isActive) {
        if (debugGrids[(int)selectedLayer] == null) {
            Debug.LogError("Debug grid is not initialized");
            return;
        }

        debugGrids[(int)selectedLayer].SetActive(isActive);
    }

    //TODO useless parameter
    void initDebug() {
        GameObject debugObj;
        DebugGrid debugGrid;
        MyGrid[] datagrids = worldGenManager.myGrids;

        clearDebugGrids();

        //NOTE the gridsArray is in generation order defined by Layers enum
        for (int i =0; i < datagrids.Length; i++) {
            debugObj = Instantiate(debugGridPrefab, transform);
            debugGrid = debugObj.GetComponent<DebugGrid>();
            debugGrid.gridFactory(datagrids[i]);
            debugObj.name = $"Debug Grid - {datagrids[i].layer}";
            debugGrids[i] = debugObj;

            debugObj.SetActive(false);
        }
    }

    void clearDebugGrids() {
        foreach (GameObject debugGrid in debugGrids) {
            if (debugGrid == null) continue;
            DestroyImmediate(debugGrid);
        }
    }
}
