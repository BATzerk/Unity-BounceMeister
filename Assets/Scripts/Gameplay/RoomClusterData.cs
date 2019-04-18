using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomClusterData {
    // Properties
    public int ClusterIndex { get; private set; }
    public int WorldIndex { get; private set; }
    //public int NumGems { get; private set; }
    //public int NumGemsCollected { get; private set; }
    public int NumSnacks { get; private set; }
    public int NumSnacksEaten { get; private set; }
    // References
    public List<RoomData> rooms=new List<RoomData>();
    
    // Initialize
    public RoomClusterData(int WorldIndex, int ClusterIndex) {
        this.WorldIndex = WorldIndex;
        this.ClusterIndex = ClusterIndex;
    }
    
    // Doers
    public void UpdateEdiblesCounts() {
        NumSnacks = 0;
        NumSnacksEaten = 0;
        for (int r=0; r<rooms.Count; r++) {
            rooms[r].UpdateNumSnacks();
            NumSnacks += rooms[r].NumSnacksTotal;
            NumSnacksEaten += rooms[r].NumSnacksEaten;
            
            //for (int p=0; p<rooms[r].allPropDatas.Count; p++) {
            //    if (rooms[r].allPropDatas[p] is GemData) {
            //        //if ((rooms[r].allPropDatas[p] as GemData).isEaten) {
            //        if (SaveStorage.GetBool(SaveKeys.DidEatGem(rooms[r], gemIndex))) {
            //            NumGemsCollected ++;
            //        }
            //        gemIndex ++;
            //        NumGems ++;
            //    }
            //    else if (rooms[r].allPropDatas[p] is SnackData) {
            //        //if ((rooms[r].allPropDatas[p] as SnackData).isEaten) {
            //        if (SaveStorage.GetBool(SaveKeys.DidEatSnack(rooms[r], snackIndex))) {
            //            NumSnacksEaten ++;
            //        }
            //        snackIndex ++;
            //        NumSnacks ++;
            //    }
            //}
        }
        //Debug.Log("W"+WorldIndex+" Cluster: " + ClusterIndex + "    gems: " + NumGemsCollected+"/"+NumGems + ", snacks: " + NumSnacksCollected+"/"+NumSnacks);
    }
    
    
}
