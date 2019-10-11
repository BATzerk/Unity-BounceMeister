using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flippa : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flippa; }
    //override protected Vector2 Gravity { get { return base.Gravity * FlipDir; } }TODO: Pop all this stuffo into Player.
    //override protected float JumpForce { get { return base.JumpForce * FlipDir; } }
    //override public bool IsGrounded() {
    //    return myWhiskers.OnSurface(FlipDir<0 ? Sides.T : Sides.B);
    //}
    //override protected float WallSlideMinYVel { get { return FlipDir<0 ? Mathf.NegativeInfinity : -0.11f; } }
    //override protected float WallSlideMaxYVel { get { return FlipDir<0 ? 0.11f : Mathf.Infinity; } }
    //override protected Vector2 WallKickForce { get { return new Vector2(0.35f, 0.46f*FlipDir); } }
    //protected override Vector2 VelForWallKick() {
    //    if (FlipDir < 0) {
    //        return new Vector2(-myWhiskers.DirLastTouchedWall*WallKickForce.x, Mathf.Min(vel.y, WallKickForce.y));
    //    }
    //    return base.VelForWallKick();
    //}
    override public bool MayUseBattery() { return !isFlipRecharged; }
    //private readonly Vector2 HitByEnemyVel = new Vector2(0.5f, 0.5f);
    // Properties
    private bool isFlipRecharged;
    // References
    private FlippaBody myFlippaBody;

    // Getters
    private bool MayFlipGravity() {
        return isFlipRecharged;//IsGrounded() && 
    }



    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myFlippaBody = myBody as FlippaBody;

        base.Start();
    }


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    //override protected void OnButtonJump_Press() {
    //    if (MayWallKick()) {
    //        WallKick();
    //    }
    //    else if (MayFlipGravity()) {
    //        FlipGravity();
    //    }
    //}
    override protected void OnButtonAction_Press() {
        if (MayFlipGravity()) {
            FlipGravity();
        }
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    override protected void LandOnCollidable(Collidable collidable) {
        // Is this collidable refreshing? Recharge my plunge!
        if (collidable==null || collidable.DoRechargePlayer) {
            RechargeFlip();
        }
        // Base call.
        base.LandOnCollidable(collidable);
    }
    override public void OnUseBattery() {
        base.OnUseBattery();
        RechargeFlip();
    }


    // ----------------------------------------------------------------
    //  Flipping!
    // ----------------------------------------------------------------
    override public void FlipGravity() {
        base.FlipGravity();
        isFlipRecharged = false; // spent!
        myFlippaBody.OnFlipGravity();
    }
    
    private void RechargeFlip() {
        if (isFlipRecharged) { return; } // Already recharged? Do nothing.
        isFlipRecharged = true;
        myFlippaBody.OnRechargeFlip();
        //GameManagers.Instance.EventManager.OnPlayerRechargePlunge(this);
    }
    
    

}
