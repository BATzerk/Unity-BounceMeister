using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomClusterData {
    // Properties
    public int ClusterIndex { get; private set; }
    public int NumGems { get; private set; }
    public int NumGemsCollected { get; private set; }
    public int NumSnacks { get; private set; }
    public int NumSnacksCollected { get; private set; }
    // References
    public List<RoomData> rooms=new List<RoomData>();
    
    // Initialize
    public RoomClusterData(int ClusterIndex) {
        this.ClusterIndex = ClusterIndex;
    }
    
    // Doers
    public void UpdateCollectablesCounts() {
        NumGems = 0;
        NumGemsCollected = 0;
        NumSnacks = 0;
        NumSnacksCollected = 0;
        for (int l=0; l<rooms.Count; l++) {
            for (int p=0; p<rooms[l].allPropDatas.Count; p++) {
                //NumGems += rooms[l].gemDatas.Count;
                if (rooms[l].allPropDatas[p] is GemData) {
                    NumGems ++;
                }
                else if (rooms[l].allPropDatas[p] is SnackData) {
                    NumSnacks ++;
                }
                // TODO: Gems/Snacks collected!
            }
        }
    }
    
    
}
