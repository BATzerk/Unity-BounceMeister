﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plunga : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Plunga; }
    private readonly Vector2 PlungeForce = new Vector2(0, -0.032f); // applied in addition to Gravity.
    // Properties
    private bool isPlunging = false;
    private bool isPlungeRecharged = true;
    private bool groundedSincePlunge=true; // TEST for interactions with Batteries.
    // References
    private PlungaBody myPlungaBody;
    
    
    override protected void InitMyPhysicsValues() {
        base.InitMyPhysicsValues();
        
        WallKickForce = new Vector2(0.4f, 0.42f);
    }


    // Getters (Public)
    override public bool MayUseBattery() { return !isPlungeRecharged; }
    override protected bool MaySetGroundedRespawnPos() {
        if (isPlunging || !isPlungeRecharged) { return false; } // Plunging? Not safe to set GroundedRespawnPos.
        return base.MaySetGroundedRespawnPos();
    }
    public bool IsPlungeRecharged { get { return isPlungeRecharged; } }
    // Getters (Protected)
//  override public bool IsAffectedByLift() { return !isPlunging; } // We're immune to Lifts while plunging!
    override protected bool MayEatEdibles() {
        return base.MayEatEdibles() && !isPlunging;
    }
    override protected bool MayWallKick() {
        return base.MayWallKick() && !isPlunging;
    }
    override protected bool MayWallSlide() {
        return base.MayWallSlide() && !isPlunging;
    }
    
    override protected bool DoBounceOffColl(int mySide, Collidable coll) {
        if (!MayBounceOffColl(coll)) { return false; }
        if (isPlunging && mySide==myWhiskers.SideFeet) { return true; } // Non-bouncy, BUT I'm plunging and it's my feet? Yes!
        return base.DoBounceOffColl(mySide, coll);
    }
    override protected float ExtraBounceDistToRestore() {
        if (isPlunging) { return 4.0f; } // Give us MORE than we started with!
        return base.ExtraBounceDistToRestore();
    }
    // Getters (Private)
    private bool CanStartPlunge() {
        if (IsGrounded()) { return false; } // I can't plunge if I'm on the ground.
        if (IsInLift) { return false; }
        return isPlungeRecharged;
    }



    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myPlungaBody = myBody as PlungaBody;

        base.Start();
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    override protected void ApplyInternalForces() {
        base.ApplyInternalForces();
        if (isPlunging) {
            ChangeVel(PlungeForce.x, PlungeForce.y*GravFlipDir);
        }
    }
    override protected void UpdateMaxYSinceGround() {
        // TEST!!
        if (!groundedSincePlunge) { return; }
        base.UpdateMaxYSinceGround();
    }

    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    //override protected void OnDown_Down() {
    //  base.OnDown_Down();
    //  if (CanStartPlunge()) {
    //      StartPlunge();
    //  }
    //}
    override protected void OnButtonAction_Press() {
        if (CanStartPlunge()) {
            StartPlunge();
        }
    }


    // ----------------------------------------------------------------
    //  Plunge!
    // ----------------------------------------------------------------
    private void StartPlunge() {
        if (isPlunging) { return; } // Already plunging? Do nothing.
        StopWallSlide(); // can't both plunge AND wall-slide.
        isPlunging = true;
        isPlungeRecharged = false; // spent!
        groundedSincePlunge = false;
        isPreservingWallKickVel = false; // When we plunge, forget about retaining my wall-kick vel!
        myPlungaBody.OnStartPlunge();
        SetVel(new Vector2(vel.x, Mathf.Min(vel.y, 0))); // lose all upward momentum!
        GameManagers.Instance.EventManager.OnPlayerStartPlunge(this);
    }
    private void StopPlunge() {
        if (!isPlunging) { return; } // Not plunging? Do nothing.
        isPlunging = false;
        myPlungaBody.OnStopPlunge();
    }
    private void RechargePlunge() {
        if (isPlungeRecharged) { return; } // Already recharged? Do nothing.
        isPlungeRecharged = true;
        myPlungaBody.OnRechargePlunge();
        GameManagers.Instance.EventManager.OnPlayerRechargePlunge(this);
    }


    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    override protected void BounceOffCollidable_Up(Collidable collidable) {
        // Base call.
        base.BounceOffCollidable_Up(collidable);
        // Bouncing up off a surface stops the plunge.
        StopPlunge();
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

    override public void OnEnterLift(Lift lift) {
        base.OnEnterLift(lift);
        if (isPlunging) {
            StopPlunge();
        }
    }

    override public void OnUseBattery() {
        base.OnUseBattery();
        RechargePlunge();
    }

}
