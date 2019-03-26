using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coco : Player {
	// Overrides
//	private Vector2 GravityJettingWhileFalling = new Vector2(0, 0.02f); // gravity will actually work WITH us while we're falling! So we halt super fast.
//	override protected Vector2 Gravity {
//		get {
//			if (isJetting && vel.y<0) { return GravityJettingWhileFalling; }
//			return GravityNeutral;
//		}
//	}
	private Vector2 GravityJetting = new Vector2(0, 0); // TEST
	override protected Vector2 Gravity {
		get {
			if (isJetting) { return GravityJetting; }
			return GravityNeutral;
		}
	}
	// Constants
	private const float JetCapacityDuration = 1f; // in SECONDS, how long we have to jet until recharging.
	private const float JetFuelCapacity = 100f; // this number doesn't matter at *all*. Just has to be something.
	private const float JetSpendRate = JetFuelCapacity/JetCapacityDuration; // how much fuel we spend PER SECOND.
	private const float JetTargetYVel = 0.25f; // TEST
//	private readonly Vector2 JetForce = new Vector2(0, 0.05f);
	// Properties
	private bool isJetting = false;
	private bool groundedSinceJet; // TEST for interactions with Batteries.
	private float jetFuelLeft;
	// References
	private CocoBody myCocoBody;


	// Getters (Public)
	override public bool CanUseBattery() { return !IsFuelFull; }
	public bool IsFuelEmpty { get { return jetFuelLeft <= 0; } }
	public bool IsFuelFull { get { return jetFuelLeft >= JetFuelCapacity; } }
	// Getters (Protected)
	override protected bool MayWallSlide() {
		return base.MayWallSlide() && !isJetting;
	}
	// Getters (Private)
	private bool CanStartJetting() {
		if (feetOnGround()) { return false; } // I can't jet if I'm on the ground.
//		if (IsInLift) { return false; }
		return !IsFuelEmpty;
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		myCocoBody = myBody as CocoBody;

		SetJetFuelLeft(JetFuelCapacity);

		base.Start();
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	override protected void FixedUpdate() {
		UpdateJetting();
		base.FixedUpdate();
	}
	override protected void UpdateMaxYSinceGround() {
		// TEST
		if (!groundedSinceJet) { return; }
		base.UpdateMaxYSinceGround();
	}
	private void UpdateJetting() {
		if (isJetting) {
			// Apply jet force!
//			vel += JetForce;
			vel += new Vector2(0, (JetTargetYVel-vel.y)/8f);
			// Spend that fuel!
			jetFuelLeft -= Time.deltaTime * JetSpendRate;
			// Are we OUT of fuel?! Stop jetting!
			if (IsFuelEmpty) {
				StopJet();
			}
		}
	}

	// ----------------------------------------------------------------
	//  Input
	// ----------------------------------------------------------------
	override protected void OnButtonJump_Down() {
		if (MayWallKick()) {
			WallKick();
		}
		else if (MayJump()) {
			Jump();
		}
		else if (CanStartJetting()) {
			StartJet();
		}
		else {
			ScheduleDelayedJump();
		}
	}
	override protected void OnButtonJump_Up() {
		if (isJetting) {
			StopJet();
		}
	}
	override protected void OnDown_Down() {
		base.OnDown_Down();
		if (CanStartJetting()) {
			StartJet();
		}
	}


	// ----------------------------------------------------------------
	//  Jet!
	// ----------------------------------------------------------------
	private void StartJet() {
		if (isJetting) { return; } // Already jetting? Do nothing.
		StopWallSlide(); // can't both jet AND wall-slide.
		isJetting = true;
		groundedSinceJet = false;
		isPreservingWallKickVel = false; // When we jet, forget about retaining my wall-kick vel!
		myCocoBody.OnStartJet();
//		GameManagers.Instance.EventManager.OnPlayerStartJet(this);
	}
	private void StopJet() {
		if (!isJetting) { return; } // Not jetting? Do nothing.
		isJetting = false;
		myCocoBody.OnStopJet();
	}
	private void RechargeJet() {
		SetJetFuelLeft(JetFuelCapacity); // fill 'er up regular.
		myCocoBody.OnRechargeJet();
//		GameManagers.Instance.EventManager.OnPlayerRechargeJet(this);
	}

	private void SetJetFuelLeft(float _fuelLeft) {
		jetFuelLeft = _fuelLeft;
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override protected void LandOnCollidable(Collidable collidable) {
		// Landing on a surface stops the jet.
		StopJet();
		groundedSinceJet = true;
		// Is this collidable refreshing? Recharge me!
		if (collidable==null || collidable.DoRechargePlayer) {
			RechargeJet();
		}
		// Base call.
		base.LandOnCollidable(collidable);
	}

//	override public void OnEnterLift() {
//		base.OnEnterLift();
//		if (isJetting) {
//			StopJet();
//		}
//	}


	override public void OnUseBattery() {
		RechargeJet();
	}

}
