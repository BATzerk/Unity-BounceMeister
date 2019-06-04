using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSwapController : MonoBehaviour {
    // Components
    [SerializeField] private GameController gameController=null;
    // Properties
    public bool IsCharSwapping { get; private set; } // if TRUE, then the game's paused until we select a char!
    
    // Getters (Private)
    private CharLineup charLineup { get { return GameManagers.Instance.DataManager.CharLineup; } }
    private Room CurrRoom { get { return gameController.CurrRoom; } }
    private bool CanCyclePlayerType() {
        if (IsCharSwapping) { return true; } // Already swapping? Sure, allow free selection!
        if (CurrRoom.Player.IsDead) { return false; } // Dead? Can't cycle.
        if (!CurrRoom.Player.IsGrounded()) { return false; } // Not grounded? Can't cycle.
        if (CurrRoom.MyClusterData!=null && CurrRoom.MyClusterData.IsCharTrial) { return false; } // Char Trial? No cycling.
        return charLineup.CanCyclePlayerType(); // Ask CharLineup.
    }
    
    private bool Temp_IsTrialEnd(Room _room) {
        return _room.RoomKey.EndsWith("TrialEnd", System.StringComparison.Ordinal);
    }
    private PlayerTypes Temp_GetTrialEndPlayerType(Room _room) {
        string rk = _room.RoomKey;
        string typeStr = rk.Substring(0, rk.IndexOf("TrialEnd", System.StringComparison.Ordinal));
        return PlayerTypeHelper.TypeFromString(typeStr);
    }
    
    
    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Start() {
        // Add event listeners!
        GameManagers.Instance.EventManager.SetPlayerType += OnSetPlayerType;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.SetPlayerType -= OnSetPlayerType;
    }
    

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSetPlayerType(Player player) {
        // Tell CharLineup.
        charLineup.OnSetCurrPlayerType(player.PlayerType());
    }
    public void Temp_OnPlayerTouchRoomDoor(RoomDoor rd) {
        if (Temp_IsTrialEnd(rd.MyRoom)) {
            PlayerTypes playerType = Temp_GetTrialEndPlayerType(rd.MyRoom);
            charLineup.AddPlayerType(playerType);
        }
    }
    public void OnButton_CycleChar() {
        if (CanCyclePlayerType()) { // If we can...!
            SetIsCharSwapping(true);
            PlayerTypes nextType = charLineup.GetNextPlayerType();
            gameController.SetPlayerType(nextType);
        }
    }

    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetIsCharSwapping(bool val) {
        IsCharSwapping = val;
        GameManagers.Instance.EventManager.OnSetIsCharSwapping(val);
    }

    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        RegisterInput();
    }
    private void RegisterInput() {
        if (IsCharSwapping) { // We're char-swapping?
            // If there's any character input, stop the swapping-ness!
            if (InputController.Instance.LeftStickRaw.magnitude > 0.2f
            || InputController.Instance.IsAction_Press
            || InputController.Instance.IsJump_Press) {
                SetIsCharSwapping(false);
            }
        }
    }


    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //  Debug
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void Debug_TogPlayerInLineup(PlayerTypes pt) {
        if (!charLineup.Lineup.Contains(pt)) {
            gameController.SetPlayerType(pt);
            charLineup.AddPlayerType(pt);
        }
        else {
            charLineup.Debug_RemovePlayerType(pt);
        }
    }
    
    
}
