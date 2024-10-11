using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MyGrid {
    public const int HEIGHT = 13;
    public const int WIDTH = 21;
    public static int AREA => WIDTH * HEIGHT;
    public Layer layer { get; private set; }

    public Node[,] nodeGrid { get; set; } = new Node[WIDTH, HEIGHT];
    public Node[,] nodeGridBackup { get; private set; } = new Node[WIDTH, HEIGHT];//This solution is memory intensive.

    List<TileData> allPossConns = new List<TileData>(); //TOOD cringe
    //TODO not really happy how dealing with multiple grid layers 
    //TODO also not happy wuth how classes interact with each other


    public MyGrid(List<TileData> allPossConns, Layer layer) {
        this.allPossConns = allPossConns;
        this.layer = layer;
        initGrid(allPossConns);
    }

    public MyGrid(){
        //did comment cos needed to initilse backupgrid
        //if (allPossConns == null) Debug.LogWarning("Null poss conns");
        initGrid(allPossConns);
    }

    /*
    //Clone deepcopy
    public MyGrid(MyGrid grid) {
        //These are constant and can be left as shallow copy
        this.allPossConns = grid.allPossConns;
        this.layer = grid.layer;

        this.nodeGrid = new Node[WIDTH, HEIGHT];
        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                //makes use of node deep copy
                this.nodeGrid[x, y] = new Node(grid.nodeGrid[x, y]);
            }
        }

        this.nodeGridBackup = new Node[,]<grid.nodeGridBackup>;
    }
    */

    private void initGrid(List<TileData> allPossConns) {
        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                //making a new instance to prevent nodes affecting each other (stop unintential pass by reference)
                nodeGrid[x, y] = new Node(new List<TileData>(allPossConns), new Vector2Int(x, y));
            }
        }
    }

    //TODO this technically can be improved
    //no need to recopy already backuped nodes
    public void backupGrid(){
        // Debug.Log($"Backup is being performed on relative iteration:{itrCnt}");

        //Deep copy (by value must be made of the grid and it's elements)
        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                nodeGridBackup[x, y] = new Node(nodeGrid[x, y]);
            }
        }
    }

    public void backTrack() {
       // Debug.Log("back track is being performed");

        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                nodeGrid[x, y] = new Node(nodeGridBackup[x, y]);
            }
        }
        MyGridRenderer.redrawTileMap(nodeGrid, layer);
    }

    public Node getNode(Vector2Int coord) {
        if (!isInGrid(coord)) {
            Debug.LogError($"Coord:{coord} is out of bounds");
            return null;
        }
        return nodeGrid[coord.x, coord.y];
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

    public void Clear() {
        initGrid(allPossConns);
        if(nodeGridBackup != null) {
            Array.Clear(nodeGridBackup, 0, nodeGridBackup.Length);
        }
    }

}

//NOTE Sorted first to last in generation order
public enum Layer {
    MIDDLE,
    BOTTOM,
    TOP
}