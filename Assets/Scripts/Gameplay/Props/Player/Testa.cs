﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Testa is for DEVELOPMENT. Wanna quick-test a new character idea? Do it with Testa! */
public class Testa : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Testa; }


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