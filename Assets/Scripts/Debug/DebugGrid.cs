using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//TODO this debug grid implementation is specfic to my implementation of MyGrid
[ExecuteAlways]
public class DebugGrid : MonoBehaviour {
    [SerializeField] GameObject debugPrefab;
    GameObject[,] debugGrid = new GameObject[MyGrid.WIDTH, MyGrid.WIDTH];

    IGrid grid;

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
    public void gridFactory(IGrid grid) {
        this.grid = grid;
    }

    //TODO NOTE this is being enabled multiple times in the inspector

    private void OnEnable() {
        //refreshing the whole grid since debug was last enabled
        if (grid != null) {
            //Debug.Log($"Updating! {transform.name}");
            updateDebugGrid(grid);
        }

        WFC.collapsedNode += updateDebugNode;
        WFC.updateGrid += updateDebugGrid;
        WFC.propogateNodeData += updateDebugNode;
    }

    private void OnDisable() {
        WFC.collapsedNode -= updateDebugNode;
        WFC.updateGrid -= updateDebugGrid;
        WFC.propogateNodeData -= updateDebugNode;
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
    public void updateDebugNode(IGrid collapseOngrid, Node node) {
        if (grid == null) return; // grid reference has not been intialised yet
        if (!collapseOngrid.isInGrid(node.coord)) return; // This function can be called with an invalid Node
        if (!this.grid.Equals(collapseOngrid)) return; //This function can be called by actions working on other grids. 

        GameObject debugContainer = debugGrid[node.coord.x, node.coord.y];

        //Do not preview collapsed nodes
        debugContainer.SetActive(!node.isCollapsed);
        if (node.isCollapsed) {
            return;
        }

        //Update the preview for non collasped nodes
        //Debug.Log($"In updateDebugDisplay entropy at ({x}, {y}) is {grid[x, y].entropy}");
        TextMeshPro entropyText = debugContainer.GetComponentInChildren<TextMeshPro>();
        float roundedEntropy = (float)Math.Round(node.entropy, 1);
        entropyText.text = roundedEntropy.ToString();

        previewPossConnections(debugContainer, node);
    }

    /// <summary>
    /// Refreash the whole debug grid. This can be in the case of a backtrack or a new layer being worked on.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="cnts"></param>
    //TODO Counters really shouldn't be there
    public void updateDebugGrid(IGrid grid) {
        foreach (Node node in grid.getAllNodes()) {
            updateDebugNode(grid, node);
        }
    }

    public void highlightNextNodeToCollapse(IGrid grid, Vector2Int collapseNodeCoord) {
        if (!grid.isInGrid(collapseNodeCoord)) return; // This function can be called with an invalid Node

        TextMeshPro currEntropyText;
        //TODO still need if statment
        if (!grid.isInGrid(collapseNodeCoord)) return;
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
    void previewPossConnections(GameObject debugContainer, Node node) {
        const float nodeWidthHeight = 1;
        const float padding = 0.01f;
        const float gridScaleFactor = 0.3f; // 1/3 sacle to fit 9 preview tiles

        float sizeSubGrid = (float)(gridScaleFactor * nodeWidthHeight);
        float baseScale = sizeSubGrid - padding;

        //TODO ELERT NOTE HARD CODDED VALUE WHICH WILL TOTALLY NOT CAUSE PROBLEMS
        Transform miniTileGridContainer = debugContainer.transform.Find("Tile Preview");
        destroyDebug(miniTileGridContainer); //prevent gameobject pileup

        //NOTE Unity does not support priority queue
        List<TileData> possConnections = new List<TileData>(node.possConnections);
        if (possConnections.Count == 0) return;

        //Dispaly the list in descending order of weight
        //TODO This doesnt account for same tile bias
        possConnections.Sort((a, b) => b.weight.CompareTo(a.weight));

        float largestWeight = possConnections[0].weight;

        // Check if largestWeight is zero or NaN to prevent invalid scale calculations
        if (largestWeight <= 0 || float.IsNaN(largestWeight)) {
            Debug.LogError("Invalid largestWeight: " + largestWeight);
            return;
        }

        createTileGrid(possConnections, miniTileGridContainer, sizeSubGrid, baseScale, largestWeight);

        //will generate a 3x3 grid from left to right, top to bottom
        void createTileGrid(List<TileData> possibleConnections, Transform tileParent, float sizeSubGrid, float baseScale, float largestWeight) {
            foreach (Vector2Int offset in debugTileOffsets) {
                //No more possible tiles to preview
                if (possConnections.Count == 0) return;

                Vector3 tilePosition = getTilePos(offset, sizeSubGrid);
                GameObject tileDisplay = createTileDisplay(possConnections[0], tilePosition, tileParent, baseScale, largestWeight);

                possConnections.RemoveAt(0);
            }
        }

        Vector3 getTilePos(Vector2Int offset, float sizeSubGrid) {
            float x = offset.x * sizeSubGrid;
            float y = offset.y * sizeSubGrid;
            return new Vector3(x, y, 0f);
        }

        GameObject createTileDisplay(TileData tiledata, Vector3 pos, Transform parent, float baseScale, float largestWeight) {
            GameObject tileDisplay = new GameObject($"{tiledata.name} {pos.x}, {pos.y}");
            tileDisplay.transform.parent = parent;
            tileDisplay.transform.localPosition = pos;

            SpriteRenderer sr = tileDisplay.AddComponent<SpriteRenderer>();
            sr.sprite = tiledata.tile.sprite;
            //TODO NOTE HARD CODED VALUES
            sr.sortingLayerName = "Grids";
            sr.sortingOrder = 100;

            float weightedScale = baseScale * (tiledata.weight / largestWeight);
            tileDisplay.transform.localScale = new Vector3(weightedScale, weightedScale, 1f);

            return tileDisplay;
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
