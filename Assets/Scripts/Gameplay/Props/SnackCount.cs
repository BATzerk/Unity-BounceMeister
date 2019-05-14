using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnackCount {
    // Properties
    private Dictionary<PlayerTypes, int> eaten; // num eaten by PlayerType, including the Any type.
    private Dictionary<PlayerTypes, int> total; // num total by PlayerType, including the Any type.
    public int Eaten_All { get; private set; } // sum of ALL types total!
    //private int total_all; // sum of all other types.
    
    
    // Getters (Public)
    /// Returns num eaten by ONLY this PlayerType.
    public int Eaten(PlayerTypes pt) {
        if (pt == PlayerTypes.Any) { return eaten[pt]; }
        else { return eaten[pt] + eaten[PlayerTypes.Any]; }
        //return eaten[pt];
    }
    /// Returns num for this PlayerType, PLUS num for the ANY PlayerType! (E.g. if 5 Plunga and 3 Any, I'll return 8.)
    public int Total(PlayerTypes pt) {
        if (pt == PlayerTypes.Any) { return total[pt]; }
        else { return total[pt] + total[PlayerTypes.Any]; }
    }
    public bool AreSnacks(PlayerTypes pt) { return Total(pt) > 0; }
    public bool AreUneatenSnacks(PlayerTypes pt) { return Uneaten(pt) > 0; }
    public int Uneaten(PlayerTypes pt) { return Total(pt) - Eaten(pt); }
    
    
    // Initialize
    public SnackCount() {
        ZeroCounts();
    }
    
    
    // Doers
    public void ZeroCounts() {
        eaten = new Dictionary<PlayerTypes, int>();
        total = new Dictionary<PlayerTypes, int>();
        Eaten_All = 0;
        //eaten[PlayerTypes.Every] = 0;
        //total[PlayerTypes.Every] = 0;
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
            //total[PlayerTypes.Every] ++;
            if (SaveStorage.GetBool(SaveKeys.DidEatSnack(rd, i))) {
                eaten[playerType] ++;
                Eaten_All ++;
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
