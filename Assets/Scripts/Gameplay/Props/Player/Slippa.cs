using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slippa : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Slippa; }
    override protected float InputScaleX { get { return 0.05f; } }
    override protected float FrictionAir { get { return 1; } }
    override protected float FrictionGround {
        get {
            if (Mathf.Abs(LeftStick.x) > 0.1f) { return 0.95f; } // Providing input? Less friction!
            return 0.76f;
        }
    }
    override protected Vector2 Gravity {
        get {
            Vector2 grav = new Vector2(0, -0.04f);
            if (IsAgainstWall()) { grav *= 0.5f; } // On a wall? Reduce gravity!
            if (isReducedJumpGravity) { grav *= 0.7f; } // We're still holding down the jump button? Reduce gravity!
            return grav;
        }
    }
    override protected float MaxVelXAir { get { return 0.5f; } }
    override protected float MaxVelXGround { get { return 0.5f; } }

    override protected float JumpForce { get { return 0.48f; } }
    override protected float WallSlideMinYVel { get { return -0.25f; } }
    override protected Vector2 WallKickVel { get { return new Vector2(0.4f,0.6f); } }
    override protected float PostWallKickHorzInputLockDur { get { return 0.1f; } }
    override protected bool DoesFarFallHop { get { return false; } }

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
    override protected void StartWallSlide(int dir) {
        base.StartWallSlide(dir);
        // Convert our horizontal speed to vertical speed!
        float newYVel = Mathf.Abs(vel.x)*0.5f + Mathf.Max(0, vel.y);
        SetVel(new Vector2(vel.x, newYVel));
    }

    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    //  override public void OnWhiskersTouchCollider(int side, Collider2D col) {
    //      base.OnWhiskersTouchCollider(side, col);
    //
    //      // Touched a side?
    //      if (side==Sides.L || side==Sides.R) {
    //
    //      }
    //  }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        print(Time.frameCount + "  " + vel.y);
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
        else {
            ScheduleDelayedJump();
        }
    }
    override protected void OnButtonJump_Release() {
        isReducedJumpGravity = false; // Not anymore, boss!
    }


}

