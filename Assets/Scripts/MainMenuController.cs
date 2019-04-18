using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour {


    // ----------------------------------------------------------------
    //  Button Events
    // ----------------------------------------------------------------
    public void OnClick_World(int worldIndex) {
        SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, worldIndex);
        SceneHelper.OpenScene(SceneNames.Gameplay);
    }

    public void OnClick_Quit() {
        Application.Quit();
    }



    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        RegisterButtonInput();
    }
    private void RegisterButtonInput() {
        bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // ~~~~ DEBUG ~~~~
        if (Input.GetKeyDown(KeyCode.J)) {
            SceneHelper.OpenScene(SceneNames.RoomJump); return;
        }
        else if (Input.GetKeyDown(KeyCode.M)) {
            SceneHelper.OpenScene(SceneNames.MapEditor); return;
        }


        // ALT + ___
        if (isKey_alt) {
        }
        // SHIFT + ___
        if (isKey_shift) {
        }
        // CONTROL + ___
        if (isKey_control) {
            // CONTROL + DELETE = Clear all save data!
            if (Input.GetKeyDown(KeyCode.Delete)) {
                GameManagers.Instance.DataManager.ClearAllSaveData();
                SceneHelper.ReloadScene();
                return;
            }
        }
    }
}
