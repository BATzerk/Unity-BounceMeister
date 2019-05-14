using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Add this to the GameController GameObject. I control...
 A) When we consider it to be EditMode (it's by window focus).
 B) Debug-moving the Player with right-click.
 */
public class EditModeController : MonoBehaviour {
    // Constants
    private const string WindowName_Game = "Game"; // And Edit window is "Scene".
    //private const string WindowName_Scene = "Scene"; // And Game window is "Game".
    // Components
    private GameController gameController; // set in Awake.
    // Properties
    private bool isGameWindowFocus;
    private bool isSettingPlayerPos; // TRUE when we right-click to teleport the Player around the Room.
    public bool IsEditMode { get; private set; }
    private string currWindow; // current Editor window with focus. Used to detect when we give focus to the Scene (aka room-editor) window.
    // References
    private GameObject prevSelectedGO; // for checking when we change our UI selection.
    
    // Getters
    private DataManager dm { get { return GameManagers.Instance.DataManager; } }
    private Room CurrRoom { get { return gameController.CurrRoom; } }


    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake() {
        // Not in EditMode? Destroy this script!
        if (!GameProperties.IsEditModeAvailable) {
            Destroy(this);
            return;
        }

        // Set references.
        gameController = GetComponent<GameController>();
        
        // Add event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent += OnStartRoom;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent -= OnStartRoom;
    }



#if UNITY_EDITOR
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnApplicationFocus(bool focus) {
        UpdateIsEditMode();
    }
    private void OnGameWindowFocus(bool isFocus) {
        isGameWindowFocus = isFocus;
        UpdateIsEditMode();
    }
    private void OnStartRoom(Room room) {
        // Expand the hierarchy for easier Room-editing!
        ExpandRoomHierarchy();
        GameUtils.SetEditorCameraPos(room.PosGlobal); // conveniently move the Unity Editor camera, too!
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateIsEditMode() {
        // It's EditMode if we're...
        IsEditMode = !Application.isFocused // A) NOT focused on the application, or...
                  || !isGameWindowFocus // B) NOT focused on the Game scene window, or...
                  || isSettingPlayerPos // C) Setting the Player's pos (via right-click), or...
                  || GameUtils.CurrSelectedGO()!=null; // D) Selecting any UI!
        GameManagers.Instance.EventManager.OnSetIsEditMode(IsEditMode);
    }
    private void ExpandRoomHierarchy() {
        if (!GameUtils.IsEditorWindowMaximized()) { // If we're maximized, do nothing (we don't want to open up the Hierarchy if it's not already open).
            GameUtils.SetExpandedRecursive(CurrRoom.gameObject, true); // Open up Room all the way down.
            for (int i=0; i<CurrRoom.transform.childCount; i++) { // Ok, now (messily) close all its children.
                GameUtils.SetExpandedRecursive(CurrRoom.transform.GetChild(i).gameObject, false);
            }
            GameUtils.FocusOnWindow("Game"); // focus back on Game window.
            //GameUtils.SetGOCollapsed(transform.parent, false);
            //GameUtils.SetGOCollapsed(tf_world, false);
            //GameUtils.SetGOCollapsed(room.transform, false);
        }
    }
    public void SaveRoomFile() {
        // Save it!
        RoomSaverLoader.SaveRoomFile(CurrRoom);
        // Update properties that may have changed.
        if (CurrRoom.MyClusterData != null) {
            CurrRoom.MyClusterData.RefreshSnackCount();
        }
        // Update total edibles counts!
        dm.RefreshSnackCountGame();
    }
    private void StartNewBlankRoom() {
        // Keep it in the current world, and give it a unique name.
        WorldData wd = dm.GetWorldData(CurrRoom.WorldIndex);
        string roomKey = wd.GetUnusedRoomKey();
        RoomData emptyRD = wd.GetRoomData(roomKey, true);
        gameController.StartGameAtRoom(emptyRD);
    }
    private void DuplicateCurrRoom() {
        // Add a new room file, yo!
        RoomData currRD = CurrRoom.MyRoomData;
        string newRoomKey = currRD.MyWorldData.GetUnusedRoomKey(currRD.RoomKey);
        RoomSaverLoader.SaveRoomFileAs(currRD, currRD.WorldIndex, newRoomKey);
        dm.ReloadWorldDatas();
        RoomData newLD = dm.GetRoomData(currRD.WorldIndex,newRoomKey, false);
        newLD.SetPosGlobal(newLD.PosGlobal + new Vector2(15,-15)*GameProperties.UnitSize); // offset its position a bit.
        RoomSaverLoader.UpdateRoomPropertiesInRoomFile(newLD); // update file!
        dm.currRoomData = newLD;
        SceneHelper.ReloadScene();
    }
    private void OpenRoomTextFile() {
        string path = FilePaths.RoomFile(CurrRoom.WorldIndex, CurrRoom.RoomKey);
        Application.OpenURL("File:" + path);
    }
    

    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private Vector2 pmousePosWorld;
    private void Update() {
        UpdateCurrWindow();
        UpdateSettingPlayerPos();
        UpdateCurrSelectedGO();
        RegisterButtonInput();
        
        pmousePosWorld = InputController.Instance.MousePosWorld;
    }

    private void UpdateCurrWindow() {
        if (UnityEditor.EditorWindow.focusedWindow != null) { // If there IS a focused window (e.g. not stopping the game in Unity Editor)...
            string window = UnityEditor.EditorWindow.focusedWindow.titleContent.text;
            if (currWindow != window) {
                if (currWindow == WindowName_Game) { OnGameWindowFocus(false); } // LOST Game focus?
                else if (window == WindowName_Game) { OnGameWindowFocus(true); } // GAINED Game focus?
                currWindow = window;
            }
        }
    }

    private void UpdateSettingPlayerPos() {
        if (gameController.Player != null) {
            // Start or Stop isSettingPlayerPos?
            if (Input.GetMouseButtonDown(1)) {
                isSettingPlayerPos = true;
                UpdateIsEditMode();
            }
            else if (Input.GetMouseButtonUp(1)) {
                isSettingPlayerPos = false;
                UpdateIsEditMode();
            }
            // We are setting Player pos??
            if (isSettingPlayerPos) {
                gameController.Player.SetPosGlobal(InputController.Instance.MousePosWorld);
                gameController.Player.SetVel(InputController.Instance.MousePosWorld - pmousePosWorld);
            }
        }
    }
    private void UpdateCurrSelectedGO() {
        GameObject currSelectedGO = GameUtils.CurrSelectedGO();
        if (prevSelectedGO != currSelectedGO) {
            prevSelectedGO = currSelectedGO;
            UpdateIsEditMode();
        }
    }
    
    private void RegisterButtonInput() {
        // Canvas has a selected element? Ignore ALL button input.
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
            return;
        }

        bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);


