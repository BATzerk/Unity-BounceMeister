using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeza : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Freeza; }
    // Components
    private FreezaBody freezaBody; // set in Start.
    // Properties
    public bool IsRoomFrozen { get; private set; }
    // References
    private GameTimeController gameTimeController;


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        gameTimeController = MyRoom.GameController.GameTimeController;
        freezaBody = myBody as FreezaBody;
        base.Start();
        
        SetIsRoomFrozen(false);
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetIsRoomFrozen(bool val) {
        IsRoomFrozen = val;
        gameTimeController.OnFreezaSetIsRoomFrozen(IsRoomFrozen);
        freezaBody.OnSetIsRoomFrozen(IsRoomFrozen);
    }
    private void ToggleIsRoomFrozen() { SetIsRoomFrozen(!IsRoomFrozen); }
    
    
    protected override void OnButtonAction_Press() {
        base.OnButtonAction_Press();
        ToggleIsRoomFrozen();
    }
    //// TEST!!
    //protected override void LandOnCollidable(Collidable collidable) {
    //    base.LandOnCollidable(collidable);
    //    SetIsRoomFrozen(false);
    //}
    //protected override void OnFeetLeaveCollidable(Collidable collidable) {
    //    base.OnFeetLeaveCollidable(collidable);
    //    if (!IsGrounded()) {
    //        SetIsRoomFrozen(true);
    //    }
    //}


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
