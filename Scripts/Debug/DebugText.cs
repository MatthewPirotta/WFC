using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI totItrTxt;
    [SerializeField] TextMeshProUGUI itrsRelBackupTxt;
    [SerializeField] TextMeshProUGUI totBacktrackCntTxt;
    [SerializeField] TextMeshProUGUI RelBacktrackCntTxt;
    [SerializeField] TextMeshProUGUI BackupCntTxt;
    [SerializeField] TextMeshProUGUI seedTxt;
    [SerializeField] TextMeshProUGUI layerTxt;

    public DebugText() {
        WFC.initWorld += setConstantInfo;
        WFC.updataGameState += onUpdateCounters;
    }

    /// <summary>
    /// Update the counter info, that changes through each iteration
    /// </summary>
    /// <param name="cnts"></param>
    void onUpdateCounters(Counters cnts, Layer layer) {
        totItrTxt.text = $"Itr Total: {cnts.totItrCnt}";
        itrsRelBackupTxt.text = $"Itr Rel Backup: {cnts.relItrCnt}";
        totBacktrackCntTxt.text = $"Backtrack Cnt Total: {cnts.totBacktrackCnt}";
        RelBacktrackCntTxt.text = $"Backtrack Cnt Rel: {cnts.relBacktrackCnt}";
        BackupCntTxt.text = $"Backup Cnt Total: {cnts.totBackupCnt}";
        layerTxt.text = $"Layer: {layer}";
    }

    //Initial one time constant info update
    void setConstantInfo() {
        resetText();

        seedTxt.text = $"Seed: {GameConstants.SEED}";

        void resetText() {
            totItrTxt.text = "Itr Total: 0";
            itrsRelBackupTxt.text = "Itr Rel Backup: 0";
            totBacktrackCntTxt.text = "Backtrack Cnt Total: 0";
            RelBacktrackCntTxt.text = "Backtrack Cnt Rel: 0";
            BackupCntTxt.text = "Backup Cnt Total: 0";
            seedTxt.text = "Seed: NA";
            layerTxt.text = "Layer: NA";
        }
    }
}
