using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This will allows other grid type implementations. 
/// For example a dictionary, enabling non square worlds
/// </summary>
public interface IGrid
{
    Node getNode(Vector2Int coord);
    List<Node> getNeighbors(Node node);
    IEnumerable<Node> getAllNodes();
    Node getLowestEntropyNode();
    bool isInGrid(Vector2Int coord);
    void backupGrid();
    void backtrack();
    void Clear();

    int Area { get; }

    public Layer layer { get;}

    bool Equals(IGrid other); // Add this method to the interface
}
