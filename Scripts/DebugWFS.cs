using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//[ExecuteAlways]
public class DebugWFS : MonoBehaviour
{
    const int width = 10;
    const int height = 10;

    [SerializeField] GameObject debugPrefab;
    [SerializeField] Transform debugParent;
    GameObject[,] debugGrid = new GameObject[width, height];

    public void initDebug() {
        //clearing previous instances


        // Store references to all children first
        int childCount = debugParent.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++) {
            children[i] = debugParent.GetChild(i);
        }

        // Detach children from parent to avoid modifying the collection during iteration
        debugParent.DetachChildren();

        // Destroy all children
        for (int i = 0; i < childCount; i++) {
            DestroyImmediate(children[i].gameObject);
        }

        //while(debugParent.childCount != 0) {
        //    DestroyImmediate(debugParent.GetChild(0));
        //}


        //foreach (Transform child in debugParent) {
        //    debugParent.
        //    DestroyImmediate(child.gameObject);
        //}

        //generating new instances
        Vector3 textCoord;
        Vector3 offset = new Vector3(0.5f, 0.5f);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                textCoord = new Vector3Int(x, y) + offset;
                GameObject debugObj = Instantiate(debugPrefab, textCoord, Quaternion.identity, debugParent);
                debugObj.name = ($"{x},{y}");
                debugGrid[x, y] = debugObj;
            }
        }
    }

    public void updateDebugDisplay(Node[,] grid) {
        TextMeshPro entropyText;
        for(int x = 0; x < width;x++) {
            for(int y = 0;y < height;y++) {
                entropyText = debugGrid[x, y].GetComponentInChildren<TextMeshPro>();
                //Debug.Log($"Debug entropy at {x},{y} is {grid[x, y].entropy}");
                entropyText.text = grid[x, y].entropy.ToString();
            }
        }
    }
    //TODO
    //Toggling item off, breaks references and stops them from being updated.
    //GetComponent<Renderer>().enabled = false
    //GetComponent<CanvasRenderer>().cull = false
    //Meshrender?
    public void toggleDebug() {
        foreach (Transform child in debugParent) {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }
}
