using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "WFCTile", menuName = "ScriptableObjects/TileData")]
[SerializeField]
public class TileData : ScriptableObject
{
    public Tile tile;
    public const string directions = "UDLR"; //Used in for loops to traverse all possible direction


    //Order Matters
    //First edge type is what is on 'top'/'right'
    //Second edge type is what is on 'bottom'/'left'
    //if two edge types are parrel then sort acording to other cardinal direction
    public List<EdgeType> edgeU;
    public List<EdgeType> edgeD;
    public List<EdgeType> edgeL;
    public List<EdgeType> edgeR;

    //Accepted connections for each direction
    public List<TileData> accU; 
    public List<TileData> accD;
    public List<TileData> accL;
    public List<TileData> accR;


    public List<EdgeType> getEdge(char dir) {
        switch (dir) {
            case 'U': return edgeU;
            case 'D': return edgeD;
            case 'L': return edgeL;
            case 'R': return edgeR;
            default: Debug.LogError("OH no"); return null;
        }
    }

    public void setAccDir(List<TileData> AccDir, char dir) {
        switch (dir) {
            case 'U': accU = AccDir; break;
            case 'D': accD = AccDir; break;
            case 'L': accL = AccDir; break;
            case 'R': accR = AccDir; break;
        }
    }

    public List<TileData> getAccDir(char dir) {
        switch (dir) {
            case 'U': return accU;
            case 'D': return accD;
            case 'L': return accL;
            case 'R': return accR;
            default: Debug.LogError("OH no"); return null;
        }
    }

    public static char getOppDir(char dir) {
        switch (dir) {
            case 'U': return 'D';
            case 'D': return 'U';
            case 'L': return 'R';
            case 'R': return 'L';
            default: Debug.LogError("OH no"); return '-';
        }
    }

    public static Vector2Int getOffset(char dir) {
        switch (dir) {
            case 'U': return Vector2Int.up;
            case 'D': return Vector2Int.down;
            case 'L': return Vector2Int.left;
            case 'R': return Vector2Int.right; 
        }
        Debug.LogError("OH no");
        return Vector2Int.zero;
    }

    public static List<TileData> LoadAllTileData() {
        // Path to the folder containing your ScriptableObjects
        string path = "Assets/TileData";

        // Load all assets of type TileData from the specified folder
        string[] assetGuids = AssetDatabase.FindAssets("t:TileData", new[] { path });
        List<TileData> tileCompendium = new List<TileData>();

        foreach (string guid in assetGuids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TileData tileData = AssetDatabase.LoadAssetAtPath<TileData>(assetPath);
            if (tileData != null) {
                tileCompendium.Add(tileData);
            }
        }

        // Output the list or do something with it
        //Debug.Log($"Loaded {tileCompendium.Count} TileData assets from {path}");

        return tileCompendium;
    }
}

public enum EdgeType{
    Water,
    Sand
}