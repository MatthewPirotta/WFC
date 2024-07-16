using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//[ExecuteAlways]
public class WFS : MonoBehaviour {
    const int width = 10;
    const int height = 10;

    private Vector2Int[] offsets = new Vector2Int[] {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    [SerializeField] Tilemap tilemap;
    Node[,] grid = new Node[width, height];

    [SerializeField] List<TileData> allConnections; //TODO, automate

    [SerializeField] DebugWFS debugWFS;
    public bool collapseAll = false;

    void Start() {
        InitGrid();
        WFC();
    }

    public void InitGrid() {
        tilemap.ClearAllTiles();
        debugWFS.initDebug();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                //making a new instance to prevent nodes affecting each other (stop unintential pass by reference)
                grid[x, y] = new Node(new List<TileData>(allConnections), new Vector2Int(x,y));
            }
        }
        debugWFS.updateDebugDisplay(grid);
    }

    public void WFC() {
        Node nodeToCollapse;
        do {
            nodeToCollapse = FindLowestEntropyNode(grid);
            //Debug.Log($"Node to collapse Entropy: {nodeToCollapse.entropy}");
            if (nodeToCollapse.entropy == 0) break; //No more nodes to collapse

            CollapseNode(nodeToCollapse);
           // Debug.Log("calling update Debug Display");
            debugWFS.updateDebugDisplay(grid);
        } while (collapseAll);
    }

    //select random tile to collape Node
    void CollapseNode(Node node) {
        //TODO make this weighted
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

    //Update the tile restrictions to neighbouring nodes
    //TODO make this cleaner, use dir for the getting
    void Propogate(Node node) {
        Vector2Int newCoord;
        Node neighborNode;

        for(int i = 0; i<offsets.Length; i++) {
            newCoord = node.coord + offsets[i];
            if (!isInGrid(newCoord)) continue;
            neighborNode = grid[newCoord.x, newCoord.y];
            if (neighborNode.isCollapsed) continue;

            //Debug.Log($"Node: {node.coord} propogating to neighbor {neighborNode.coord}");

            //TODO check that this is correct
            switch (i) {
                case 0: whittleConnections(neighborNode, node.connections[0].accU); break; //up
                case 1: whittleConnections(neighborNode, node.connections[0].accD); break; //down
                case 2: whittleConnections(neighborNode, node.connections[0].accL); break; //left
                case 3: whittleConnections(neighborNode, node.connections[0].accR); break; //right
            }
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

       
        string temp = $"Neighbor Node: {neighborNode.coord},\nSource Node Restrictions:";
     
        foreach (TileData restrictions in sourceNodeRestrictions) {
            temp += $"{restrictions.name}, ";
        }

        temp += "\nRemianing conenctions:";

        foreach (TileData possibleConnection in neighborNode.connections) {
            temp += $"{possibleConnection.name}, ";
        }
        Debug.Log(temp);
    }

    bool isInGrid(Vector2Int coord) {
        if (coord.x < 0 || coord.y < 0) return false; //underflow
        //co-ordinates starts count from 0
        if (coord.x > (width-1) || coord.y > (height-1)) return false; //overflow
        return true;
    }

    //TODO can be optimised with a list but nahhhh
    Node FindLowestEntropyNode(Node[,] grid) {
        int lowestEntropy = int.MaxValue;
        Node LowestEntropyNode = new Node();
        Node tempNode;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tempNode = grid[x, y];
                if (tempNode.isCollapsed) continue;
                if (tempNode.entropy == 0) {
                    //TODO actually fix it
                    Debug.LogWarning($"Node {tempNode.coord.x},{tempNode.coord.y} failed to collapse");
                    continue;
                }

                if (tempNode.entropy < lowestEntropy) {
                    lowestEntropy = grid[x, y].entropy;
                    LowestEntropyNode = grid[x, y];
                }
            }
        }
        return LowestEntropyNode;
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

    
