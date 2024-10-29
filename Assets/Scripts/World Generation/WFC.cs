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

// make static/singleton? overkill imo
/// <summary>
/// The Wave Function Collapse algorithm implementation.
/// </summary>
public class WFC{
    List<Node> collapsedNodes = new List<Node>(); // Nodes that have been collapsed since last bakcup

    int maxBacktrackFailures { get; } = 10;

    Counters cnts = new Counters();

    public bool collapseAll = false;

    //TODO Remove
    int sameTileBias = 1;

    System.Random random;

    public static event Action initWorld; //One time call to initilise scripts
    public static event Action<IGrid, Node> collapsedNode; //Node was collapsed
    public static event Action<IGrid, Node> propogateNodeData; //Propogating the new constraints from the collapsed node
    public static event Action<IGrid> updateGrid; // Updating the whole grid, this is used on backtracks
    public static event Action<Counters, Layer> updataGameState; //Providing other classes with low level data

    /*
    public static event Action<> generatingNextLayer;
    public static event Action<> backupGrid;
    public static event Action<> backtrackGrid;
    */
    
    public void initWorldSpace(IGrid[] myGrids) {
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
    void resetGeneration(IGrid[] grids) {
        foreach(IGrid grid in grids) {
            MyGridRenderer.clearTilemap((int)grid.layer);
            grid.Clear();

            //The middle grid is the first to be generated
            //All other grids are dependent on it
            if (grid.layer == Layer.MIDDLE) {
                foreach (Node node in grid.getAllNodes()) {
                    node.isCollapsed = false;
                }
            }

            //NOTE REMOVED the code for RESETING node entropy value, cos clearing the grid creates a new Node
        }

        collapsedNodes.Clear();

        cnts.relItrCnt = 0;
        cnts.relBacktrackCnt = 0;
    }

    public void WaveFunctionCollapse(IGrid[] grids, Layer layer) {
        //All grids should have the same area, so
        //The first grid is arbitrarley chosen
        long maxItr = grids[0].Area * 10; // stop after what would be generating the world 10 times (ignoring all generation failures)

        IGrid chosenGrid = grids[(int)layer];
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

        //Backing up grids when a layer is complete
        //Prevents a new layer from ondoing work from other layers
        backup(grids);
    }

    /// <summary>
    /// Performs a single Iteration of WFC algorithm
    /// </summary>
    /// <param name="grids"></param>
    /// <param name="chosenGrid"></param>
    /// <returns>Returns true if all nodes have been collapsed on current grid layer. Otherwise returns false.</returns>
    /// <remarks>How the code handles reseting the world, it only works if there is a backup between layers. 
    /// SinceGenerationManager is iterating over the layers and can not go back. </remarks>
    public bool performSingleIteration(IGrid[] grids, IGrid chosenGrid) {
        //All grids should have the same area, so
        //The first grid is arbitrarley chosen
        int backupInterval = grids[0].Area / 10; //makes backup in 1/10th intervals

        //counters are done before trying to collapse,
        //to prevent infinite loops, where there a non solveable checkpoint is created
        cnts.relItrCnt++;
        cnts.totItrCnt++;

        Node nodeToCollapse = chosenGrid.getLowestEntropyNode();
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
    }


    //select random tile to collape Node
    void collapseNode(IGrid[] grids, IGrid chosenGrid, Node node) {
        TileData chosenTile = selectWeightedRandomTile(node);

        //Debug.Log($"node count: {node.connections.Count}");
        //Debug.Log($"randNum: {Random.Range(0, node.connections.Count)}");
        node.possConnections.Clear();
        node.possConnections.Add(chosenTile);
        node.isCollapsed = true;

        Tilemap tilemap = MyGridRenderer.getTilemap((int)chosenGrid.layer);
        tilemap.SetTile(new Vector3Int(node.coord.x, node.coord.y), chosenTile.tile);
        collapsedNodes.Add(node);

        EnableNodesOtherLayers(grids, chosenGrid, node, chosenTile);

        collapsedNode?.Invoke(chosenGrid, node);
        updataGameState?.Invoke(cnts, chosenGrid.layer);

        Propogate(chosenGrid, node);
        // Debug.Log("calling update Debug Display");
        //Debug.Log($"Before UpdateDebugDisplay. {node.coord} entropy: {node.entropy}");
    }

    /// <summary>
    /// This is currently hardcoded for the current 3 layer grid system.
    /// Where only the middle grid affects other layers
    /// </summary>
    /// <param name="grids"></param>
    /// <param name="grid"></param>
    /// <param name="node"></param>
    /// <param name="chosenTile"></param>
    private void EnableNodesOtherLayers(IGrid[] grids, IGrid grid, Node node, TileData chosenTile) {
        if (grid.layer != Layer.MIDDLE) return;

        if (chosenTile.enableLayerBelow) {
            enableNodeInLayer(Layer.BOTTOM);
        }
        if (chosenTile.enableLayerAbove) {
            enableNodeInLayer(Layer.TOP);
        }

        void enableNodeInLayer(Layer targetLayer) {
            IGrid otherGrid = grids[(int)targetLayer];
            Node newNode = otherGrid.getNode(node.coord);

            if (newNode == null) return;
            if (!newNode.isCollapsed) return; // The node shouldn't already by collapsed, but future proofing

            newNode.isCollapsed = false;
            propogateNodeData?.Invoke(otherGrid, newNode);
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
    void Propogate(IGrid grid, Node node) {
        Vector2Int newCoord;
        Node neighborNode;

        foreach (char dir in TileData.directions) {
            newCoord = node.coord + TileData.getOffset(dir);

            if (!grid.isInGrid(newCoord)) continue;
            neighborNode = grid.getNode(newCoord);
            if (neighborNode.isCollapsed) continue;

            //Debug.Log($"Node: {node.coord} propogating to neighbor {neighborNode.coord}");

            whittleConnections(neighborNode, node.possConnections[0].getAccDir(dir));
            propogateNodeData?.Invoke(grid, neighborNode);
        }
    }

    void whittleConnections(Node neighborNode, List<TileData> sourceNodeRestrictions) {
        List<TileData> filteredNeighborConnections = new List<TileData>(neighborNode.possConnections); 

        foreach (TileData possibleConnection in neighborNode.possConnections) {
            if (!sourceNodeRestrictions.Contains(possibleConnection)) {
                filteredNeighborConnections.Remove(possibleConnection);
            }
        }

        neighborNode.possConnections = filteredNeighborConnections;
    }

    //TODO dont need chosenGrid and can prob rework DebugGrid::updateDebugGrid interacts and runs
    ///Since layers can influence others, all grid states must be updated
    void backtrack(IGrid[] workingGrids, IGrid chosenGrid) {
        //Debug.Log($"Node {nodeToCollapse.coord} failed to collapse");
        cnts.totBacktrackCnt++;
        cnts.relBacktrackCnt++;

        for (int i = 0; i < workingGrids.Length; i++) {
            workingGrids[i].backtrack();
        }

        updateGrid?.Invoke(chosenGrid);
        updataGameState?.Invoke(cnts, chosenGrid.layer);
    }

    void backup(IGrid[] workingGrids) {
        cnts.relItrCnt = 0;
        cnts.relBacktrackCnt = 0;
        cnts.totBackupCnt++;

        for (int i = 0; i < workingGrids.Length; i++) {
            workingGrids[i].backupGrid();
        }
    }

    // Debuging 
    public void printEntropy(IGrid grid) {
        foreach(Node node in grid.getAllNodes()) { 
            Debug.Log($"{node.coord.x},{node.coord.y}: entropy = {node.entropy}");
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

    
