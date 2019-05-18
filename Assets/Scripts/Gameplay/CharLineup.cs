using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharLineup {
    // Properties
    public int CurrTypeIndex = 0; // type in Lineup.
    public List<PlayerTypes> Lineup; // who's in da lineuppp
    
    // Getters (Public)
    public bool CanCyclePlayerType() { return true; }
    public PlayerTypes GetNextPlayerType() {
        return Lineup[GetNextTypeIndex()];
    }
    private int GetNextTypeIndex() {
        if (CurrTypeIndex+1 >= Lineup.Count) { return 0; }
        return CurrTypeIndex+1;
    }
    
    
    // ----------------------------------------------------------------
    //  Save / Load
    // ----------------------------------------------------------------
    public void LoadFromSaveData() {
        string str = SaveStorage.GetString(SaveKeys.PlayerLineup, PlayerTypes.Neutrala.ToString());
        string[] strs = str.Split(',');
        Lineup = new List<PlayerTypes>();
        for (int i=0; i<strs.Length; i++) {
            Lineup.Add(PlayerTypeHelper.TypeFromString(strs[i]));
        }
    }
    private void SaveLineup() {
        string str = "";
        for (int i=0; i<Lineup.Count; i++) {
            str += Lineup[i];
            if (i < Lineup.Count-1) { str += ","; }
        }
        SaveStorage.SetString(SaveKeys.PlayerLineup, str);
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnSetCurrPlayerType(PlayerTypes pt) {
        // Update CurrTypeIndex!
        for (int i=0; i<Lineup.Count; i++) {
            if (pt == Lineup[i]) { CurrTypeIndex = i; return; }
        }
    }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void AddPlayerType(PlayerTypes pt) {
        Lineup.Add(pt);
        SaveLineup();
        //GameManagers.Instance.EventManager.On
    }
    public void Debug_RemovePlayerType(PlayerTypes pt) {
        Lineup.Remove(pt);
        SaveLineup();
    }
    
    
}
