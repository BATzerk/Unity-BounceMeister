using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetta : Player {
	// Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Jetta; }
//	private Vector2 GravityJettingWhileFalling = new Vector2(0, 0.02f); // gravity will actually work WITH us while we're falling! So we halt super fast.
//	override protected Vector2 Gravity {
//		get {
//			if (isJetting && vel.y<0) { return GravityJettingWhileFalling; }
//			return GravityNeutral;
//		}
//	}
	override protected Vector2 Gravity {
		get {
			if (IsJetting) { return Vector2.zero; }
			return base.Gravity;
		}
	}
    //protected override float JumpForce { get { return 0.55f; } }
    // Constants
    private const float JetDuration = 1.9f; // in SECONDS, how long we may jet until recharging.
	public  const float FuelCapacity = 100f; // this number doesn't matter at *all*. Just has to be something.
	private const float FuelSpendRate = FuelCapacity/JetDuration; // how much fuel we spend PER SECOND.
	private const float JetTargetYVel = 0.04f; // TEST
//	private readonly Vector2 JetForce = new Vector2(0, 0.05f);
	// Properties
	public bool IsJetting { get; private set; }
	private bool groundedSinceJet; // TEST for interactions with Batteries.
	public float FuelLeft { get; private set; }
	// References
	private JettaBody myJettaBody;


	// Getters (Public)
	override public bool MayUseBattery() { return !IsFuelFull; }
	public bool IsFuelEmpty { get { return FuelLeft <= 0; } }
	public bool IsFuelFull { get { return FuelLeft >= FuelCapacity; } }
	// Getters (Protected)
	override protected bool MayWallSlide() {
		return base.MayWallSlide() && !IsJetting;
	}
	// Getters (Private)
	private bool CanStartJetting() {
		if (IsGrounded()) { return false; } // I can't jet if I'm on the ground.
//		if (IsInLift) { return false; }
		return !IsFuelEmpty;
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		myJettaBody = myBody as JettaBody;

		SetJetFuelLeft(FuelCapacity);

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
		if (IsJetting) {
			// Apply jet force!
//			vel += JetForce;
			vel += new Vector2(0, (JetTargetYVel-vel.y)/4f);
			// Spend that fuel!
			FuelLeft -= Time.deltaTime * FuelSpendRate;
            myJettaBody.UpdateFillSprite();
			// Are we OUT of fuel?! Stop jetting!
			if (IsFuelEmpty) {
				StopJet();
			}
		}
	}
    override protected void OnHitJumpApex() {
        base.OnHitJumpApex();
        // Pushing up, not jetting and we MAY start? Do!
        if (inputController.IsJump_Held && !IsJetting && CanStartJetting()) {
            StartJet();
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
		else if (CanStartJetting()) {
			StartJet();
		}
		else {
			ScheduleDelayedJump();
		}
	}
	override protected void OnButtonJump_Release() {
		if (IsJetting) {
			StopJet();
		}
	}
	//override protected void OnDown_Down() {
	//	base.OnDown_Down();
	//	if (CanStartJetting()) {
	//		StartJet();
	//	}
	//}


	// ----------------------------------------------------------------
	//  Jet!
	// ----------------------------------------------------------------
	private void StartJet() {
		if (IsJetting) { return; } // Already jetting? Do nothing.
		StopWallSlide(); // can't both jet AND wall-slide.
		IsJetting = true;
		groundedSinceJet = false;
		isPreservingWallKickVel = false; // When we jet, forget about retaining my wall-kick vel!
		myJettaBody.OnStartJet();
//		GameManagers.Instance.EventManager.OnPlayerStartJet(this);
	}
	private void StopJet() {
		if (!IsJetting) { return; } // Not jetting? Do nothing.
		IsJetting = false;
		myJettaBody.OnStopJet();
	}
	private void RechargeJet() {
		SetJetFuelLeft(FuelCapacity); // fill 'er up regular.
		myJettaBody.OnRechargeJet();
//		GameManagers.Instance.EventManager.OnPlayerRechargeJet(this);
	}

	private void SetJetFuelLeft(float _fuelLeft) {
		FuelLeft = _fuelLeft;
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
