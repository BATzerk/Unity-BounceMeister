using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Britta : Player {
	// Overrides
	override protected float InputScaleX { get { return 0.03f; } }
	override protected float FrictionGround {
		get {
			if (Mathf.Abs(inputAxis.x) > 0.1f) { return 0.95f; } // Providing input? Less friction!
			return 0.86f;
		}
	}
	override protected Vector2 Gravity {
		get {
			if (isTouchingWall()) { return GravityNeutral * 0.4f; } // On a wall? Barely any gravity!
			return GravityNeutral;
		}
	}
	override protected float MaxVelXAir { get { return 0.5f; } }
	override protected float MaxVelXGround { get { return 0.5f; } }

	override protected float WallSlideMinYVel { get { return -0.3f; } }



	// ----------------------------------------------------------------
	//  Input
	// ----------------------------------------------------------------
	override protected void OnUpPressed() {
		// We're on the ground and NOT timed out of jumping! Go!
		if (feetOnGround()) {//numJumpsSinceGround<MaxJumps && Time.time>=timeWhenCanJump
			GroundJump();
		}
		else if (isTouchingWall()) {
			WallKick();
		}
//		else if (CanStartPlunge()) {
//			StartPlunge();
//		}
		else {
			ScheduleDelayedJump();
		}
	}
}
