using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomClusterData {
    // Properties
    public int ClusterIndex { get; private set; }
    public int WorldIndex { get; private set; }
    public SnackCount SnackCount = new SnackCount();
    // References
    public List<RoomData> rooms=new List<RoomData>();
    
    // Initialize
    public RoomClusterData(int WorldIndex, int ClusterIndex) {
        this.WorldIndex = WorldIndex;
        this.ClusterIndex = ClusterIndex;
    }
    
    // Doers
    public void RefreshSnackCount() {
        // Clear my own count.
        SnackCount.ZeroCounts();
        // Refresh counts for all my rooms, and add 'em to MY count!
        for (int r=0; r<rooms.Count; r++) {
            rooms[r].RefreshSnackCount();
            SnackCount.Add(rooms[r].SnackCount);
        }
        
        
        //NumSnacks = 0;
        //NumSnacksEaten = 0;
        //for (int r=0; r<rooms.Count; r++) {
        //    rooms[r].UpdateNumSnacks();
        //    NumSnacks += rooms[r].NumSnacksTotal;
        //    NumSnacksEaten += rooms[r].NumSnacksEaten;
            
        //    //for (int p=0; p<rooms[r].allPropDatas.Count; p++) {
        //    //    if (rooms[r].allPropDatas[p] is GemData) {
        //    //        //if ((rooms[r].allPropDatas[p] as GemData).isEaten) {
        //    //        if (SaveStorage.GetBool(SaveKeys.DidEatGem(rooms[r], gemIndex))) {
        //    //            NumGemsCollected ++;
        //    //        }
        //    //        gemIndex ++;
        //    //        NumGems ++;
        //    //    }
        //    //    else if (rooms[r].allPropDatas[p] is SnackData) {
        //    //        //if ((rooms[r].allPropDatas[p] as SnackData).isEaten) {
        //    //        if (SaveStorage.GetBool(SaveKeys.DidEatSnack(rooms[r], snackIndex))) {
        //    //            NumSnacksEaten ++;
        //    //        }
        //    //        snackIndex ++;
        //    //        NumSnacks ++;
        //    //    }
        //    //}
        //}
        //Debug.Log("W"+WorldIndex+" Cluster: " + ClusterIndex + "    gems: " + NumGemsCollected+"/"+NumGems + ", snacks: " + NumSnacksCollected+"/"+NumSnacks);
    }
    
    
}