        // SHIFT + ___
        if (isKey_shift) {
            // SHIFT + S = Save room as text file!
            if (Input.GetKeyDown(KeyCode.S)) {
                SaveRoomFile();
            }
        }
        // CONTROL + ___
        if (isKey_control) {
            // CONTROL + N = Create/Start new room!
            if (Input.GetKeyDown(KeyCode.N)) { StartNewBlankRoom(); }
            // CONTROL + D = Duplicate/Start new room!
            else if (Input.GetKeyDown(KeyCode.D)) { DuplicateCurrRoom(); }
            // CONTROL + K = Open room text file!
            else if (Input.GetKeyDown(KeyCode.K)) { OpenRoomTextFile(); }
            // CONTROL + SHIFT + ____...
            else if (isKey_shift) {
                if (CurrRoom != null) {
                    // CONTROL + SHIFT + X = Flip Horizontal!
                    if (Input.GetKeyDown(KeyCode.X)) { CurrRoom.FlipHorz(); }
                    // CONTROL + SHIFT + [ARROW KEYS] = Move all Props!
                    else if (Input.GetKeyDown(KeyCode.LeftArrow))  { CurrRoom.MoveAllProps(Vector2Int.L); }
                    else if (Input.GetKeyDown(KeyCode.RightArrow)) { CurrRoom.MoveAllProps(Vector2Int.R); }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))  { CurrRoom.MoveAllProps(Vector2Int.B); }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))    { CurrRoom.MoveAllProps(Vector2Int.T); }
                }
            }
        }
    }

#endif
}
