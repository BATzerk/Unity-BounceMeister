using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditModeUI : MonoBehaviour {
    // Components
    [SerializeField] private Image i_editModeBorder=null;
    //[SerializeField] private TextMeshProUGUI t_roomName=null;
    [SerializeField] private TMP_InputField tif_roomName=null;
    // References
    //	[SerializeField] private GameController gameControllerRef;
    private Room room;

    // Getters
    private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
    private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }


    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Start() {
        tif_roomName.interactable = GameProperties.IsEditModeAvailable;

        // Add event listeners!
        eventManager.SetIsEditModeEvent += OnSetIsEditMode;
        eventManager.StartRoomEvent += OnStartRoom;
    }
    private void OnDestroy() {
        // Remove event listeners!
        eventManager.SetIsEditModeEvent -= OnSetIsEditMode;
        eventManager.StartRoomEvent -= OnStartRoom;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSetIsEditMode(bool isEditMode) {
        i_editModeBorder.enabled = isEditMode;
    }
    private void OnStartRoom(Room _room) {
        room = _room;
        ResetRoomNameText();
    }

    public void OnRoomNameTextChanged() {
        // Change color if there's a naming conflict!
        string newName = tif_roomName.text;
        Color color;
        if (newName == room.RoomKey) { color = Color.black; } // Same name? Black.
        else if (RoomSaverLoader.MayRenameRoomFile(room,newName)) { color = new Color(130/255f, 160/255f, 40/255f); } // Can rename? Green!
        else { color = new Color(140/255f, 55/255f, 40/255f); } // CAN'T rename? Red.
        // Apply the color.
        foreach (TextMeshProUGUI t in tif_roomName.GetComponentsInChildren<TextMeshProUGUI>()) {
            t.color = color;
        }
    }
    public void OnRoomNameTextEndEdit() {
        string newName = tif_roomName.text;
        // MAY rename!
        if (RoomSaverLoader.MayRenameRoomFile(room, newName)) {
            // Rename the room file, and reload all RoomDatas!
            RoomSaverLoader.RenameRoomFile(room, newName);
            dataManager.ReloadWorldDatas();
            // Save that this is the most recent room we've been to!
            SaveStorage.SetString(SaveKeys.LastPlayedRoomKey(room.WorldIndex),newName);
            dataManager.currRoomData = dataManager.GetRoomData(room.WorldIndex,room.RoomKey, false);
            // Reload the scene for safety.
            SceneHelper.ReloadScene();
        }
        // May NOT rename.
        else {
            ResetRoomNameText();
        }
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void ResetRoomNameText() {
        tif_roomName.text = room.RoomKey;
    }


}



