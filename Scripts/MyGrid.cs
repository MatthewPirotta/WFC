using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

//TODO struct?
public class MyGrid {
    //All these nums are arbritraty 
    public const int WIDTH = 10;
    public const int HEIGHT = 8;
    public static int AREA => WIDTH * HEIGHT;

    public List<TileData> allConnections { get; private set; }  //TODO static or maybe extract all this data into a struct?

    public Node[,] nodeGrid { get; set; } = new Node[WIDTH, HEIGHT];

    //TOOD If I add another counter, consider a struct (nuhhhh?)
    public int itrCnt { get; set; }

    public int backtrackCntRelBackup { get; set; } //only working grid makes use of this, backup grid does not 

    public MyGrid() {
        this.itrCnt = 0;
        this.backtrackCntRelBackup = 0;
        allConnections = LoadAllTileData();

        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                //making a new instance to prevent nodes affecting each other (stop unintential pass by reference
                nodeGrid[x, y] = new Node(new List<TileData>(allConnections), new Vector2Int(x, y));
            }
        }
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

    public static void backupGrid(MyGrid workingGrid, MyGrid backupGrid) {
        // Debug.Log($"Backup is being performed on relative iteration:{itrCnt}");

        //Deep copy (by value must be made of the grid and it's elements)
        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                backupGrid.nodeGrid[x, y] = new Node(workingGrid.nodeGrid[x, y]);
            }
        }
        backupGrid.itrCnt = workingGrid.itrCnt;
        workingGrid.backtrackCntRelBackup = 0;
    }

    public static void backTrack(MyGrid workingGrid, MyGrid backupGrid) {
       // Debug.Log("back track is being performed");

        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                workingGrid.nodeGrid[x, y] = new Node(backupGrid.nodeGrid[x, y]);
            }
        }
        workingGrid.itrCnt = backupGrid.itrCnt;
        workingGrid.backtrackCntRelBackup++;
        MyGridRenderer.redrawTileMap(workingGrid.nodeGrid);
    }

    //TODO replace old code in other classes with this
    public List<Node> getNeighbors(Node startingNode) {
       List<Node> neighbors = new List<Node>();
        Vector2Int newCoord;
        foreach (char dir in TileData.directions) {
            newCoord = startingNode.coord + TileData.getOffset(dir);
            if (!isInGrid(newCoord)) continue;
            neighbors.Add(nodeGrid[newCoord.x, newCoord.y]);
       }
       return neighbors;
    }

    public static bool isInGrid(Vector2Int coord) {
        if (coord.x < 0 || coord.y < 0) return false; //underflow
                                                      //co-ordinates starts count from 0
        if (coord.x > (WIDTH - 1) || coord.y > (HEIGHT - 1)) return false; //overflow
        return true;
    }
}
