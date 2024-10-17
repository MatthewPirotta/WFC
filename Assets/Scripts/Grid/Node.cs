using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Node {
    //In a collapsed node collections only contains the chosen tile
    private List<TileData> _possConnections;
    public List<TileData> possConnections {
        get { return _possConnections; }
        set {
            _possConnections = value;
            isEntropyUpdateNeeded = true;
        }
    }

    // Flag to track when recalculating Entropy is needed
    private bool isEntropyUpdateNeeded = true;
    private float _entropy;
    public float entropy {
        get {
            if (isEntropyUpdateNeeded) {
                _entropy = calcEntropy();
                isEntropyUpdateNeeded = false;
            }
            return _entropy;
        }
    }

    public Vector2Int coord { get;}

    private bool _isCollapsed = true;
    public bool isCollapsed {
        get { return _isCollapsed; }
        set {
            _isCollapsed = value;
            //When a node is collapsed, its entropy needs to be updated to 0
            //And if a node is being enabled, the entropy needs to be recalulcated
            isEntropyUpdateNeeded = true;
        }
    }

    public Node(List<TileData> possConnections, Vector2Int coord, bool isCollapsed) {
        this.possConnections = new List<TileData>(possConnections);
        this.coord = coord;
        this.isCollapsed = isCollapsed;
    }

    public Node(List<TileData> connections, Vector2Int coord): 
        this(connections,coord, true) {
    }

    public Node(): 
        this(new List<TileData>(), new Vector2Int(-1, -1),true) {
    }

    //Creating a deep copy of provided node
    public Node(Node node):
        //Tile data elements can be left as a shallow copy, since it is not instance specific
        //coord is a struct so is automatically makes as a deep copy
        this(node.possConnections, node.coord,node.isCollapsed) {
    }

    public float calcEntropy() {
        if (isCollapsed) return 0;
        if (possConnections.Count <= 1) return 0;

        int sumWeights = 0;
        foreach(TileData possTile in possConnections) {
            sumWeights += possTile.weight;
        }

        float p;
        float entropy = 0;
        foreach (TileData possTile in possConnections) {
            //If a tile weight is set to 0, to stop it from appearing
            //this can cause undefined errors with log(0)
            if (possTile.weight == 0) continue;

            p = (float) possTile.weight / sumWeights; //typecast ensure floating-point division
            entropy -= p * Mathf.Log(p);
        }

        return entropy;
    }
}
