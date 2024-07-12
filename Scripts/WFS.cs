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

    [SerializeField] List<TileData> allConnections; //TODO

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
                //making a copy to prevent nodes affecting each other
                grid[x, y] = new Node(new List<TileData>(allConnections), new Vector2Int(x,y));
            }
        }
        debugWFS.updateDebugDisplay(grid);
    }

    public void WFC() {
        Node nodeToCollapse;
        do {
            nodeToCollapse = FindLowestEntropyNode(grid);
            Debug.Log($"Node to collapse Entropy: {nodeToCollapse.entropy}");
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
        node.collapsed = true;
        tilemap.SetTile(new Vector3Int(node.coord.x, node.coord.y), chosenTile.tile);

        Propogate(node);
    }

    //Update the tile restrictions to neighbouring nodes
    void Propogate(Node node) {
        Vector2Int coord = node.coord;
        Vector2Int newCoord;
        Node neighborNode;

        for(int i = 0; i<offsets.Length; i++) {
            newCoord = coord + offsets[i];
            if (!isInGrid(newCoord)) continue;
            neighborNode = grid[newCoord.x, newCoord.y];
            if (neighborNode.collapsed) continue;

            Debug.Log($"Node: {node.coord} propogating to neighbor {neighborNode.coord}");

            //TODO check that this is correct
            switch (i) {
                case 1: whittleConnections(neighborNode, node.connections[0].accD); break; //up
                case 2: whittleConnections(neighborNode, node.connections[0].accU); break; //down
                case 3: whittleConnections(neighborNode, node.connections[0].accR); break; //left
                case 4: whittleConnections(neighborNode, node.connections[0].accL); break; //right
            }
        }
    }

    void whittleConnections(Node neighborNode, List<TileData> restrictions) {
        //copy is created to deal with deleating elements while iterating over list
        List<TileData> copyNeighborConnections = new List<TileData>(neighborNode.connections); 

        foreach (TileData possibleConnection in copyNeighborConnections) {
            if (!restrictions.Contains(possibleConnection)) {
                neighborNode.connections.Remove(possibleConnection);
                neighborNode.entropy--;
            }
        }
    }

    bool isInGrid(Vector2Int coord) {
        if (coord.x < 0 || coord.y < 0) return false; //underflow
        //co-ordinates starts count from 0
        if (coord.x > (width-1) || coord.y > (height-1)) return false; //overflow
        return true;
    }

    Node FindLowestEntropyNode(Node[,] grid) {
        int lowestEntropy = int.MaxValue;
        Node node = new Node();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (grid[x, y].entropy == 0) continue; //already collapsed
                if (grid[x, y].entropy < lowestEntropy) {
                    lowestEntropy = grid[x, y].entropy;
                    node = grid[x, y];
                }
            }
        }
        return node;
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

    
