using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct Counters {
    public int totItrCnt;
    public int totBackupCnt;
    public int totBacktrackCnt;

    //Relative to last backup
    public int relItrCnt { get; set; }
    public int relBacktrackCnt { get; set; }
}

//TODO should grid backup be in WFC?

//TODO getrid of Mono
// make static/singleton? overkill imo
public class WFS : MonoBehaviour {
    List<Node> collapsedNodes = new List<Node>(); // Nodes that have been collapsed since last bakcup

    int backupInterval { get; } = MyGrid.AREA / 10; //makes backup in 1/10th intervals
    long maxItr { get; } = (MyGrid.AREA * 10); // stop after what would be generating the world 10 times (ignoring all generation failures)
    int maxBacktrackFailures { get; } = 10;

    Counters cnts = new Counters();

    public bool collapseAll = false;

    //TODO Remove
    int sameTileBias = 1;

    System.Random random;

    public static event Action initWorld; //One time call to initilise scripts
    public static event Action<Node> collapsedNode; //Node was collapsed
    public static event Action<Node> propogateNodeData; //Propogating the new constraints from the collapsed node
    public static event Action<MyGrid> updateGrid; // Updating the whole grid, this is used on backtracks
    public static event Action<Counters, Layer> updataGameState; //Providing other classes with low level data

    /*
    public static event Action<> generatingNextLayer;
    public static event Action<> backupGrid;
    public static event Action<> backtrackGrid;
    */
    
    public void initWorldSpace(MyGrid[] myGrids) {
        random = new System.Random(GameConstants.SEED);

        cnts.totItrCnt = 0;
        cnts.totBackupCnt = 0;
        cnts.totBacktrackCnt = 0;

        resetGeneration(myGrids);

        initWorld?.Invoke();
    }

    /// <summary>
    /// Reset the whole world generation after multiple failed backtracks
    /// </summary>
    /// <param name="grids"></param>
    void resetGeneration(MyGrid[] grids) {
        foreach(MyGrid grid in grids) {
            MyGridRenderer.clearTilemap((int)grid.layer);
            grid.Clear();


            //The middle grid is the first to be generated
            //All other grids are dependent on it
            if (grid.layer == Layer.MIDDLE) {
                foreach (Node node in grid.nodeGrid) {
                    node.isCollapsed = false;
                }
            }

            foreach (Node node in grid.nodeGrid) {
                node.entropy = node.calcEntropy();
            }
        }

        collapsedNodes.Clear();

        cnts.relItrCnt = 0;
        cnts.relBacktrackCnt = 0;
    }

    public void WaveFunctionCollapse(MyGrid[] grids, Layer layer) {
        MyGrid chosenGrid = grids[(int)layer];
        bool isLayerGened = false; //Is layer fully generated

        do {
            isLayerGened = performSingleIteration(grids, chosenGrid);

            //stop infinite loops
            if (cnts.totItrCnt >= maxItr) {
                Debug.Log($"maxItr {maxItr}");
                Debug.Log($"total Iterations{cnts.totItrCnt}");
                Debug.LogWarning("Reach max iteration");
            }
            //TODO NOTE collapseAll coondition
        } while (cnts.totItrCnt < maxItr && !isLayerGened);
    }

    /// <summary>
    /// Performs a single Iteration of WFC algorithm
    /// </summary>
    /// <param name="grids"></param>
    /// <param name="chosenGrid"></param>
    /// <returns>Returns true if all nodes have been collapsed on current grid layer. Otherwise returns false.</returns>
    /// NOTE TODO if resetGeneration is called the 'WorldGenerationManager' code does not hande it properly
    public bool performSingleIteration(MyGrid[] grids, MyGrid chosenGrid) {
        //counters are done before trying to collapse,
        //to prevent infinite loops, where there a non solveable checkpoint is created
        cnts.relItrCnt++;
        cnts.totItrCnt++;

        Node nodeToCollapse = findLowestEntropyNode(chosenGrid.nodeGrid);
        //Debug.Log($"Node to collapse Entropy: {nodeToCollapse.entropy}");
        if (nodeToCollapse == null || nodeToCollapse.isCollapsed) return true; //No more nodes to collapse

        //Reset world if there is non solveable constraints after backup
        if (cnts.relBacktrackCnt == maxBacktrackFailures) {
            Debug.LogWarning("Reached max backtrack count relative to bakcup");
            resetGeneration(grids);
            return false;
        }

        //Performing Backtrack
        //if node can not be collapsed
        if (nodeToCollapse.possConnections.Count == 0) {
            backtrack(grids, chosenGrid);
            return false;
        }

        collapseNode(grids, chosenGrid, nodeToCollapse);

        //Peforming Backup
        if (cnts.relItrCnt % backupInterval == 0) {
            backup(grids);
        }

        updataGameState?.Invoke(cnts, chosenGrid.layer);

        return false;

        //TODO dont need chosenGrid and can prob rework DebugGrid::updateDebugGrid interacts and runs
        ///Since layers can influence others, all grid states must be updated
        void backtrack(MyGrid[] workingGrids, MyGrid chosenGrid) {
            //Debug.Log($"Node {nodeToCollapse.coord} failed to collapse");
            cnts.totBacktrackCnt++;
            cnts.relBacktrackCnt++;

            for(int i = 0; i < workingGrids.Length; i++) {
                workingGrids[i].backTrack();
            }

            updateGrid?.Invoke(chosenGrid);
            updataGameState?.Invoke(cnts, chosenGrid.layer);
        }

        //TODO backup and backtrack needs to be on all the grids
        void backup(MyGrid[] workingGrids) {
            cnts.relItrCnt = 0;
            cnts.relBacktrackCnt = 0;
            cnts.totBackupCnt++;

            for (int i = 0; i < workingGrids.Length; i++) {
                workingGrids[i].backupGrid();
            }
        }
    }


