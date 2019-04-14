using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flatline : Player {
	// Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flatline; }
    override public Vector2 Size { get { return new Vector2(1.6f, 1.6f); } }
	override protected float InputScaleX { get { return IsSuspended ? 0 : 0.018f; } } // No horz input while suspended.
	override protected float FrictionAir { get { return 1; } }
	override protected float FrictionGround {
		get {
			if (Mathf.Abs(inputAxis.x) > 0.1f) { return 0.98f; } // Providing input? Less friction!
			return 0.76f;
		}
	}
	override protected Vector2 Gravity {
		get {
            if (isTouchingWall()) { return GravityNeutral * 0.2f; } // On a wall? Reduce gravity!
            if (IsSuspended) { return Vector2.zero; } // Suspended? No gravity!
			return GravityNeutral * 1f;
		}
	}
	override protected float MaxVelXAir { get { return 0.5f; } }
	override protected float MaxVelXGround { get { return 0.8f; } }

	override protected float JumpForce { get { return 0; } }
	override protected float WallSlideMinYVel { get { return -0.75f; } }
    override protected Vector2 WallKickVel { get { return new Vector2(Mathf.Abs(vel.y), 0); } }
    override protected float PostWallKickHorzInputLockDur { get { return 999f; } }
    
    // Properties
    private const float SuspensionDur = 2f; // we can only stay suspended for a few seconds.
    private bool isButtonHeld_Suspension;
    public bool IsSuspended { get; private set; }
    private float timeWhenEndSuspension;
    private Vector2 ppvel; // HACKY workaround for getting vel from hitting a wall. ppvel is ACTUALLY how fast we were going before we hit the wall.
    // References
    private FlatlineBody myFlatlineBody;


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myFlatlineBody = myBody as FlatlineBody;
        base.Start();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    override protected void Jump() { } // Flatline can't jump.
	override protected void WallKick() {
		base.WallKick();
        StartSuspension();
    }
    private void StartSuspension() {
        if (IsSuspended) { return; } // Already suspended? Do nothin'.
        IsSuspended = true;
        timeWhenEndSuspension = Time.time + SuspensionDur;
        // No yVel.
        SetVel(new Vector2(vel.x, 0));
        // Tell my body!
        myFlatlineBody.OnStartSuspension();
    }
    private void StopSuspension() {
        if (!IsSuspended) { return; } // Already not suspended? Do nothin'.
        IsSuspended = false;
        // Tell my body!
        myFlatlineBody.OnStopSuspension();
    }


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	override protected void StartWallSlide(int dir) {
		base.StartWallSlide(dir);
        // Lose any negative yVel.
        SetVel(new Vector2(vel.x, Mathf.Max(0, vel.y)));
    }

    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    public override void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);
        StopSuspension();
    }
    protected override void OnFeetLeaveCollidable(Collidable collidable) {
        base.OnFeetLeaveCollidable(collidable);
        // We're not touching ANYthing?!
        if (!myWhiskers.IsTouchingAnySurface()) {
            if (isButtonHeld_Suspension) {
                StartSuspension();
            }
        }
    }
    override protected void OnArmTouchCollidable(int side, Collidable collidable) {
        base.OnArmTouchCollidable(side, collidable);
        // It's a Ground??
        if (!isWallSliding() && collidable is BaseGround) {
            int dir = side==Sides.L ? -1 : 1;
            StartWallSlide(dir);
            // Immediately start going UP!
            vel = new Vector2(0, Mathf.Abs(ppvel.x));
        }
    }
    
    
    
    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    protected override void FixedUpdate() {
        ppvel = pvel;
        
        base.FixedUpdate();
        
        // End suspension?
        if (Time.time >= timeWhenEndSuspension) {
            StopSuspension();
        }
        
        // Suspended? 0 yVel!
        if (IsSuspended) {
            SetVel(new Vector2(vel.x, 0));
        }
    }


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Down() {
		if (MayWallKick()) {
			WallKick();
		}
        
        isButtonHeld_Suspension = true;
        
        // In the air? Start suspension!
        if (!feetOnGround() && !isTouchingWall()) {
            StartSuspension();
        }
	}
	override protected void OnButtonJump_Up() {
        isButtonHeld_Suspension = false;
        StopSuspension();
    }


}



