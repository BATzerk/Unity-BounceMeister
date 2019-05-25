using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetta : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Jetta; }
//  private Vector2 GravityJettingWhileFalling = new Vector2(0, 0.02f); // gravity will actually work WITH us while we're falling! So we halt super fast.
//  override protected Vector2 Gravity {
//      get {
//          if (isJetting && vel.y<0) { return GravityJettingWhileFalling; }
//          return GravityNeutral;
//      }
//  }
    override protected Vector2 Gravity {
        get {
            if (IsJetting) { return Vector2.zero; }
            Vector2 baseGravity = new Vector2(0, -0.024f);
            //if (IsAgainstWall()) { return baseGravity * 0.2f; } // On a wall? Reduce gravity!
            return baseGravity;
        }
    }
    override protected float FrictionAir {
        get {
            return isPreservingWallKickVel ? 1f : 0.94f; // No air friction while we're preserving our precious wall-kick vel.
        }
    }
    override protected float FrictionGround {
        get {
            //if (Mathf.Abs(LeftStick.x) > 0.1f) { return 0.7f; } // Providing input? Less friction!
            return 0.82f; // No input? Basically halt.
        }
    }
    override protected float InputScaleX { get { return 0.08f; } }
    override protected float WallSlideMinYVel { get { return -0.28f; } }
    override protected float JumpForce { get { return 0.46f; } }
    override protected Vector2 WallKickVel { get { return new Vector2(0.32f,0.4f); } }
    override protected float HorzMoveInputVelXDelta() {
        float val = base.HorzMoveInputVelXDelta();
        return IsGrounded() ? val : val*0.2f; // less (finer!) control in air.
    }
    // Constants
    private const float JetDuration = 1.9f; // in SECONDS, how long we may jet until recharging.
    public  const float FuelCapacity = 100f; // this number doesn't matter at *all*. Just has to be something.
    private const float FuelSpendRate = FuelCapacity/JetDuration; // how much fuel we spend PER SECOND.
    private const float JetTargetYVel = 0;//.052f;
//  private readonly Vector2 JetForce = new Vector2(0, 0.05f);
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
//      if (IsInLift) { return false; }
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
//          vel += JetForce;
            vel += new Vector2(0, (JetTargetYVel-vel.y)/4f);
            // Spend that fuel!
            FuelLeft -= Time.deltaTime * FuelSpendRate;
            //myJettaBody.UpdateFillSprite();
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


    // ----------------------------------------------------------------
    //  Jet!
    // ----------------------------------------------------------------
    private void StartJet() {
        if (IsJetting) { return; } // Already jetting? Do nothing.
        StopWallSlide(); // can't both jet AND wall-slide.
        IsJetting = true;
        groundedSinceJet = false;
        //isPreservingWallKickVel = false; // When we jet, forget about retaining my wall-kick vel!
        myJettaBody.OnStartJet();
    }
    private void StopJet() {
        if (!IsJetting) { return; } // Not jetting? Do nothing.
        IsJetting = false;
        isPreservingWallKickVel = false; // When we stop jet, forget about retaining my wall-kick vel!
        myJettaBody.OnStopJet();
    }
    private void RechargeJet() {
        SetJetFuelLeft(FuelCapacity); // fill 'er up regular.
        myJettaBody.OnRechargeJet();
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

//  override public void OnEnterLift() {
//      base.OnEnterLift();
//      if (isJetting) {
//          StopJet();
//      }
//  }


    override public void OnUseBattery() {
        RechargeJet();
    }

}

/*
// CORKA TEST.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetta : Player {
	// Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Jetta; }
	//override protected Vector2 Gravity {
	//	get {
	//		if (IsJetting) { return Vector2.zero; }
	//		return base.Gravity;
	//	}
	//}
    // Constants
    private const float JetDuration = 1.7f; // in SECONDS, how long we may jet until recharging.
	public  const float FuelCapacity = 100f; // this number doesn't matter at *all*. Just has to be something.
	private const float FuelSpendRate = FuelCapacity/JetDuration; // how much fuel we spend PER SECOND.
    //private float JetTargetYVel = 0.052f;
//  private readonly Vector2 JetForce = new Vector2(0, 0.05f);
    // Properties
    private float JetForceY;
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
			vel += new Vector2(0, JetForceY);
			//vel += new Vector2(0, (JetTargetYVel-vel.y)/8f);
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


	// ----------------------------------------------------------------
	//  Jet!
	// ----------------------------------------------------------------
	private void StartJet() {
		if (IsJetting) { return; } // Already jetting? Do nothing.
		StopWallSlide(); // can't both jet AND wall-slide.
		IsJetting = true;
		groundedSinceJet = false;
        JetForceY = valA + Mathf.Max(0, -vel.y*valB);
        //JetForceY = -Gravity.y*1.7f;
		myJettaBody.OnStartJet();
	}
    public float valA = 0.03f;
    public float valB = 0.05f;
	private void StopJet() {
		if (!IsJetting) { return; } // Not jetting? Do nothing.
		IsJetting = false;
        isPreservingWallKickVel = false; // When we stop jet, forget about retaining my wall-kick vel!
		myJettaBody.OnStopJet();
	}
	private void RechargeJet() {
		SetJetFuelLeft(FuelCapacity); // fill 'er up regular.
		myJettaBody.OnRechargeJet();
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

	override public void OnUseBattery() {
		RechargeJet();
	}

}
*/
