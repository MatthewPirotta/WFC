using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//NOTE everyhting is static (cringe?)
public class MyGridRenderer : MonoBehaviour
{
    private static Tilemap[] tilemaps;
    // 0 - topTilemap
    // 1 - midTilemap
    // 2 - botTilemap


    [Tooltip("This is automatically being loaded from Assets/TileData")]
    private void OnValidate() {
        //NOTE imp that objects are in above order
        tilemaps = GetComponentsInChildren<Tilemap>();
    }

    public static void redrawTileMap(Node[,] nodeGrid, Layer layer){
        // Debug.Log($"RedrawingTileMap {tilemap.name}");
        Tilemap tilemap = tilemaps[(int)layer];
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

    static public void clearTilemap(int layer) {
        tilemaps[layer].ClearAllTiles();
    }

    static public Tilemap getTilemap(int layer) {
        return tilemaps[layer];
    }

    //TODO prob better way
    public void toggledGridRenderer(bool isActivated) {
        TilemapRenderer[] tilemapRenderers = GetComponentsInChildren<TilemapRenderer>();
        foreach(TilemapRenderer tilemapRenderer in tilemapRenderers) {
            tilemapRenderer.enabled = isActivated;
        }
    }
}
