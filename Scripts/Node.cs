using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    //In a collapsed node collections only contains the chosen tile
    public List<TileData> connections { get; set; } 
    public int entropy { get; set; }

    public Vector2Int coord { get; set; }

    public bool isCollapsed { get; set; } = false;

    public Node(List<TileData> connections, Vector2Int coord) {
        this.connections = connections;
        this.coord = coord;
        this.entropy = calcEntopy();
        this.isCollapsed = false;
    }

    public Node() : this(new List<TileData>(), new Vector2Int(-1, -1)) {
       
    }

    int calcEntopy() {
       // Debug.Log($"calcEntropy: {connections.Count}, coord{coord}");
        return connections.Count;
    }
}
