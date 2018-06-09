using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alph : Player {
	// Overrides
	private Vector2 GravityPlunging = new Vector2(0, -0.084f); // gravity is much stronger when we're plunging!
	override protected Vector2 Gravity {
		get {
			if (isPlunging) { return GravityPlunging; }
			return GravityNeutral;
		}
	}
	// Properties
	private bool isPlunging = false;
	private bool isPlungeRecharged = true;
	private bool groundedSincePlunge; // TEST for interactions with Batteries.
	// References
	private AlphBody myAlphBody;


	// Getters (Public)
	override public bool CanUseBattery() { return isPlungeRecharged; }
	public bool IsPlungeRecharged { get { return isPlungeRecharged; } }
	// Getters (Protected)
//	override public bool IsAffectedByLift() { return !isPlunging; } // We're immune to Lifts while plunging!
	override protected bool MayWallSlide() {
		return base.MayWallSlide() && !isPlunging;
	}
	override protected bool DoBounceOffCollidable(Collidable collidable) {
		if (isPlunging) { return true; }
		return base.DoBounceOffCollidable(collidable);
	}
	// Getters (Private)
	private bool CanStartPlunge() {
		if (feetOnGround()) { return false; } // I can't plunge if I'm on the ground.
		if (IsInLift) { return false; }
		return isPlungeRecharged;
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		myAlphBody = myBody as AlphBody;

		base.Start();
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	override protected void UpdateMaxYSinceGround() {
		// TEST!!
		if (!groundedSincePlunge) { return; }
		base.UpdateMaxYSinceGround();
	}


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
		else if (CanStartPlunge()) {
			StartPlunge();
		}
		else {
			ScheduleDelayedJump();
		}
	}
	override protected void OnDownPressed() {
		base.OnDownPressed();
		if (CanStartPlunge()) {
			StartPlunge();
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void StartPlunge() {
		if (isPlunging) { return; } // Already plunging? Do nothing.
		StopWallSlide(); // can't both plunge AND wall-slide.
		isPlunging = true;
		isPlungeRecharged = false; // spent!
		groundedSincePlunge = false;
		isPreservingWallKickVel = false; // When we plunge, forget about retaining my wall-kick vel!
		myAlphBody.OnStartPlunge();
		vel = new Vector2(vel.x, Mathf.Min(vel.y, 0)); // lose all upward momentum!
		GameManagers.Instance.EventManager.OnPlayerStartPlunge(this);
	}
	private void StopPlunge() {
		if (!isPlunging) { return; } // Not plunging? Do nothing.
		isPlunging = false;
		myAlphBody.OnStopPlunge();
	}
	private void RechargePlunge() {
		isPlungeRecharged = true;
		myAlphBody.OnRechargePlunge();
		GameManagers.Instance.EventManager.OnPlayerRechargePlunge(this);
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override protected void BounceOffCollidable_Up(Collidable collidable) {
		// Bouncing up off a surface stops the plunge.
		StopPlunge();
		// Base call.
		base.BounceOffCollidable_Up(collidable);
	}
	override protected void LandOnCollidable(Collidable collidable) {
		// Landing on a surface stops the plunge.
		StopPlunge();
		groundedSincePlunge = true;
		// Is this collidable refreshing? Recharge my plunge!
		if (collidable==null || collidable.DoRechargePlayer) {
			RechargePlunge();
		}
		// Base call.
		base.LandOnCollidable(collidable);
	}

	override public void OnEnterLift() {
		base.OnEnterLift();
		if (isPlunging) {
			StopPlunge();
		}
	}


	override public void OnUseBattery() {
		RechargePlunge();
	}

}
