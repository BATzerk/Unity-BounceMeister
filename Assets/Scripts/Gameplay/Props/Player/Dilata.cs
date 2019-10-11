using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dilata : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Dilata; }
    override protected void InitMyPhysicsValues() {
        base.InitMyPhysicsValues();
        
        GravityNeutral = new Vector2(0, -0.042f);
        InputEffectX = 0.05f;
        MaxVelXAir = 0.5f;
        MaxVelXGround = 0.5f;
        JumpForce = 0.5f;
        WallSlideMinYVel = -0.25f;
        WallKickForce = new Vector2(0.4f, 0.6f);
        PostWallKickHorzInputLockDur = 0.1f;
    }
    override protected float FrictionAir() { return 1; }
    override protected float FrictionGround() {
        if (Mathf.Abs(LeftStick.x) > 0.1f) { return 0.95f; } // Providing input? Less friction!
        return 0.76f;
    }
    override protected float GravityScale() {
        if (IsAgainstWall()) { return 0.2f; } // On a wall? Reduce gravity!
        if (isReducedJumpGravity) { return 0.7f; } // We're still holding down the jump button? Reduce gravity!
        return 1.6f;
    }

    // Properties
    public bool IsDilatingTime { get; private set; }
    private bool isReducedJumpGravity; // true when we jump. False when A) We release the jump button, or B) We hit our jump apex.
    // References
    private GameTimeController gameTimeController;


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        gameTimeController = MyRoom.GameController.GameTimeController;
        SetIsDilatingTime(true);
        base.Start();
    }

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
    private void ToggleIsDilatingTime() { SetIsDilatingTime(!IsDilatingTime); }
    private void SetIsDilatingTime(bool val) {
        IsDilatingTime = val;
        gameTimeController.OnDilataSetIsDilatingTime(IsDilatingTime);
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
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonAction_Press() {
        base.OnButtonAction_Press();
        ToggleIsDilatingTime();
    }
    override protected void OnButtonJump_Release() {
        isReducedJumpGravity = false; // Not anymore, boss!
    }
    
    
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    override protected void Update() {
        base.Update();
        if (!DoUpdate()) { return; } // Not supposed to Update? No dice.
        gameTimeController.UpdateFromDilata(this);
    }


}

