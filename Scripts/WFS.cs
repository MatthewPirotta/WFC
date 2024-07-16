using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFS : MonoBehaviour {
    //All these nums are arbritraty 
    const int width = 20;
    const int height = 15;
    const int area = width * height;
    const int backupInterval = area / 10; //makes backup in 1/10th intervals
    const long maxItr = area * 4; // stop after what would be generating the world 3 times (ignoring all generation failures)

    [SerializeField] Tilemap tilemap;
    Node[,] grid = new Node[width, height];
    int relIterCnt, totIterCnt = 0;

    //Note this solution is memory intensive O(n^2)
    Node[,] gridBackup = new Node[width, height]; 
    int iterCntBackup = 0;

    [Tooltip("This is automatically being loaded from Assets/TileData")]
    [SerializeField] List<TileData> allConnections;

    [SerializeField] DebugWFS debugWFS;
    public bool collapseAll = false;

    void Start() {
        InitGrid();
        WFC();
    }

    public void InitGrid() {
        allConnections = TileData.LoadAllTileData();
        tilemap.ClearAllTiles();
        relIterCnt = 0;
        totIterCnt = 0;
        iterCntBackup = 0;

        debugWFS.initDebug();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                //making a new instance to prevent nodes affecting each other (stop unintential pass by reference)
                grid[x, y] = new Node(new List<TileData>(allConnections), new Vector2Int(x,y));
                gridBackup[x,y] = new Node(new List<TileData>(allConnections), new Vector2Int(x, y));
            }
        }
        debugWFS.updateDebugDisplay(grid);
    }

    public void WFC() {
        Node nodeToCollapse;
        do {
            //counters are done before trying to collapse,
            //to prevent infinite loops, where there a non solveable checkpoint is created
            relIterCnt++;
            totIterCnt++;

            nodeToCollapse = FindLowestEntropyNode(grid);
            //Debug.Log($"Node to collapse Entropy: {nodeToCollapse.entropy}");
            if (nodeToCollapse.isCollapsed) break; //No more nodes to collapse

            if(nodeToCollapse.entropy == 0) {
                Debug.LogWarning($"Node {nodeToCollapse.coord} failed to collapse");
                backTrack(grid, gridBackup);
                continue;
            }
           
            CollapseNode(nodeToCollapse);
           // Debug.Log("calling update Debug Display");
            debugWFS.updateDebugDisplay(grid);
           
            //TODO techinally off by 1, but who really cares
            if (relIterCnt % backupInterval == 0) {
                backupGrid(grid,gridBackup);
            }

            Debug.Log(totIterCnt);

            //stop infinite loops
            if(totIterCnt >= maxItr) {
                Debug.Log("Reach max iteration");
            }
        } while (collapseAll && totIterCnt < maxItr);
    }

    //select random tile to collape Node
    void CollapseNode(Node node) {
        //TODO make this weighted
        //extra weight for same tile
        //weighting in general to control world gen / biomes
        TileData chosenTile = node.connections[Random.Range(0, node.connections.Count)];


        //Debug.Log($"node count: {node.connections.Count}");
        //Debug.Log($"randNum: {Random.Range(0, node.connections.Count)}");
        node.connections.Clear();
        node.connections.Add(chosenTile);
        node.entropy = 0;
        node.isCollapsed = true;
        tilemap.SetTile(new Vector3Int(node.coord.x, node.coord.y), chosenTile.tile);

        Propogate(node);
    }

    void chooseTile(Node node) {

    }

    //Update the tile restrictions to neighbouring nodes
    void Propogate(Node node) {
        Vector2Int newCoord;
        Node neighborNode;

        foreach(char dir in TileData.directions) {
            newCoord = node.coord + TileData.getOffset(dir);

            if (!isInGrid(newCoord)) continue;
            neighborNode = grid[newCoord.x, newCoord.y];
            if (neighborNode.isCollapsed) continue;

            //Debug.Log($"Node: {node.coord} propogating to neighbor {neighborNode.coord}");

            whittleConnections(neighborNode, node.connections[0].getAccDir(dir));
        }

        bool isInGrid(Vector2Int coord) {
            if (coord.x < 0 || coord.y < 0) return false; //underflow
                                                          //co-ordinates starts count from 0
            if (coord.x > (width - 1) || coord.y > (height - 1)) return false; //overflow
            return true;
        }
    }

    void whittleConnections(Node neighborNode, List<TileData> sourceNodeRestrictions) {
        //copy is created to deal with deleating elements while iterating over list
        List<TileData> copyNeighborConnections = new List<TileData>(neighborNode.connections); 

        foreach (TileData possibleConnection in copyNeighborConnections) {
            if (!sourceNodeRestrictions.Contains(possibleConnection)) {
                neighborNode.connections.Remove(possibleConnection);
                neighborNode.entropy--;
            }
        }

       /*
        string temp = $"Neighbor Node: {neighborNode.coord},\nSource Node Restrictions:";
     
        foreach (TileData restrictions in sourceNodeRestrictions) {
            temp += $"{restrictions.name}, ";
        }

        temp += "\nRemianing conenctions:";

        foreach (TileData possibleConnection in neighborNode.connections) {
            temp += $"{possibleConnection.name}, ";
        }
        Debug.Log(temp);
       */
    }

    //TODO can be optimised with a list but nahhhh
    Node FindLowestEntropyNode(Node[,] grid) {
        int lowestEntropy = int.MaxValue;
        Node LowestEntropyNode = new Node();

        // if no uncollapsed node is found, then generation is complete
        // This is done to work with code within WFC()
        // TODO this code could be cleaner
        LowestEntropyNode.isCollapsed = true; 
        Node tempNode;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tempNode = grid[x, y];
                if (tempNode.isCollapsed) continue;
                if (tempNode.entropy == 0) return tempNode; // generation failed

                if (tempNode.entropy < lowestEntropy) {
                    lowestEntropy = grid[x, y].entropy;
                    LowestEntropyNode = grid[x, y];
                }
            }
        }
        return LowestEntropyNode;
    }

    void backupGrid(Node[,] grid, Node[,] gridBackup) {
        Debug.Log($"Backup is being performed on iteration:{relIterCnt}");
        //Deep copy (by value must be made of the grid and it's elements)
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                gridBackup[x, y] = new Node(grid[x, y]);
            }
        }
        iterCntBackup = relIterCnt;
    }

    void backTrack(Node[,] grid, Node[,] gridBackup) {
        Debug.Log("back track is being performed");
        revertToGridBackup(grid, gridBackup);
        redrawTileMap();
        

        void revertToGridBackup(Node[,] grid, Node[,] gridBackup) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    grid[x, y] = new Node(gridBackup[x, y]);
                }
            }
            relIterCnt = iterCntBackup;
        }

        void redrawTileMap() {
            tilemap.ClearAllTiles();
            Node node;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    node = grid[x, y];
                    if (!node.isCollapsed) continue;
                    tilemap.SetTile(new Vector3Int(node.coord.x, node.coord.y), node.connections[0].tile);
                }
            }
        }
    }


    // Debuging 

    public void toggleDebugDisplay() {
        debugWFS.toggleDebug();
    }

    public void printEntropy() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Debug.Log($"{x},{y}: entropy = {grid[x, y].entropy}");
            }
        }
    }
}

    
