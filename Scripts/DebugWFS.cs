using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

//[ExecuteAlways]
public class DebugWFS : MonoBehaviour
{
    //TODO circular referance
    [SerializeField] WFS wfs;

    [SerializeField] GameObject debugPrefab;
    [SerializeField] Transform debugParent;
    GameObject[,] debugGrid = new GameObject[MyGrid.WIDTH, MyGrid.HEIGHT];

    [SerializeField] TextMeshProUGUI totItrTxt;
    [SerializeField] TextMeshProUGUI itrsRelBackupTxt;
    [SerializeField] TextMeshProUGUI totBacktrackCntTxt;
    [SerializeField] TextMeshProUGUI seedTxt;

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

    public void initDebug() {
        //to prevent undefined behaviour
        destroyChildren(debugParent);

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

    private void destroyChildren(Transform gameObject) {
        // Store references to all children first
        int childCount = gameObject.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++) {
            children[i] = gameObject.GetChild(i);
        }

        // Detach children from parent to avoid modifying the collection during iteration
        gameObject.DetachChildren();

        // Destroy all children
        for (int i = 0; i < childCount; i++) {
            DestroyImmediate(children[i].gameObject);
        }
    }

    //TODO
    //This can be optimised by only chaning the updated blocks
    public void updateDebugDisplay(Node[,] grid) {
        updateDebugGrid(grid);
        updateInfoTxt();
    }

    void updateDebugGrid(Node[,] grid) {
        TextMeshPro entropyText;
        GameObject debugObj;
        float roundedEntropy;
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                debugObj = debugGrid[x, y];
                entropyText = debugObj.GetComponentInChildren<TextMeshPro>();
                //Debug.Log($"In updateDebugDisplay entropy at ({x}, {y}) is {grid[x, y].entropy}");
                roundedEntropy = (float)Math.Round(grid[x, y].entropy, 1);
                entropyText.text = roundedEntropy.ToString();

                previewTile(debugGrid[x, y], grid[x, y]);
            }
        }

        highlightCollapseNode();
    }

    void updateInfoTxt() {
        totItrTxt.text = $"Total Itr: {wfs.totIterCnt}";
        itrsRelBackupTxt.text = $"Itrs Rel Backtrack: {wfs.workingGrid.itrCnt}";
        totBacktrackCntTxt.text = $"Total Back Track Count: {wfs.totalBackupCnt}";
        seedTxt.text = $"Seed: {wfs.seed}";
    }

    void highlightCollapseNode() {
        TextMeshPro currEntropyText;
        Vector2Int coord = wfs.FindLowestEntropyNode(wfs.workingGrid.nodeGrid).coord;
        if (!MyGrid.isInGrid(coord)) return;
        currEntropyText = debugGrid[coord.x, coord.y].GetComponentInChildren<TextMeshPro>();


        if (prevEntropyText != null) {
            prevEntropyText.color = Color.white;
        }

        prevEntropyText = currEntropyText;
        currEntropyText.color = Color.green;
    }

    //Again very inneficient
    //Only preview the first 9 most probale tiles
    void previewTile(GameObject debugObj, Node node) {
        const float nodeWH = 1;
        float padding = 0.01f;
        float sizeSubGrid = (float)(0.3 * nodeWH);
        float baseScale = sizeSubGrid - padding;
        

        //TODO ELERT HARD CODDED VALUE WHICH WILL TOTALLY NOT CAUSE PROBLEMS
        Transform tileParent = debugObj.transform.GetChild(1);
        destroyChildren(tileParent); //prevent gameobject pileup

        //Unity does not support priority queue
        List<TileData> possConnections = new List<TileData>(node.possConnections);
        //Dispaly the list in descending order of weight
        //TODO This doesnt account for same tile bias
        possConnections.Sort((a, b) => b.weight.CompareTo(a.weight));
        float largestWeight = possConnections[0].weight;

        //will generate a 3x3 grid from left to right, top to bottom
        foreach(Vector2Int offset in debugTileOffsets) {
            if (possConnections.Count == 0) return;
            float x  = offset.x * sizeSubGrid;
            float y = offset.y * sizeSubGrid;

            GameObject tileDisplay = new GameObject($"{possConnections[0].name} {x}, {y}");
            tileDisplay.transform.parent = tileParent;
            tileDisplay.transform.localPosition = new Vector3(x, y);
            SpriteRenderer sr = tileDisplay.AddComponent<SpriteRenderer>();
            sr.sprite = possConnections[0].tile.sprite;

            float weightedScale = baseScale * (possConnections[0].weight / largestWeight);
            tileDisplay.transform.localScale = new Vector3(weightedScale, weightedScale);

            possConnections.RemoveAt(0);
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
