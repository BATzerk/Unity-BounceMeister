using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnackCount {
    // Properties
    public Dictionary<PlayerTypes, int> eaten;
    public Dictionary<PlayerTypes, int> total;
    
    
    // Getters (Public)
    public bool AreSnacks(PlayerTypes pt) { return total[pt] > 0; }
    public bool AreUneatenSnacks(PlayerTypes pt) { return eaten[pt] < total[pt]; }
    
    
    public SnackCount() {
        ZeroCounts();
    }
    
    
    // Doers
    public void ZeroCounts() {
        eaten = new Dictionary<PlayerTypes, int>();
        total = new Dictionary<PlayerTypes, int>();
        eaten[PlayerTypes.Any] = 0;
        total[PlayerTypes.Any] = 0;
        foreach (PlayerTypes playerType in PlayerTypeHelper.AllTypes) {
            eaten[playerType] = 0;
            total[playerType] = 0;
        }
    }
    public void Refresh(RoomData rd) {
        // Clear totals.
        ZeroCounts();
        // Recalculate totals!
        for (int i=0; i<rd.snackDatas.Count; i++) {
            PlayerTypes playerType = PlayerTypeHelper.TypeFromString(rd.snackDatas[i].playerType);
            total[playerType] ++;
            total[PlayerTypes.Any] ++;
            if (SaveStorage.GetBool(SaveKeys.DidEatSnack(rd, i))) {
                eaten[playerType] ++;
                eaten[PlayerTypes.Any] ++;
            }
        }
    }
    
    public void Add(SnackCount other) {
        foreach (PlayerTypes pt in other.total.Keys) {
            eaten[pt] += other.eaten[pt];
            total[pt] += other.total[pt];
        }
    }
}
