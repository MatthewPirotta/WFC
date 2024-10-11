using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class TileConnectionsParser {
    [MenuItem("Tools/Load TileData")]
    public static void LoadTileData() {
        setConnecetions(TileSetManager.LoadTileSetData("Assets/TileData/TopTileData"));
        setConnecetions(TileSetManager.LoadTileSetData("Assets/TileData/MidTileData"));
        setConnecetions(TileSetManager.LoadTileSetData("Assets/TileData/BotTileData"));
    }

    public static void setConnecetions(List<TileData> tileSet) {
        //Dictionary<char, List<TileData>> tileDirDict = convertTileCompToDict(tileCompendium);

        foreach (TileData tileData in tileSet) {
            foreach (char dir in TileData.directions) {
                List<TileData> validConnections = getValidConnections(tileData.getEdge(dir), tileSet, dir);
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
    static List<TileData> getValidConnections(List<EdgeType> tileEdge, List<TileData> tileSet, char dir) {
        char oppDir = TileData.getOppDir(dir); 
        List<TileData> validNeighbours = new List<TileData>();

        foreach(TileData neighbor in tileSet) {
            if (isValidConnecction(tileEdge,neighbor.getEdge(oppDir))) {
                validNeighbours.Add(neighbor);
            }
        }

        return validNeighbours;
    }
    

    //Make sure bordering edgeTypes match
    static bool isValidConnecction(List<EdgeType> tileEdge, List<EdgeType> neighborTileEdge) {
        //cant match mixed tile edge with non mixed tile edge
        if (tileEdge.Count != neighborTileEdge.Count) return false;

        //EdgeType Sequence must be identical, especially the order
        if (tileEdge.SequenceEqual(neighborTileEdge)) return true;

        return false;
    }
}

