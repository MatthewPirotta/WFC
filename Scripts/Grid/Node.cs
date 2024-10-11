using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Node {
    //In a collapsed node collections only contains the chosen tile
    public List<TileData> possConnections { get; set; } 
    public float entropy { get; set; }

    public Vector2Int coord { get;}

    public bool isCollapsed { get; set; } = true;

    public Node(List<TileData> possConnections, Vector2Int coord, float entropy, bool isCollapsed) {
        this.possConnections = new List<TileData>(possConnections);
        this.coord = coord;
        this.entropy = entropy;
        this.isCollapsed = isCollapsed;
    }

    public Node(List<TileData> connections, Vector2Int coord): 
        this(connections,coord, 0, true) {
        //properly assigning entropy, due to static nonsense
        this.entropy= calcEntropy();
    }

    public Node(): 
        this(new List<TileData>(), new Vector2Int(-1, -1),0,true) {
        this.entropy = calcEntropy();
    }

    //Creating a deep copy of provided node
    public Node(Node node):
        //Tile data elements can be left as a shallow copy, since it is not instance specific
        //coord is a struct so is automatically made as a deep copy
        this(node.possConnections, node.coord, node.entropy,node.isCollapsed) {
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
            p = (float) possTile.weight / sumWeights; //typecast ensure floating-point division
            entropy -= p * Mathf.Log(p);
        }

        return entropy;
    }
}
