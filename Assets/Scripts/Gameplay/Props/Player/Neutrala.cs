using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neutrala : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Neutrala; }


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
