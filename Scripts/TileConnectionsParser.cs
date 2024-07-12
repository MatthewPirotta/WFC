using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class TileConnectionsParser {

    const int directionsIndex = 1;

    [MenuItem("Tools/Load TileData")]
    public static void setConnecetions() {
        List<TileData> tileCompendium = LoadTileData();
        Dictionary<char, List<TileData>> tileDirDict = convertTileCompToDict(tileCompendium);


        foreach (TileData tileData in tileCompendium) {
            string directions = getTileDirections(tileData);

            foreach (char dir in directions) {
                char oppDir = TileData.getOppDir(dir);
                List<TileData> validConnections = getValidConnections(tileData.getEdge(dir), tileDirDict, oppDir);
                tileData.setAccDir(validConnections, dir);
            }
        }

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
        //Debug.Log($"Loaded {tileDataList.Count} TileData assets from {path}");

        return tileCompendium;
    }

    static string getTileDirections(TileData tileData) {
        string tileName = tileData.tile.name;
        string directions = tileName.Split("_")[directionsIndex];
        return directions;
    }

    //This is being done to support varients of same tile
    static Dictionary<char, List<TileData>> convertTileCompToDict(List<TileData> tileCompendium) {
        Dictionary<char, List<TileData>> tileDirDict = new Dictionary<char, List<TileData>>();
        
        foreach(TileData tile in tileCompendium) { 
             foreach(char direction in getTileDirections(tile)) {
                if (!tileDirDict.ContainsKey(direction)) {
                    tileDirDict[direction] = new List<TileData>();
                }
                tileDirDict[direction].Add(tile);
             }
        }
        return tileDirDict;
    }

    //checks for tile edge type
    static List<TileData> getValidConnections(List<EdgeType> tileEdge, Dictionary<char, List<TileData>> tileDirDict, char oppDir) {
        List<TileData> possibleNeighbours = tileDirDict[oppDir];
        List<TileData> validNeighbours = new List<TileData>();

        //TODO type checking not goods
        foreach(TileData neighbor in possibleNeighbours) {
            if (isValidConnecction(tileEdge,neighbor.getEdge(oppDir),oppDir)) {
                validNeighbours.Add(neighbor);
            }
        }

        return validNeighbours;
    }

    //TODO im not sure if this is correct
    static bool isValidConnecction(List<EdgeType> tileEdge, List<EdgeType> neighborTileEdge, char oppDir) {
        if (tileEdge.SequenceEqual(neighborTileEdge)) return true; //co-linear tiles border tiles

        if(tileEdge.Count == 1 && neighborTileEdge.Count == 1) {
            if (compareEdges(tileEdge, neighborTileEdge, oppDir)) return true; //
        }
        return false;
    }

    //TODO put im tileData class?
    //compares if the edge type of tile matches with the neighbors edge type
    static bool compareEdges(List<EdgeType> edge, List<EdgeType> neighborEdge, char oppDir) {
        //TODO check
        switch (oppDir) {
            case 'U': return edge[0] == neighborEdge.Last();
            case 'D': return edge.Last() == neighborEdge[0];
            case 'L': return edge[0] == neighborEdge.Last();
            case 'R': return edge.Last() == neighborEdge[0];
        }
        Debug.LogWarning("Oh no");
        return false;
    }

}

