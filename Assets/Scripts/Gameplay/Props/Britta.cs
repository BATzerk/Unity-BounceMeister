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
			if (isReducedJumpGravity) { return GravityNeutral * 0.5f; } // We're still holding down the jump button? Reduce gravity!
			if (isTouchingWall()) { return GravityNeutral * 0.7f; } // On a wall? Reduce gravity!
			return GravityNeutral;
		}
	}
	override protected float MaxVelXAir { get { return 0.5f; } }
	override protected float MaxVelXGround { get { return 0.5f; } }

	override protected float JumpForce { get { return 0.48f; } }
	override protected float WallSlideMinYVel { get { return -0.5f; } }

	// Properties
	private bool isReducedJumpGravity; // true when we jump. False when A) We release the jump button, or B) We hit our jump apex.


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	override protected void Jump() {
		base.Jump();
		isReducedJumpGravity = true;
	}
	override protected void WallKick() {
		base.WallKick();
		isReducedJumpGravity = true;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	override protected void OnHitJumpApex() {
		isReducedJumpGravity = false; // the moment we start descending, stop giving us reduced gravity.
	}
	override protected void StartWallSlide(int side) {
		base.StartWallSlide(side);
		// Convert all our horizontal speed to vertical speed!
		float newYVel = Mathf.Abs(vel.x)*0.7f + Mathf.Max(0, vel.y);
		SetVel(new Vector2(vel.x, newYVel));
	}

	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
//	override public void OnWhiskersTouchCollider(int side, Collider2D col) {
//		base.OnWhiskersTouchCollider(side, col);
//
//		// Touched a side?
//		if (side==Sides.L || side==Sides.R) {
//
//		}
//	}


	// ----------------------------------------------------------------
	//  Input
	// ----------------------------------------------------------------
	override protected void OnUp_Down() {
		if (MayWallKick()) {
			WallKick();
		}
		else if (MayJump()) {
			Jump();
		}
//		else if (CanStartPlunge()) {
//			StartPlunge();
//		}
		else {
			ScheduleDelayedJump();
		}
	}
	override protected void OnUp_Up() {
		isReducedJumpGravity = false; // Not anymore, boss!
	}


}
