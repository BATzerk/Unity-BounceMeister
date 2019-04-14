using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flatline : Player {
	// Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flatline; }
    override public Vector2 Size { get { return new Vector2(1.6f, 1.6f); } }
	override protected float InputScaleX { get { return 0.018f; } }
	override protected float FrictionAir { get { return 1; } }
	override protected float FrictionGround {
		get {
			if (Mathf.Abs(inputAxis.x) > 0.1f) { return 1f; } // Providing input? Less friction!
			return 0.76f;
		}
	}
	override protected Vector2 Gravity {
		get {
            if (isTouchingWall()) { return GravityNeutral * 0.2f; } // On a wall? Reduce gravity!
            if (isPreservingWallKickVel) { return Vector2.zero; } // coming off of a wall? NO gravity!
            //if (isReducedJumpGravity) { return GravityNeutral * 0.7f; } // We're still holding down the jump button? Reduce gravity!
			return GravityNeutral * 1f;//0.4f;
		}
	}
	override protected float MaxVelXAir { get { return 0.5f; } }
	override protected float MaxVelXGround { get { return 1f; } }

	override protected float JumpForce { get { return 0;}}//.15f; } }
	override protected float WallSlideMinYVel { get { return -0.25f; } }
    //override protected Vector2 WallKickVel { get { return new Vector2(0.4f,0.6f); } }
    override protected Vector2 WallKickVel { get { return new Vector2(0.4f, 0); } }
    override protected float PostWallKickHorzInputLockDur { get { return 999f; } }

	// Properties
	private bool isReducedJumpGravity; // NOT USED currently. // true when we jump. False when A) We release the jump button, or B) We hit our jump apex.


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
        // No yVel.
        SetVel(new Vector2(vel.x, 0.02f));//HACK testing controls fudge.
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	override protected void OnHitJumpApex() {
		isReducedJumpGravity = false; // the moment we start descending, stop giving us reduced gravity.
	}
	override protected void StartWallSlide(int side) {
		base.StartWallSlide(side);
		//// Convert our horizontal speed to vertical speed!
		//float newYVel = Mathf.Abs(vel.x)*0.5f + Mathf.Max(0, vel.y);
		//SetVel(new Vector2(vel.x, newYVel));
        // Lose any negative yVel.
        SetVel(new Vector2(vel.x, Mathf.Max(0, vel.y)));
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
    override protected void OnArmTouchCollidable(Collidable collidable) {
        base.OnArmTouchCollidable(collidable);
        // It's a Ground??
        if (collidable is BaseGround) {
            OnArmTouchBaseGround();
        }
    }
    
    private void OnArmTouchBaseGround() {
        // Immediately start going UP!
        // HACKY use silly ppvel value 'cause it's ACTUALLY how fast we were going before we hit the wall.
        //Debug.Log("vel " + vel.x + ", " + vel.y + "         pvel: " + pvel.x+","+pvel.y + "     ppvel: " + ppvel.x+", " + ppvel.y);
        vel = new Vector2(0, Mathf.Abs(ppvel.x)*1.0f);
    }
    
    
    private Vector2 ppvel; // HACKY workaround for getting vel from hitting a wall.
    protected override void FixedUpdate() {
        ppvel = pvel;
        base.FixedUpdate();
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
		else {
			ScheduleDelayedJump();
		}
        
        isPreservingWallKickVel = true;//HACK Using incorrectly here. Just to suspend gravity.
        // Lose any negative yVel.
        SetVel(new Vector2(vel.x, Mathf.Max(0, vel.y)));
        
	}
	override protected void OnButtonJump_Up() {
		isReducedJumpGravity = false; // Not anymore, boss!
        isPreservingWallKickVel = false;
	}


}



