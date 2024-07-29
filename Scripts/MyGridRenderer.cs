using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MyGridRenderer : MonoBehaviour
{
    [SerializeField] public static Tilemap tilemap; //Should this be in WFC? (prob not?)
    [Tooltip("This is automatically being loaded from Assets/TileData")]
    private void OnValidate() {
        tilemap = GetComponentInChildren<Tilemap>();
    }

    public static void redrawTileMap(Node[,] nodeGrid) {
       // Debug.Log($"RedrawingTileMap {tilemap.name}");
        tilemap.ClearAllTiles();
        Node node;
        for (int x = 0; x < MyGrid.WIDTH; x++) {
            for (int y = 0; y < MyGrid.HEIGHT; y++) {
                node = nodeGrid[x, y];
                if (!node.isCollapsed) continue;
                tilemap.SetTile(new Vector3Int(node.coord.x, node.coord.y), node.possConnections[0].tile);
            }
        }
    }
}
