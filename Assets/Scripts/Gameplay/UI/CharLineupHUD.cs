using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharLineupHUD : MonoBehaviour {
    // Properties
    private bool isCharTrial; // TRUE for char-trial cluster rooms.
    
    // Getters (Private)
    private CharLineup lineup { get { return GameManagers.Instance.DataManager.CharLineup; } }
    

    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Start () {
        // Add event listeners!
        //GameManagers.Instance.EventManager.CharLineupAddCharEvent += OnCharLineupAddChar;
        GameManagers.Instance.EventManager.StartRoomEvent += OnStartRoom;
        GameManagers.Instance.EventManager.SwapPlayerTypeEvent += OnSwapPlayerType;
    }
    private void OnDestroy() {
        // Remove event listeners!
        //GameManagers.Instance.EventManager.CharLineupAddCharEvent += OnCharLineupAddChar;
        GameManagers.Instance.EventManager.StartRoomEvent -= OnStartRoom;
        GameManagers.Instance.EventManager.SwapPlayerTypeEvent -= OnSwapPlayerType;
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    //private void OnCharLineupAddChar() {
    
    //}
    private void OnStartRoom(Room room) {
        isCharTrial = room.MyClusterData!=null && room.MyClusterData.IsCharTrial;
    }
    private void OnSwapPlayerType() {
    
    }
    

    
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void OnGUI() {
        if (!isCharTrial) { // Don't show ANYthing in a CharTrial.
            //GUI.color = Color.black;
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            for (int i=0; i<lineup.Lineup.Count; i++) {
                PlayerTypes pt = lineup.Lineup[i];
                //GUI.color = lineup.CurrTypeIndex == i ? new Color(1,0.8f,0) : Color.black;
                style.fontStyle = lineup.CurrTypeIndex == i ? FontStyle.Bold : FontStyle.Normal;
                GUI.Label(new Rect(14,Screen.height-40-i*30, 400,80), pt.ToString(), style);
            }
        }
    }

}
