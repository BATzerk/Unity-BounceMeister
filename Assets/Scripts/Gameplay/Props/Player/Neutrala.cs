using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neutrala : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Neutrala; }
    // Properties
    public bool IsRoomFrozen { get; private set; } // TEST!!
    // References
    private GameTimeController gameTimeController;


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        gameTimeController = MyRoom.GameController.GameTimeController;
        SetIsRoomFrozen(false);
        base.Start();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetIsRoomFrozen(bool val) {
        IsRoomFrozen = val;
        gameTimeController.OnNeutralaSetIsRoomFrozen(IsRoomFrozen);
    }
    private void ToggleIsRoomFrozen() { SetIsRoomFrozen(!IsRoomFrozen); }
    
    
    //// TEST!! HACK!
    //private int numBonusFixedUpdates;
    //protected override void FixedUpdate() {
    //    base.FixedUpdate();
        
    //    if (IsSlowMo && numBonusFixedUpdates<10) {
    //        numBonusFixedUpdates ++;
    //        FixedUpdate();
    //    }
    //    numBonusFixedUpdates = 0;
    //}
    
    //protected override void OnButtonAction_Press() {
    //    base.OnButtonAction_Press();
    //    ToggleIsRoomFrozen();
    //}
    // TEST!!
    protected override void LandOnCollidable(Collidable collidable) {
        base.LandOnCollidable(collidable);
        SetIsRoomFrozen(false);
    }
    protected override void OnFeetLeaveCollidable(Collidable collidable) {
        base.OnFeetLeaveCollidable(collidable);
        if (!IsGrounded()) {
            SetIsRoomFrozen(true);
        }
    }


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Press() {
		if (MayWallKick()) {
			WallKick();
		}
		else if (MayJump()) {
			Jump();
		}
		else {
			ScheduleDelayedJump();
		}
	}




}
