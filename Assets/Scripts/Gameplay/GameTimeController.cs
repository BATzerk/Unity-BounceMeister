using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Add this to GameController GameObject.
 * I manage pausing, slow-mo, edit-mode-pausing, etc. */
public class GameTimeController : MonoBehaviour {
    // Components
    [SerializeField] private EditModeController editModeController=null;
    // Properties
    private bool isPaused = false;
    private bool isSlowMo = false;


    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake() {
        // Add event listeners!
        GameManagers.Instance.EventManager.StartLevelEvent += OnStartLevel;
        GameManagers.Instance.EventManager.SetIsEditModeEvent += OnSetIsEditMode;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.StartLevelEvent -= OnStartLevel;
        GameManagers.Instance.EventManager.SetIsEditModeEvent -= OnSetIsEditMode;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnStartLevel(Level level) {
        UpdateTimeScale();
    }
    private void OnSetIsEditMode(bool isEditMode) {
        UpdateTimeScale();
    }



    // ----------------------------------------------------------------
    //  Doers (Public)
    // ----------------------------------------------------------------
    public void TogglePause() {
        isPaused = !isPaused;
        UpdateTimeScale();
        GameManagers.Instance.EventManager.OnSetPaused(isPaused);
    }
    public void ToggleSlowMo() {
        isSlowMo = !isSlowMo;
        UpdateTimeScale();
    }


    // ----------------------------------------------------------------
    //  Doers (Private)
    // ----------------------------------------------------------------
    private void UpdateTimeScale() {
        if (isPaused) { Time.timeScale = 0; }
        else if (editModeController.IsEditMode && GameProperties.DoPauseInEditMode) { Time.timeScale = 0; }
        else if (isSlowMo) { Time.timeScale = 0.2f; }
        else { Time.timeScale = 1; }
    }


}
