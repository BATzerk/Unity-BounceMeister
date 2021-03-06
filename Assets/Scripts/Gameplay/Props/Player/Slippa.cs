﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slippa : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Slippa; }
    override protected float FrictionAir() { return 1; }
    override protected float FrictionGround() {
        if (Mathf.Abs(LeftStick.x) > 0.1f) { return 0.95f; } // Providing input? Less friction!
        return 0.76f;
    }
    override protected float GravityScale() {
        float val = base.GravityScale();
        if (IsAgainstWall()) { val *= 0.5f; } // On a wall? Reduce gravity!
        if (isReducedJumpGravity) { val *= 0.7f; } // We're still holding down the jump button? Reduce gravity!
        return val;
    }
    override protected void InitMyPhysicsValues() {
        base.InitMyPhysicsValues();
        
        GravityNeutral = new Vector2(0, -0.04f);
        InputEffectX = 0.05f;
        MaxVelXAir = 0.5f;
        MaxVelXGround = 0.5f;
        JumpForce = 0.48f;
        WallSlideMinYVel = -0.25f;
        WallKickForce = new Vector2(0.4f, 0.6f);
        PostWallKickHorzInputLockDur = 0.1f;
        DoesFarFallHop = false;
    }

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
        //print(Time.frameCount + "  " + vel.y);
    }


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Release() {
        isReducedJumpGravity = false; // Not anymore, boss!
    }


}

