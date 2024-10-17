using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MyGrid : IGrid {
    public const int HEIGHT = 20;
    public const int WIDTH = 20;
    public int Area => WIDTH * HEIGHT;
    public Layer layer { get; private set; }

    public Node[,] nodeGrid { get; set; } = new Node[WIDTH, HEIGHT];
    public Node[,] nodeGridBackup { get; private set; } = new Node[WIDTH, HEIGHT];//This solution is memory intensive.

    List<TileData> allPossConns = new List<TileData>(); //TOOD cringe
    //TODO also not happy with how classes interact with each other

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

    public void backtrack() {
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


    public IEnumerable<Node> getAllNodes() {
        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                yield return nodeGrid[x, y];
            }
        }
    }

    /// <summary>
    /// Finds and returns the node with the lowest entropy in the grid.
    /// </summary>
    /// <returns>
    /// The <see cref="Node"/> with the lowest entropy value that has not been collapsed yet. 
    /// Returns null if all nodes are collapsed.
    /// </returns>
    public Node getLowestEntropyNode() {
        float lowestEntropy = float.MaxValue;

        Node lowestEntropyNode = null;

        Node tempNode;
        for (int x = 0; x < WIDTH; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                tempNode = nodeGrid[x, y];
                if (tempNode.isCollapsed) continue;

                if (tempNode.entropy < lowestEntropy) {
                    lowestEntropy = tempNode.entropy;
                    lowestEntropyNode = tempNode;
                }
            }
        }
        return lowestEntropyNode;
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

    public bool isInGrid(Vector2Int coord) {
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

    public bool Equals(IGrid other) {
        if (other == null) return false;
        if (other is not MyGrid) return false;

        //This assumes that there is only one grid per layer
        if (this.layer == other.layer) return true;

        return false;
    }
}