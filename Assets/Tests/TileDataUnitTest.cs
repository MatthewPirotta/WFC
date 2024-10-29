using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;


public class TileDataUnitTests {
    private TileData tileData;

    [SetUp]
    public void SetUp() {
        tileData = ScriptableObject.CreateInstance<TileData>();
        tileData.tile = ScriptableObject.CreateInstance<Tile>(); // Create a mock tile instance
        tileData.tile.name = "TestTile";

        // Initialize edges and accepted connections
        tileData.edgeU = new List<EdgeType> { EdgeType.Empty, EdgeType.Water };
        tileData.edgeD = new List<EdgeType> { EdgeType.Sand };
        tileData.edgeL = new List<EdgeType> { EdgeType.Plank };
        tileData.edgeR = new List<EdgeType> { EdgeType.Empty };

        tileData.accU = new List<TileData>();
        tileData.accD = new List<TileData>();
        tileData.accL = new List<TileData>();
        tileData.accR = new List<TileData>();
    }

    [Test]
    public void TestGetEdge_ValidDirection_ReturnsEdges() {
        var edgesU = tileData.getEdge('U');
        Assert.AreEqual(2, edgesU.Count);
        Assert.AreEqual(EdgeType.Empty, edgesU[0]);
        Assert.AreEqual(EdgeType.Water, edgesU[1]);
    }

    [Test]
    public void TestGetEdge_InvalidDirection_ReturnsNull() {
        Assert.Throws<ArgumentException>(() => tileData.getEdge('X'));
    }

    [Test]
    public void TestSetAccDir_ValidDirection_SetsConnections() {
        List<TileData> newAcc = new List<TileData> { ScriptableObject.CreateInstance<TileData>() };
        tileData.setAccDir(newAcc, 'U');
        var accU = tileData.getAccDir('U');
        Assert.AreEqual(newAcc, accU);
    }

    [Test]
    public void TestGetAccDir_ValidDirection_ReturnsConnections() {
        var accD = tileData.getAccDir('D');
        Assert.IsEmpty(accD); // Initially, this should be empty
    }

    [Test]
    public void TestGetOppDir_ValidDirection_ReturnsOppositeDirection() {
        Assert.AreEqual('D', TileData.getOppDir('U'));
        Assert.AreEqual('U', TileData.getOppDir('D'));
        Assert.AreEqual('R', TileData.getOppDir('L'));
        Assert.AreEqual('L', TileData.getOppDir('R'));
    }

    [Test]
    public void TestGetOffset_ValidDirection_ReturnsOffset() {
        Assert.AreEqual(Vector2Int.up, TileData.getOffset('U'));
        Assert.AreEqual(Vector2Int.down, TileData.getOffset('D'));
        Assert.AreEqual(Vector2Int.left, TileData.getOffset('L'));
        Assert.AreEqual(Vector2Int.right, TileData.getOffset('R'));
    }

    [Test]
    public void TestEquals_SameTile_ReturnsTrue() {
        TileData otherTileData = ScriptableObject.CreateInstance<TileData>();
        otherTileData.tile = tileData.tile; // Same tile
        Assert.IsTrue(tileData.Equals(otherTileData));
    }

    [Test]
    public void TestEquals_DifferentTile_ReturnsFalse() {
        TileData otherTileData = ScriptableObject.CreateInstance<TileData>();
        otherTileData.tile = ScriptableObject.CreateInstance<Tile>();
        Assert.IsFalse(tileData.Equals(otherTileData));
    }

    [Test]
    public void TestGetHashCode_SameTile_ReturnsSameHash() {
        TileData otherTileData = ScriptableObject.CreateInstance<TileData>();
        otherTileData.tile = tileData.tile; // Same tile
        Assert.AreEqual(tileData.GetHashCode(), otherTileData.GetHashCode());
    }

    [Test]
    public void TestToString_ReturnsTileName() {
        Assert.AreEqual("TestTile", tileData.ToString());
    }
}
