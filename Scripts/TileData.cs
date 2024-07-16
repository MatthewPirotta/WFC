using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[CreateAssetMenu(fileName = "WFCTile", menuName = "ScriptableObjects/TileData")]
[SerializeField]
public class TileData : ScriptableObject
{
    public Tile tile;
   
    //Order Matters
    //First edge type is what is on 'top'/'right'
    //Second edge type is what is on 'bottom'/'left'
    //if two edge types are parrel then sort acording to other cardinal direction
    public List<EdgeType> edgeU;
    public List<EdgeType> edgeD;
    public List<EdgeType> edgeL;
    public List<EdgeType> edgeR;

    //Accepted connections for each direction
    public List<TileData> accU; 
    public List<TileData> accD;
    public List<TileData> accL;
    public List<TileData> accR;


    public List<EdgeType> getEdge(char dir) {
        switch (dir) {
            case 'U': return edgeU;
            case 'D': return edgeD;
            case 'L': return edgeL;
            case 'R': return edgeR;
            default: return null; //should not reach this but ifykyk
        }
    }

    public void setAccDir(List<TileData> AccDir, char dir) {
        switch (dir) {
            case 'U': accU = AccDir; break;
            case 'D': accD = AccDir; break;
            case 'L': accL = AccDir; break;
            case 'R': accR = AccDir; break;
        }
    }

    public static char getOppDir(char dir) {
        switch (dir) {
            case 'U': return 'D';
            case 'D': return 'U';
            case 'L': return 'R';
            case 'R': return 'L';
            default: return '-'; //should not reach this but ifykyk
        }
    }
}

public enum EdgeType{
    Water,
    Sand
}