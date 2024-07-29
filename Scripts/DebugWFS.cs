using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//[ExecuteAlways]
public class DebugWFS : MonoBehaviour
{
    [SerializeField] GameObject debugPrefab;
    [SerializeField] Transform debugParent;
    GameObject[,] debugGrid = new GameObject[MyGrid.WIDTH, MyGrid.HEIGHT];

    public void initDebug() {
        //to prevent undefined behaviour
        clearPreviousInstance();

        //generating new instances
        Vector3 textCoord;
        Vector3 offset = new Vector3(0.5f, 0.5f);
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                textCoord = new Vector3Int(x, y) + offset;
                GameObject debugObj = Instantiate(debugPrefab, textCoord, Quaternion.identity, debugParent);
                debugObj.name = ($"{x},{y}");
                debugGrid[x, y] = debugObj;
            }
        }
    }

    private void clearPreviousInstance() {
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
    }

    //TODO
    //This can be optimised by only chaning the updated blocks
    public void updateDebugDisplay(Node[,] grid) {
        TextMeshPro entropyText;
        int roundedEntropy;
        for (int x = 0; x < MyGrid.WIDTH;x++) {
            for(int y = 0;y < MyGrid.HEIGHT; y++) {
                entropyText = debugGrid[x, y].GetComponentInChildren<TextMeshPro>();
                //Debug.Log($"In updateDebugDisplay entropy at ({x}, {y}) is {grid[x, y].entropy}");
                // roundedEntropy = Mathf.RoundToInt(grid[x, y].entropy);
                entropyText.text = grid[x, y].entropy.ToString();
            }
        }
    }

    //TODO
    //Toggling item off, breaks references and stops them from being updated.
    //GetComponent<Renderer>().enabled = false
    //GetComponent<CanvasRenderer>().cull = false
    //Meshrender?
    //NVM it seems to work fine as is
    public void toggleDebug() {
        foreach (Transform child in debugParent) {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }
}
