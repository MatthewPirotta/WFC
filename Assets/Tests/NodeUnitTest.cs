using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

//NOTE CHATGPT Generated
[TestFixture]
public class NodeUnitTest {
    // Utility method to create TileData instances for testing
    private TileData CreateTileData(int weight, EdgeType edgeUType, EdgeType edgeDType) {
        TileData tileData = ScriptableObject.CreateInstance<TileData>();
        tileData.weight = weight;
        tileData.edgeU = new List<EdgeType> { edgeUType };
        tileData.edgeD = new List<EdgeType> { edgeDType };
        return tileData;
    }

    // Test to verify the node's entropy calculation logic
    [Test]
    public void Test_CalcEntropy_WithMultipleConnections() {
        // Arrange
        List<TileData> tiles = new List<TileData>
        {
            CreateTileData(2, EdgeType.Water, EdgeType.Sand),
            CreateTileData(3, EdgeType.Sand, EdgeType.Empty),
            CreateTileData(5, EdgeType.Empty, EdgeType.Plank)
        };
        Node node = new Node(tiles, new Vector2Int(0, 0), false);

        // Act
        float entropy = node.entropy;

        // Assert
        Assert.Greater(entropy, 0); // Ensure entropy is greater than 0 for multiple connections
    }

    // Test to verify that a collapsed node's entropy is set to 0
    [Test]
    public void Test_Entropy_CollapsedNode() {
        // Arrange
        List<TileData> tiles = new List<TileData>
        {
            CreateTileData(1, EdgeType.Water, EdgeType.Sand),
            CreateTileData(1, EdgeType.Sand, EdgeType.Water)
        };
        Node node = new Node(tiles, new Vector2Int(1, 1), true);

        // Act
        float entropy = node.entropy;

        // Assert
        Assert.AreEqual(0, entropy); // A collapsed node should have 0 entropy
    }

    // Test the node's deep copy functionality
    [Test]
    public void Test_DeepCopy_Node() {
        // Arrange
        List<TileData> tiles = new List<TileData>
        {
            CreateTileData(1, EdgeType.Sand, EdgeType.Water),
            CreateTileData(2, EdgeType.Empty, EdgeType.Plank)
        };
        Node originalNode = new Node(tiles, new Vector2Int(2, 2), false);

        // Act
        Node copiedNode = new Node(originalNode);

        // Assert
        Assert.AreEqual(originalNode.coord, copiedNode.coord); // Coordinates should be the same
        Assert.AreEqual(originalNode.possConnections.Count, copiedNode.possConnections.Count); // Number of possible connections should be the same
        Assert.AreEqual(originalNode.isCollapsed, copiedNode.isCollapsed); // Collapsed state should be the same
    }

    // Test node initialization with default constructor
    [Test]
    public void Test_Node_DefaultConstructor() {
        // Act
        Node node = new Node();

        // Assert
        Assert.AreEqual(new Vector2Int(-1, -1), node.coord); // Default coordinates
        Assert.AreEqual(0, node.possConnections.Count); // Empty list of possible connections
        Assert.IsTrue(node.isCollapsed); // Default state should be collapsed
        Assert.AreEqual(0, node.entropy); // Entropy should be 0 for a collapsed node
    }

    // Test changing possible connections and ensure entropy is updated
    [Test]
    public void Test_PossConnections_Update() {
        // Arrange
        List<TileData> tiles = new List<TileData> { CreateTileData(1, EdgeType.Water, EdgeType.Sand) };
        Node node = new Node(tiles, new Vector2Int(3, 3), false);

        // Act
        List<TileData> newTiles = new List<TileData>
        {
            CreateTileData(4, EdgeType.Sand, EdgeType.Plank),
            CreateTileData(6, EdgeType.Water, EdgeType.Empty)
        };
        node.possConnections = newTiles; // This should trigger entropy recalculation
        float entropy = node.entropy;

        // Assert
        Assert.AreEqual(2, node.possConnections.Count); // Ensure the new connections are set
        Assert.Greater(entropy, 0); // Ensure entropy is updated and greater than 0
    }

    // Test that changing isCollapsed forces entropy recalculation
    [Test]
    public void Test_IsCollapsed_EntropyUpdate() {
        // Arrange
        List<TileData> tiles = new List<TileData> { CreateTileData(1, EdgeType.Water, EdgeType.Sand) };
        Node node = new Node(tiles, new Vector2Int(4, 4), false);

        // Act
        node.isCollapsed = true; // Collapse the node
        float entropy = node.entropy; // Entropy should be recalculated

        // Assert
        Assert.AreEqual(0, entropy); // A collapsed node should have 0 entropy
    }
}
