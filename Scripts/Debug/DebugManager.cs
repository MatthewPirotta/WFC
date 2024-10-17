using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] WorldGenerationManager worldGenManager;
    [SerializeField] GameObject debugGridPrefab;

    static int numLayers = Enum.GetNames(typeof(Layer)).Length;
    GameObject[] debugGrids = new GameObject[numLayers];

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
        WFC.initWorld += initDebug;
    }

    void toggleDebug(bool isActive) {
        if (debugGrids[(int)selectedLayer] == null) {
            Debug.LogError("Debug grid is not initialized");
            return;
        }

        debugGrids[(int)selectedLayer].SetActive(isActive);
    }

    void initDebug() {
        IGrid[] datagrids = worldGenManager.myGrids;

        clearDebugGrids();


        //NOTE the gridsArray is in generation order defined by Layers enum
        for (int i =0; i < datagrids.Length; i++) {
            instantiateDebugGrid(i);
        }

        void instantiateDebugGrid(int layerIndex) {
            GameObject debugObj;
            DebugGrid debugGrid;

            debugObj = Instantiate(debugGridPrefab, transform);
            debugGrid = debugObj.GetComponent<DebugGrid>();
            debugGrid.gridFactory(datagrids[layerIndex]);
            debugObj.name = $"Debug Grid - {datagrids[layerIndex].layer}";
            debugGrids[layerIndex] = debugObj;

            debugObj.SetActive(false);
        }
    }

    void clearDebugGrids() {
        // Loop through all child objects and destroy them
        //When reebooting unity it can create duplicate debug objects
        for (int i = transform.childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Clear references after destruction
        debugGrids = new GameObject[3];
    }
}
