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
    private bool isSettingPlayerPos; // TRUE when we right-click to teleport the Player around the Level.
    public bool IsEditMode { get; private set; }
    private string currWindow; // current Editor window with focus. Used to detect when we give focus to the Scene (aka level-editor) window.



    // ----------------------------------------------------------------
    //  Awake
    // ----------------------------------------------------------------
    private void Awake() {
        // Not in EditMode? Destroy this script!
        if (!GameProperties.IsEditModeAvailable) {
            Destroy(this);
            return;
        }

        // Set references.
        gameController = GetComponent<GameController>();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateIsEditMode() {
        // It's EditMode if we're...
        IsEditMode = !Application.isFocused // A) NOT focused on the application, or...
                  || !isGameWindowFocus // B) NOT focused on the Game scene window, or...
                  || isSettingPlayerPos; // C) Setting the Player's pos (via right-click)!
        GameManagers.Instance.EventManager.OnSetIsEditMode(IsEditMode);
    }


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



    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        UpdateCurrWindow();
        UpdateSettingPlayerPos();
    }

    private void UpdateCurrWindow() {
        string window = UnityEditor.EditorWindow.focusedWindow.titleContent.text;
        if (currWindow != window) {
            if (currWindow == WindowName_Game) { OnGameWindowFocus(false); } // LOST Game focus?
            else if (window == WindowName_Game) { OnGameWindowFocus(true); } // GAINED Game focus?
            currWindow = window;
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
            }
        }
    }


}
