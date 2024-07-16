using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    //In a collapsed node collections only contains the chosen tile
    public List<TileData> connections { get; set; } 
    public int entropy { get; set; }

    public Vector2Int coord { get;}

    public bool isCollapsed { get; set; } = false;

    public Node(List<TileData> connections, Vector2Int coord, int entropy, bool isCollapsed) {
        this.connections = new List<TileData>(connections);
        this.coord = coord;
        this.entropy = entropy;
        this.isCollapsed = isCollapsed;
    }

    //TODO connections.Count should be calcEntropy Instead?
    public Node(List<TileData> connections, Vector2Int coord): 
        this(connections,coord, connections.Count,false) {
    }

    public Node(): 
        this(new List<TileData>(), new Vector2Int(-1, -1)) {
    }

    //Creating a deep copy of provided node
    public Node(Node node):
        //Tile data elements can be left as a shallow copy, since it is not instance specific
        //coord is a struct so is automatically made as a deep copy
        this(node.connections, node.coord, node.entropy,node.isCollapsed) {
    }

    int calcEntropy() {
       // Debug.Log($"calcEntropy: {connections.Count}, coord{coord}");
        return connections.Count;
    }
}
