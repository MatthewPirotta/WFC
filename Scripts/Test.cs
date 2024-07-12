using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tile grassTile;
    [SerializeField] Tile waterTile;

    // Start is called before the first frame update
    void Start()
    {
        tilemap.SetTile(new Vector3Int(0,0), grassTile);
        tilemap.SetTile(new Vector3Int(1,0,0), waterTile);
        tilemap.SetTile(new Vector3Int(2,0,0), waterTile);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
