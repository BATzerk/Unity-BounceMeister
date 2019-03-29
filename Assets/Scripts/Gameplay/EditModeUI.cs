using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditModeUI : MonoBehaviour {
    // Components
    [SerializeField] private Image i_editModeBorder=null;
    //[SerializeField] private TextMeshProUGUI t_levelName=null;
    [SerializeField] private TMP_InputField tif_levelName=null;
    // References
    //	[SerializeField] private GameController gameControllerRef;
    private Level level;

    // Getters
    private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
    private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }


    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Start() {
        tif_levelName.interactable = GameProperties.IsEditModeAvailable;

        // Add event listeners!
        eventManager.SetIsEditModeEvent += OnSetIsEditMode;
        eventManager.StartLevelEvent += OnStartLevel;
    }
    private void OnDestroy() {
        // Remove event listeners!
        eventManager.SetIsEditModeEvent -= OnSetIsEditMode;
        eventManager.StartLevelEvent -= OnStartLevel;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSetIsEditMode(bool isEditMode) {
        i_editModeBorder.enabled = isEditMode;
    }
    private void OnStartLevel(Level _level) {
        level = _level;
        ResetLevelNameText();
    }

    public void OnLevelNameTextChanged() {
        // Change color if there's a naming conflict!
        string newName = tif_levelName.text;
        Color color;
        if (newName == level.LevelKey) { color = Color.black; } // Same name? Black.
        else if (LevelSaverLoader.MayRenameLevelFile(level,newName)) { color = new Color(130/255f, 160/255f, 40/255f); } // Can rename? Green!
        else { color = new Color(140/255f, 55/255f, 40/255f); } // CAN'T rename? Red.
        // Apply the color.
        foreach (TextMeshProUGUI t in tif_levelName.GetComponentsInChildren<TextMeshProUGUI>()) {
            t.color = color;
        }
    }
    public void OnLevelNameTextEndEdit() {
        string newName = tif_levelName.text;
        // MAY rename!
        if (LevelSaverLoader.MayRenameLevelFile(level, newName)) {
            // Rename the level file, and reload all LevelDatas!
            LevelSaverLoader.RenameLevelFile(level, newName);
            dataManager.ReloadWorldDatas();
            // Save that this is the most recent level we've been to!
            SaveStorage.SetString(SaveKeys.LastPlayedLevelKey(level.WorldIndex),newName);
            dataManager.currentLevelData = dataManager.GetLevelData(level.WorldIndex,level.LevelKey, false);
            // Reload the scene for safety.
            SceneHelper.ReloadScene();
        }
        // May NOT rename.
        else {
            ResetLevelNameText();
        }
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void ResetLevelNameText() {
        tif_levelName.text = level.LevelKey;
    }


}



