using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class DebugGrid : MonoBehaviour {
    [SerializeField] GameObject debugPrefab; //TODO somehowmake this constant throughout all instances
    GameObject[,] debugGrid = new GameObject[MyGrid.WIDTH, MyGrid.HEIGHT];

    MyGrid grid;

    TextMeshPro prevEntropyText;

    //left to right, top to bottom
    private Vector2Int[] debugTileOffsets = {
        new Vector2Int(-1, 1), //top left
        new Vector2Int(0, 1), //top mid
        new Vector2Int(1, 1), //top right
        new Vector2Int(-1, 0), //mid left
        new Vector2Int(0, 0), //mid mid
        new Vector2Int(1, 0), //mid right
        new Vector2Int(-1, -1), //bottom left
        new Vector2Int(0, -1), //bottom mid
        new Vector2Int(1, -1) //bottom right
    };


    //TODO not happy
    public void gridFactory(MyGrid grid) {
        this.grid = grid;
    }

    //TODO NOTE this is being enabled multiple times in the inspector

    private void OnEnable() {
        //refreshing the whole grid since debug was last enabled
        if(grid != null) {
            //Debug.Log($"Updating! {transform.name}");
            updateDebugGrid(grid);
        }

        WFS.collapsedNode += updateDebugNode;
        WFS.updateGrid += updateDebugGrid;
        WFS.propogateNodeData += updateDebugNode;
    }

    private void OnDisable() {
        WFS.collapsedNode -= updateDebugNode;
        WFS.updateGrid -= updateDebugGrid;
        WFS.propogateNodeData -= updateDebugNode;
    }

    private void Awake() {
        initDebug();
    }

    public void initDebug() {
        //generating new instances
        Vector3 textCoord;
        Vector3 offset = new Vector3(0.5f, 0.5f);
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                textCoord = new Vector3Int(x, y) + offset;
                GameObject debugObj = Instantiate(debugPrefab, textCoord, Quaternion.identity, this.transform);
                debugObj.name = ($"{x},{y}");
                debugGrid[x, y] = debugObj;
            }
        }
    }

    public void resetDebug() {
        //to prevent undefined behaviour
        destroyDebug(transform);
        initDebug();
    }

    private void destroyDebug(Transform transform) {
        // Store references to all children first
        int childCount = transform.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++) {
            children[i] = transform.GetChild(i);
        }

        // Detach children from parent to avoid modifying the collection during iteration
        transform.DetachChildren();

        // Destroy all children
        for (int i = 0; i < childCount; i++) {
            DestroyImmediate(children[i].gameObject);
        }
    }


    /// <summary>
    /// Updates the debug display for a specific node in the grid.
    /// </summary>
    /// <param name="node">The node data</param>
    public void updateDebugNode(Node node) {
        if (!MyGrid.isInGrid(node.coord)) return; // This function can be called with an invalid Node

        GameObject debugObj = debugGrid[node.coord.x, node.coord.y];
       

        //Do not preview collapsed nodes
        debugObj.SetActive(!node.isCollapsed);
        if (node.isCollapsed) {
            return;
        }

        //Update the preview for non collasped nodes
        //Debug.Log($"In updateDebugDisplay entropy at ({x}, {y}) is {grid[x, y].entropy}");
        TextMeshPro entropyText = debugObj.GetComponentInChildren<TextMeshPro>();
        float roundedEntropy = (float)Math.Round(node.entropy, 1);
        entropyText.text = roundedEntropy.ToString();

        previewPossConnections(debugObj, node);
    }

    /// <summary>
    /// Refreash the whole debug grid. This can be in the case of a backtrack or a new layer being worked on.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="cnts"></param>
    //TODO Counters really shouldn't be there
    public void updateDebugGrid(MyGrid grid) {
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                updateDebugNode(grid.nodeGrid[x, y]);
            }
        }
    }

    public void highlightNextNodeToCollapse(Vector2Int collapseNodeCoord) {
        if (!MyGrid.isInGrid(collapseNodeCoord)) return; // This function can be called with an invalid Node

        TextMeshPro currEntropyText;
        //TODO still need if statment
        if (!MyGrid.isInGrid(collapseNodeCoord)) return;
        currEntropyText = debugGrid[collapseNodeCoord.x, collapseNodeCoord.y].GetComponentInChildren<TextMeshPro>();


        if (prevEntropyText != null) {
            prevEntropyText.color = Color.white;
        }

        prevEntropyText = currEntropyText;
        currEntropyText.color = Color.green;
    }

    public void highlightSavedNodes(List<Node> collapsedNodes) {
        TextMeshPro entropyText;
        foreach (Node node in collapsedNodes) {
            entropyText = debugGrid[node.coord.x, node.coord.y].GetComponentInChildren<TextMeshPro>();
            entropyText.color = Color.clear;
        }
    }

    //Only preview the first 9 most probale tiles
    void previewPossConnections(GameObject debugObj, Node node) {
        const float nodeWH = 1;
        float padding = 0.01f;
        float sizeSubGrid = (float)(0.3 * nodeWH);
        float baseScale = sizeSubGrid - padding;
        

        //TODO ELERT NOTE HARD CODDED VALUE WHICH WILL TOTALLY NOT CAUSE PROBLEMS
        Transform tileParent = debugObj.transform.GetChild(1);
        destroyDebug(tileParent); //prevent gameobject pileup

        //Unity does not support priority queue
        List<TileData> possConnections = new List<TileData>(node.possConnections);
        if (possConnections.Count == 0) return;

        //Dispaly the list in descending order of weight
        //TODO This doesnt account for same tile bias
        possConnections.Sort((a, b) => b.weight.CompareTo(a.weight));
        float largestWeight = possConnections[0].weight;

        //will generate a 3x3 grid from left to right, top to bottom
        foreach(Vector2Int offset in debugTileOffsets) {
            //No more possible tiles to preview
            if (possConnections.Count == 0) return;

            float x  = offset.x * sizeSubGrid;
            float y = offset.y * sizeSubGrid;
           

            GameObject tileDisplay = new GameObject($"{possConnections[0].name} {x}, {y}"); 
            tileDisplay.transform.parent = tileParent;
            tileDisplay.transform.localPosition = new Vector3(x, y);
            SpriteRenderer sr = tileDisplay.AddComponent<SpriteRenderer>();
            sr.sprite = possConnections[0].tile.sprite;
            //TODO NOTE HARD CODED VALUES
            sr.sortingLayerName = "Grids";
            sr.sortingOrder = 100;

            float weightedScale = baseScale * (possConnections[0].weight / largestWeight);
            tileDisplay.transform.localScale = new Vector3(weightedScale, weightedScale);

            possConnections.RemoveAt(0);
        }
    }

    //TODO
    //Toggling item off, breaks references and stops them from being updated.
    //GetComponent<Renderer>().enabled = false
    //GetComponent<CanvasRenderer>().cull = false
    //NVM it seems to work fine as is

    //Just disable the parent?
    public void toggleDebug() {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }
}
