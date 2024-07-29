using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class TileConnectionsParser {
    [MenuItem("Tools/Load TileData")]
    public static void setConnecetions() {
        List<TileData> tileCompendium = MyGrid.LoadAllTileData();
        //Dictionary<char, List<TileData>> tileDirDict = convertTileCompToDict(tileCompendium);

        foreach (TileData tileData in tileCompendium) {
            //string directions = getTileDirections(tileData); TODO REMOVE, depricate code from when directions were still a thing

            foreach (char dir in TileData.directions) {
                List<TileData> validConnections = getValidConnections(tileData.getEdge(dir), tileCompendium, dir);
                tileData.setAccDir(validConnections, dir);
                EditorUtility.SetDirty(tileData); // Mark as dirty to ensure changes are saved
            }
        }
        AssetDatabase.SaveAssets();
    }

    //This is being done to support varients of same tile
    //And increase look up time for large tile sets
    //TODO reimplement but with edge types
    /*
    static Dictionary<char, List<TileData>> convertTileCompToDict(List<TileData> tileCompendium) {
        Dictionary<char, List<TileData>> tileDirDict = new Dictionary<char, List<TileData>>();
        
        foreach(TileData tile in tileCompendium) { 
             foreach(char direction in directions) {
                if (!tileDirDict.ContainsKey(direction)) {
                    tileDirDict[direction] = new List<TileData>();
                }
                tileDirDict[direction].Add(tile);
             }
        }
        return tileDirDict;
    }
    */

    //checks for tile edge type
    static List<TileData> getValidConnections(List<EdgeType> tileEdge, List<TileData> tileCompendium, char dir) {
        char oppDir = TileData.getOppDir(dir); 
        List<TileData> validNeighbours = new List<TileData>();
    
        //TODO type checking not goods
        foreach(TileData neighbor in tileCompendium) {
            if (isValidConnecction(tileEdge,neighbor.getEdge(oppDir),dir)) {
                validNeighbours.Add(neighbor);
            }
        }

        return validNeighbours;
    }
    

    //Make sure bordering edgeTypes match
    static bool isValidConnecction(List<EdgeType> tileEdge, List<EdgeType> neighborTileEdge, char dir) {
        //cant match mixed tile edge with non mixed tile edge
        if (tileEdge.Count != neighborTileEdge.Count) return false;

        //EdgeType Sequence must be identical, especially the order
        if (tileEdge.SequenceEqual(neighborTileEdge)) return true;

        return false;
    }
}

