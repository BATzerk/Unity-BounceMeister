using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Add this to GameController GameObject.
 * I manage pausing, slow-mo, edit-mode-pausing, etc. */
public class GameTimeController : MonoBehaviour {
    // Components
    [SerializeField] private EditModeController editModeController=null;
    // Properties
    public bool IsPaused { get; private set; }
    public bool IsFastMo { get; private set; }
    public bool IsSlowMo { get; private set; }
    public bool IsExecutingOneFUStep { get; private set; } // if TRUE, then at the end of FixedUpdate, we'll pause the game and this will be set to false.
    private float tsFromPlayer; // 1 by default. How much this Player affects TimeScale. (E.g. Flatline slows time when going fast.)
    
    // Getters
    //private bool IsManuallyControllingTime() {
    //    return IsPaused || IsSlowMo || IsExecutingOneFUStep;
    //}


    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake() {
        // Add event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent += OnStartRoom;
        GameManagers.Instance.EventManager.SetIsEditModeEvent += OnSetIsEditMode;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent -= OnStartRoom;
        GameManagers.Instance.EventManager.SetIsEditModeEvent -= OnSetIsEditMode;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnStartRoom(Room room) {
        tsFromPlayer = 1;
        UpdateTimeScale();
    }
    private void OnSetIsEditMode(bool isEditMode) {
        UpdateTimeScale();
    }



    // ----------------------------------------------------------------
    //  Doers (Public)
    // ----------------------------------------------------------------
    private void SetIsPaused(bool val) {
        IsPaused = val;
        UpdateTimeScale();
        GameManagers.Instance.EventManager.OnSetPaused(IsPaused);
    }
    public void TogglePause() {
        SetIsPaused(!IsPaused);
    }
    public void ToggleSlowMo() {
        IsSlowMo = !IsSlowMo;
        UpdateTimeScale();
        SetIsPaused(false); // AUTOMATically unpause when we set slow-mo-ness.
    }
    public void SetIsFastMo(bool val) {
        IsFastMo = val;
        UpdateTimeScale();
    }
    /// Will execute ABOUT 1 FixedUpdate step. It's pretty accurate, certainly enough for its debugging purpose.
    public void ExecuteApproximatelyOneFUStep() {
        if (!IsPaused) { SetIsPaused(true); } // Not paused? Pause.
        StartCoroutine(Coroutine_ExecuteApproximatelyOneFUStep());
    }
    private IEnumerator Coroutine_ExecuteApproximatelyOneFUStep() {
        IsExecutingOneFUStep = true;
        UpdateTimeScale();
        yield return null;
        
        IsExecutingOneFUStep = false;
        UpdateTimeScale();
        yield return null;
    }

    // ----------------------------------------------------------------
    //  Doers (Private)
    // ----------------------------------------------------------------
    private void UpdateTimeScale() {
        if (IsExecutingOneFUStep) { Time.timeScale = 1; }
        else if (IsPaused) { Time.timeScale = 0; }
        else if (editModeController.IsEditMode && GameProperties.DoPauseInEditMode) { Time.timeScale = 0; }
        else if (IsSlowMo) { Time.timeScale = 0.2f; }
        else if (IsFastMo) { Time.timeScale = 4f; }
        else { Time.timeScale = tsFromPlayer; }
    }


    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    public void UpdateFromFlatline(Flatline flatline) {
        tsFromPlayer = 0.6f/flatline.vel.magnitude;
        tsFromPlayer = Mathf.Clamp(tsFromPlayer, 0.2f,1f);
        UpdateTimeScale();
        //print(Time.frameCount + "  tsFromPlayer: " + tsFromPlayer);
    }
    public void UpdateFromDilata(Dilata dilata) {
        if (dilata.IsDilatingTime) {
            tsFromPlayer = 0.4f/dilata.vel.magnitude;
            tsFromPlayer = Mathf.Clamp(tsFromPlayer, 0.2f,5f);
            UpdateTimeScale();
            print(Time.frameCount + "  tsFromPlayer: " + tsFromPlayer);
        }
    }
    public void OnDilataSetIsDilatingTime(bool val) {
        if (!val) { tsFromPlayer = 1; } // Not dilating? Reset tsFromPlayer.
        UpdateTimeScale();
    }
    //private void FixedUpdate() {
    //    // If we planned on executing a single FixedUpdate step, good, we've done it! Freeze time again.
    //    if (isExecutingOneFUStep) {
    //        isExecutingOneFUStep = false;
    //        UpdateTimeScale();
    //    }
    //}


}
