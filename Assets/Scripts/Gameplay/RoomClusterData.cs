using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomClusterData {
    // Properties
    public bool IsUnlocked { get; private set; }
    public int NumSnacksReq { get; private set; } // how many global snacks we need to unlock me!
    public Rect BoundsGlobal { get; private set; }
    public RoomAddress MyAddress { get; private set; }
    public SnackCount SnackCount = new SnackCount();
    public PlayerTypes TrialPlayerType { get; private set; }
    // References
    public List<RoomData> rooms=new List<RoomData>();
    
    // Getters
    public int ClustIndex { get { return MyAddress.clust; } }
    public int WorldIndex { get { return MyAddress.world; } }
    public bool IsCharTrial { get { return TrialPlayerType != PlayerTypes.Undefined; } }
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public RoomClusterData(int WorldIndex, string startRoomKey) {
        this.MyAddress = new RoomAddress(WorldIndex, -1);
        // TEMP HARDCODED-ISH set TrialPlayerType!
        if (WorldIndex == GameProperties.TEMP_TrialsWorldIndex) {
            string rk = startRoomKey;
            int strInd = rk.IndexOf("TrialStart", System.StringComparison.Ordinal);
            if (strInd >= 0) {
                string typeStr = rk.Substring(0, strInd);
                TrialPlayerType = PlayerTypeHelper.TypeFromString(typeStr);
            }
            else {
                Debug.LogWarning(startRoomKey + " doesn't fit char-trial-start-cluster naming scheme.");
            }
        }
    }
    public void SetClustIndex(int clustIndex) {
        this.MyAddress = new RoomAddress(WorldIndex,clustIndex);
        this.IsUnlocked = SaveStorage.GetBool(SaveKeys.ClustIsUnlocked(MyAddress), false);
        if (GameProperties.IsFirstCluster(MyAddress)) { this.IsUnlocked = true; } // First cluster is ALWAYS unlocked.
        
        // Set NumSnacksReq
        NumSnacksReq = GameProperties.ClustNumSnacksReq(MyAddress);
        
        // Update Rooms, yo
        // TODO: This?
    }
    
    
    // ----------------------------------------------------------------
    //  Doers (Setters)
    // ----------------------------------------------------------------
    public void SetIsUnlocked(bool val) {
        IsUnlocked = val;
        SaveStorage.SetBool(SaveKeys.ClustIsUnlocked(MyAddress), IsUnlocked);
    }
    
    
    // ----------------------------------------------------------------
    //  Doers (Refreshers)
    // ----------------------------------------------------------------
    public void RefreshBounds() {
        BoundsGlobal = Rect.zero;
        foreach (RoomData rd in rooms) {
            Rect roomBounds = new Rect(rd.BoundsGlobal.position-rd.BoundsGlobal.size*0.5f, rd.BoundsGlobal.size); // AWKWARD offset for centered-ness.
            BoundsGlobal = MathUtils.GetCompoundRect(BoundsGlobal, roomBounds);
        }
    }
    public void RefreshSnackCount() {
        // Clear my own count.
        SnackCount.ZeroCounts();
        // Refresh counts for all my rooms, and add 'em to MY count!
        for (int r=0; r<rooms.Count; r++) {
            rooms[r].RefreshSnackCount();
            SnackCount.Add(rooms[r].SnackCount);
        }
    }
    
    
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