    //select random tile to collape Node
    void collapseNode(MyGrid[] grids, MyGrid chosenGrid, Node node) {
        TileData chosenTile = selectWeightedRandomTile(node);

        //Debug.Log($"node count: {node.connections.Count}");
        //Debug.Log($"randNum: {Random.Range(0, node.connections.Count)}");
        node.possConnections.Clear();
        node.possConnections.Add(chosenTile);
        node.entropy = 0;
        node.isCollapsed = true;

        Tilemap tilemap = MyGridRenderer.getTilemap((int)chosenGrid.layer);
        tilemap.SetTile(new Vector3Int(node.coord.x, node.coord.y), chosenTile.tile);
        collapsedNodes.Add(node);

        EnableNodesOtherLayers(grids, chosenGrid, node, chosenTile);

        collapsedNode?.Invoke(node);
        updataGameState?.Invoke(cnts, chosenGrid.layer);

        Propogate(chosenGrid, node);
        // Debug.Log("calling update Debug Display");
        //Debug.Log($"Before UpdateDebugDisplay. {node.coord} entropy: {node.entropy}");
    }

    private void EnableNodesOtherLayers(MyGrid[] grids, MyGrid grid, Node node, TileData chosenTile) {
        int newLayer;
        Node newNode;
        if (grid.layer == Layer.MIDDLE) {
            if (chosenTile.enableLayerBelow) {
                newLayer = (int)Layer.BOTTOM;
                newNode = grids[newLayer].getNode(node.coord);
                newNode.isCollapsed = false;
            }
            if (chosenTile.enableLayerAbove) {
                newLayer = (int)Layer.TOP;
                newNode = grids[newLayer].getNode(node.coord);
                newNode.isCollapsed = false;
            }
        }
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
    void Propogate(MyGrid grid, Node node) {
        Vector2Int newCoord;
        Node neighborNode;

        foreach (char dir in TileData.directions) {
            newCoord = node.coord + TileData.getOffset(dir);

            if (!MyGrid.isInGrid(newCoord)) continue;
            neighborNode = grid.nodeGrid[newCoord.x, newCoord.y];
            if (neighborNode.isCollapsed) continue;

            //Debug.Log($"Node: {node.coord} propogating to neighbor {neighborNode.coord}");

            whittleConnections(neighborNode, node.possConnections[0].getAccDir(dir));
            propogateNodeData?.Invoke(neighborNode);
        }
    }

    void whittleConnections(Node neighborNode, List<TileData> sourceNodeRestrictions) {
        List<TileData> copyNeighborConnections = new List<TileData>(neighborNode.possConnections); 

        foreach (TileData possibleConnection in copyNeighborConnections) {
            if (!sourceNodeRestrictions.Contains(possibleConnection)) {
                neighborNode.possConnections.Remove(possibleConnection);
            }
        }

        neighborNode.entropy = neighborNode.calcEntropy();
    }

    public Node findLowestEntropyNode(Node[,] grid) {
        float lowestEntropy = float.MaxValue;

        // if no uncollapsed node is found, then generation is complete
        Node lowestEntropyNode = new Node();
        lowestEntropyNode.isCollapsed = true;
       

        Node tempNode;
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                tempNode = grid[x, y];
                if (tempNode.isCollapsed) continue;

                if (tempNode.entropy < lowestEntropy) {
                    lowestEntropy = tempNode.entropy;
                    lowestEntropyNode = tempNode;
                }

                //can not find a lower entropy than 0
                if(lowestEntropyNode.entropy == 0) return lowestEntropyNode;
            }
        }
        return lowestEntropyNode;
    }

    // Debuging 
    public void printEntropy(MyGrid grid) {
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                Debug.Log($"{x},{y}: entropy = {grid.nodeGrid[x, y].entropy}");
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

    
