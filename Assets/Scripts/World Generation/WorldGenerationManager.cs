using UnityEngine;

//TODO rename to WorldGenManager
//TODO requires My Grid renderer
[ExecuteInEditMode]
public class WorldGenerationManager : MonoBehaviour {
    WFC wfs = new WFC();
    public MyGrid[] myGrids { get; private set; } = new MyGrid[3];

    private int layercnt = 0;
    public int LayerCnt {
        get { return layercnt; }
        set {
            if (value < 0 && value > myGrids.Length) {
                Debug.LogWarning($"Attempted to set layerCnt to {value}, but it exceeds the grid length. Value should be between 0 and {myGrids.Length - 1}.");
                return;
            }
           layercnt = value;
        }
    }

    private void Start() {
        initWorldGen();
        genAll();
    }

    public void initWorldGen() {
        LayerCnt = 0;
        myGrids[(int)Layer.MIDDLE] = new MyGrid(TileSetManager.LoadTileSetData("Assets/TileData/MidTileData"), Layer.MIDDLE);
        myGrids[(int)Layer.BOTTOM] = new MyGrid(TileSetManager.LoadTileSetData("Assets/TileData/BotTileData"), Layer.BOTTOM);
        myGrids[(int)Layer.TOP] = new MyGrid(TileSetManager.LoadTileSetData("Assets/TileData/TopTileData"), Layer.TOP);
        wfs.initWorldSpace(myGrids);
    }

    public void collapseNextNode() {
        if (handleGenerationComplete()) return;

        bool isdoneGenerating = wfs.performSingleIteration(myGrids, myGrids[LayerCnt]);
        if (isdoneGenerating) {
            LayerCnt++;
            wfs.performSingleIteration(myGrids, myGrids[LayerCnt]);
        }
    }

    public void genNextLayer() {
        if (handleGenerationComplete()) return;

        wfs.WaveFunctionCollapse(myGrids, (Layer)LayerCnt);
        LayerCnt++;
    }

    public void genAll() {
        if (handleGenerationComplete()) return;

        for (; LayerCnt < myGrids.Length; LayerCnt++) {
            wfs.WaveFunctionCollapse(myGrids, (Layer)LayerCnt);
        }
    }

    bool handleGenerationComplete() {
        if (LayerCnt >= myGrids.Length) {
            Debug.LogWarning("No more world to generate");
            return true;
        }
        return false;
    }
}
