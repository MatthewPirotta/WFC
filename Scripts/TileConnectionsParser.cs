using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class TileConnectionsParser {

    const int directionsIndex = 1;
    const string directions = "UDLR";
    

    [MenuItem("Tools/Load TileData")]
    public static void setConnecetions() {
        List<TileData> tileCompendium = LoadTileData();
        //Dictionary<char, List<TileData>> tileDirDict = convertTileCompToDict(tileCompendium);

        foreach (TileData tileData in tileCompendium) {
            //string directions = getTileDirections(tileData); TODO REMOVE, depricate code from when directions were still a thing

            foreach (char dir in directions) {
                List<TileData> validConnections = getValidConnections(tileData.getEdge(dir), tileCompendium, dir);
                tileData.setAccDir(validConnections, dir);
                EditorUtility.SetDirty(tileData); // Mark as dirty to ensure changes are saved
            }
        }
        //TODO isn't actually saving after closing unity
        AssetDatabase.SaveAssets();
    }

    static List<TileData> LoadTileData() {
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
         Debug.Log($"Loaded {tileCompendium.Count} TileData assets from {path}");

        return tileCompendium;
    }

    //static string getTileDirections(TileData tileData) {
    //   string tileName = tileData.name;
    //    string directions = tileName.Split("_")[directionsIndex];
    //    return directions;
    //}

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
    

    //TODO im not sure if this is correct 
    static bool isValidConnecction(List<EdgeType> tileEdge, List<EdgeType> neighborTileEdge, char dir) {
        //cant match mixed tile edge with non mixed tile edge
        if (tileEdge.Count != neighborTileEdge.Count) return false;

        //co-linear tiles; intended solution (sand top, water bottom; propigate itself left and right)
        //TODO this can be problem matic take for example (sand top, water bottom; and propigate istelf up)
        if (tileEdge.SequenceEqual(neighborTileEdge)) return true;

       // if (compareEdges(tileEdge, neighborTileEdge, dir)) return true;


        return false;
    }

    //TODO rename/refactor
    //TODO put im tileData class?
    //compares if the edge type of tile matches with the neighbors edge type
    static bool compareEdges(List<EdgeType> edge, List<EdgeType> neighborEdge, char dir) {
        switch (dir) {
            case 'U': return edge[0] == neighborEdge.Last();
            case 'D': return edge.Last() == neighborEdge[0];
            case 'L': return edge[0] == neighborEdge.Last();
            case 'R': return edge.Last() == neighborEdge[0];
        }
        Debug.LogWarning("Oh no");
        return false;
    }

}

