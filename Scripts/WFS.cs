using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class WFS : MonoBehaviour {
    MyGrid workingGrid;
    MyGrid backupGrid;  //Note this solution is memory intensive O(n^2)

    //TODO const for backupInterval and maxItr
    int backupInterval = MyGrid.AREA / 10; //makes backup in 1/10th intervals
    long maxItr = MyGrid.AREA * 5; // stop after what would be generating the world 3 times (ignoring all generation failures)
    int maxBacktrackFailures = 10;
    [SerializeField] int totIterCnt = 0;

    [SerializeField] DebugWFS debugWFS;
    public bool collapseAll = false;

    [SerializeField] int seed = 42;
    [SerializeField] int sameTileBias = 1;
    System.Random random;

    void Start() {
        innitWorldSpace();
        WFC();
    }

    public void innitWorldSpace() {
        totIterCnt = 0;
        resetWorldSpace();
    }  

    void resetWorldSpace() {
        MyGridRenderer.tilemap.ClearAllTiles();
        workingGrid = new MyGrid();
        backupGrid = new MyGrid();
        random = new System.Random(seed);

        debugWFS.initDebug();
        debugWFS.updateDebugDisplay(workingGrid.nodeGrid);
    }

    public void WFC() {
        Node nodeToCollapse;
        do {
            //counters are done before trying to collapse,
            //to prevent infinite loops, where there a non solveable checkpoint is created
            workingGrid.itrCnt++;
            totIterCnt++;

            nodeToCollapse = FindLowestEntropyNode(workingGrid.nodeGrid);
            //Debug.Log($"Node to collapse Entropy: {nodeToCollapse.entropy}");
            if (nodeToCollapse.isCollapsed) break; //No more nodes to collapse

            //Reset world if world gen, if there is non solveable constraints after backup
            if (workingGrid.backtrackCntRelBackup == maxBacktrackFailures) {
                Debug.LogWarning("Reached max backtrack count relative to bakcup");
                resetWorldSpace();
                continue;
            }

            if(nodeToCollapse.possConnections.Count == 0) {
                Debug.Log($"Node {nodeToCollapse.coord} failed to collapse");
                MyGrid.backTrack(workingGrid, backupGrid);
                continue;
            }
           
            CollapseNode(nodeToCollapse);
           
            //TODO techinally off by 1, but who really cares
            if (workingGrid.itrCnt % backupInterval == 0) {
                MyGrid.backupGrid(workingGrid, backupGrid);
            }

            //Debug.Log(totIterCnt);

            //stop infinite loops
            if(totIterCnt >= maxItr) {
                Debug.LogWarning("Reach max iteration");
            }
        } while (collapseAll && totIterCnt < maxItr);
    }

    //select random tile to collape Node
    void CollapseNode(Node node) {
        //TODO make this weighted
        //extra weight for same tile
        //weighting in general to control world gen / biomes

        TileData chosenTile = selectWeightedRandomTile(node);
        //TileData chosenTile = node.possConnections[random.Next(0, node.possConnections.Count)];


        //Debug.Log($"node count: {node.connections.Count}");
        //Debug.Log($"randNum: {Random.Range(0, node.connections.Count)}");
        node.possConnections.Clear();
        node.possConnections.Add(chosenTile);
        node.entropy = 0;
        node.isCollapsed = true;
        MyGridRenderer.tilemap.SetTile(new Vector3Int(node.coord.x, node.coord.y), chosenTile.tile);

        Propogate(node);
        // Debug.Log("calling update Debug Display");
        //Debug.Log($"Before UpdateDebugDisplay. {node.coord} entropy: {node.entropy}");
        debugWFS.updateDebugDisplay(workingGrid.nodeGrid);
    }

    TileData selectWeightedRandomTile(Node node) {
        //calc totalWeight which is needed for weighted selection
        int totalWeight = 0;
        foreach (TileData possTile in node.possConnections) {
            totalWeight += possTile.weight;
            // account for the sameTileBias
            if (possTile.Equals(node.possConnections[0])) {
                totalWeight += sameTileBias; 
            }
        }

        int ranNum = random.Next(0, totalWeight);
        int cumulativeWeight = 0;
        foreach (TileData possTile in node.possConnections) {
            cumulativeWeight += possTile.weight;

            //making same tile more likely, to encourage larger patches
            if (possTile.Equals(node.possConnections[0])) {
                cumulativeWeight += sameTileBias;
            } 
            
           
            if (cumulativeWeight > ranNum ) return possTile;
        }

        Debug.LogError(":(");
        //arbiturarily return first valid node
        return node.possConnections[0];
    }

    //Update the tile restrictions to neighbouring nodes
    //TODO make more eleganto
    void Propogate(Node node) {
        Vector2Int newCoord;
        Node neighborNode;

        foreach (char dir in TileData.directions) {
            newCoord = node.coord + TileData.getOffset(dir);

            if (!MyGrid.isInGrid(newCoord)) continue;
            neighborNode = workingGrid.nodeGrid[newCoord.x, newCoord.y];
            if (neighborNode.isCollapsed) continue;

            //Debug.Log($"Node: {node.coord} propogating to neighbor {neighborNode.coord}");

            whittleConnections(neighborNode, node.possConnections[0].getAccDir(dir));
        }
    }

    void whittleConnections(Node neighborNode, List<TileData> sourceNodeRestrictions) {
        //Debug.Log($"WhittleConnections before calling CalcEntropy. {neighborNode.coord} entropy: {neighborNode.entropy}");
        //copy is created to deal with deleating elements while iterating over list

        printList(neighborNode.possConnections, $"Neighnor coord: {neighborNode.coord} Before Whittleing Connections\t");
        printList(sourceNodeRestrictions, "sourceNodeRestrictions:");

        List<TileData> copyNeighborConnections = new List<TileData>(neighborNode.possConnections); 

        foreach (TileData possibleConnection in copyNeighborConnections) {
            if (!sourceNodeRestrictions.Contains(possibleConnection)) {
                neighborNode.possConnections.Remove(possibleConnection);
            }
        }

        neighborNode.entropy = neighborNode.calcEntropy();

        //Debug.Log($"WhittleConnections after calling CalcEntropy. {neighborNode.coord} entropy: {neighborNode.entropy}");
        printList(neighborNode.possConnections, $"Neighnor coord: {neighborNode.coord} After Whittleing Connections\t");
    }

    //TODO can be optimised with a list but nahhhh
    Node FindLowestEntropyNode(Node[,] grid) {
        float lowestEntropy = float.MaxValue;
        Node LowestEntropyNode = new Node();

        // if no uncollapsed node is found, then generation is complete
        // This is done to work with code within WFC()
        // TODO this code could be cleaner
        LowestEntropyNode.isCollapsed = true; 
        Node tempNode;

        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                tempNode = grid[x, y];
                if (tempNode.isCollapsed) continue;

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
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                Debug.Log($"{x},{y}: entropy = {workingGrid.nodeGrid[x, y].entropy}");
            }
        }
    }

    public void printList<T>(List<T> myList, string msg = "") {
        string output = msg;
        foreach (T t in myList) {
            output += t.ToString() + ", ";
        }
        Debug.Log(output);
    }
}

    
