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
    private bool isExecutingOneFUStep = false; // if TRUE, then at the end of FixedUpdate, we'll pause the game and this will be set to false.


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
    private void SetIsPaused(bool val) {
        isPaused = val;
        UpdateTimeScale();
        GameManagers.Instance.EventManager.OnSetPaused(isPaused);
    }
    public void TogglePause() {
        SetIsPaused(!isPaused);
    }
    public void ToggleSlowMo() {
        isSlowMo = !isSlowMo;
        UpdateTimeScale();
    }
    /// Will execute ABOUT 1 FixedUpdate step. It's pretty accurate, certainly enough for its debugging purpose.
    public void ExecuteApproximatelyOneFUStep() {
        if (!isPaused) { SetIsPaused(true); } // Not paused? Pause.
        StartCoroutine(Coroutine_ExecuteApproximatelyOneFUStep());
    }
    private IEnumerator Coroutine_ExecuteApproximatelyOneFUStep() {
        isExecutingOneFUStep = true;
        UpdateTimeScale();
        yield return null;
        
        isExecutingOneFUStep = false;
        UpdateTimeScale();
        yield return null;
    }

    // ----------------------------------------------------------------
    //  Doers (Private)
    // ----------------------------------------------------------------
    private void UpdateTimeScale() {
        if (isExecutingOneFUStep) { Time.timeScale = 1; }
        else if (isPaused) { Time.timeScale = 0; }
        else if (editModeController.IsEditMode && GameProperties.DoPauseInEditMode) { Time.timeScale = 0; }
        else if (isSlowMo) { Time.timeScale = 0.2f; }
        else { Time.timeScale = 1; }
    }
    

    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    //private void FixedUpdate() {
    //    // If we planned on executing a single FixedUpdate step, good, we've done it! Freeze time again.
    //    if (isExecutingOneFUStep) {
    //        isExecutingOneFUStep = false;
    //        UpdateTimeScale();
    //    }
    //}


}
