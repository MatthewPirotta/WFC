using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class TileSetManager
{
    public static List<TileData> LoadTileSetData(string path) {
        // Load all assets of type TileData from the specified folder
        string[] assetGuids = AssetDatabase.FindAssets("t:TileData", new[] { path });
        List<TileData> tileSet = new List<TileData>();

        foreach (string guid in assetGuids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TileData tileData = AssetDatabase.LoadAssetAtPath<TileData>(assetPath);
            if (tileData != null) {
                tileSet.Add(tileData);
            }
        }

        // Output the list or do something with it
        //Debug.Log($"Loaded {tileSet.Count} TileData assets from {path}");

        return tileSet;
    }
}
