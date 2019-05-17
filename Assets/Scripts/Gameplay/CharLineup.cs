using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharLineup {
    // Properties
    public int CurrTypeIndex = 0; // type in playerTypesAvailable.
    public List<PlayerTypes> Lineup; // who's in da lineuppp
    
    // Getters (Public)
    public bool CanCyclePlayerType() { return true; }
    public PlayerTypes GetNextPlayerType(PlayerTypes currType) {
        CurrTypeIndex ++;
        if (CurrTypeIndex >= Lineup.Count) { CurrTypeIndex = 0; }
        return Lineup[CurrTypeIndex];
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
        
        //// Temp hack for player cycling.
        //for (int i=0; i<PlayerTypesAvailable.Length; i++) {
        //    if (Player.PlayerType() == PlayerTypesAvailable[i]) { currTypeIndex = i; break; }
        //}
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
    //  Doers
    // ----------------------------------------------------------------
    public void AddPlayerType(PlayerTypes pt) {
        Lineup.Add(pt);
        SaveLineup();
        //GameManagers.Instance.EventManager.On
    }
    
    
}
