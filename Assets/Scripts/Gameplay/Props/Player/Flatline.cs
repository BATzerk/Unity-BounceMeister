using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flatline : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flatline; }
    override public Vector2 Size { get { return new Vector2(1.6f, 1.6f); } }
    override protected float InputScaleX {
        get {
            if (IsHovering) { return 0; }// || IsHoverEmpty
            //if (HasHoveredWithoutTouchCollider) { return 0; } // No horz input while hovering (until we touch a collider). TEST DISABLED.
            if (!IsGrounded() && !IsInLift) { return 0.009f; } // In the air and NOT a Lift? Reduced input scale.
            return 0.018f;
        }
    }
    override protected float FrictionAir {
        get {
            //if (IsInLift && Mathf.Abs(inputAxis.x) < 0.1f && !isButtonHeld_Hover) { // In Lift and NOT providing input? Friction!
            //    return 0.76f;
            //}
            return 1;
        }
    }
    override protected float FrictionGround {
        get {
            if (Mathf.Abs(inputAxis.x) > 0.1f // Providing input?
             || Time.time < timeWhenLanded+0.25f // Recently landed?
            ) {
                return 0.98f; // Less friction!
            }
            return 0.76f; // Normal friction.
        }
    }
    override protected Vector2 Gravity {
        get {
            Vector2 gravNeutral = new Vector2(0, -0.042f);
            if (IsAgainstWall()) { return gravNeutral * 0.2f; } // On a wall? Reduce gravity!
            if (IsHovering) { return Vector2.zero; } // Hovering? No gravity!
            return gravNeutral;
        }
    }
    override protected float MaxVelXFromInput { get { return 0.8f; } }
    override protected float MaxVelXAir { get { return 99f; } }
    override protected float MaxVelXGround { get { return 2f; } }

    override protected float JumpForce { get { return 0; } }
    override protected float WallSlideMinYVel { get { return -999f; } }
    override protected Vector2 WallKickVel { get { return new Vector2(Mathf.Abs(vel.y), 0); } }
    override protected float PostWallKickHorzInputLockDur { get { return -1; } }
    override protected float WallKickExtensionWindow { get { return 0.25f; } }
    override protected bool DoesFarFallHop { get { return false; } }
    
    private bool MayStartHover() {
        return !IsHovering // I'm not ALREADY hovering?
            && Time.frameCount > FrameCountWhenBorn+3 // Don't allow hovering for the first few frames of my life.
            && !IsGrounded() // FEET touching nothing?
            //&& !isTouchingWall() // ARMS touching nothing?
            && !IsInLift // NOT in a Lift?
            && !IsHoverEmpty; // not out of hover-time?
    }
    private bool MayConvertVertVelToHorzFromLand() {
        if (Time.time > timeStoppedWallSlide+0.2f) { return false; } // Nah, been too long since we've wall-slid.
        if (IsInput_D()) { return false; } // Pushing down? Nah, don't convert.
        return true; // Sure, convert!
    }
    public override bool MayUseBattery() {
        if (IsHoverFull) { return false; } // Already recharged? Nah.
        return base.MayUseBattery();
    }
    protected override bool MayWallKick() {
        if (myWhiskers.DistToSurface(Sides.B) < 2.5f && vel.y<-0.4f) { // About to land on Ground? Don't allow wall-kick! We prob wanna convert our vel instead and pushed away too early.
            return false;
        }
        return base.MayWallKick();
    }
    protected override bool DoBounceOffCollidable(int mySide, Collidable collidable) {
        if (mySide == Sides.T && !IsHovering) { return true; } // My head's ALWAYS bouncy when I'm NOT hovering.
        return base.DoBounceOffCollidable(mySide, collidable);
    }
    public bool IsHoverFull { get { return HoverTimeLeft >= HoverDur; } }
    public bool IsHoverEmpty { get { return HoverTimeLeft <= 0; } }
    private bool IsInputToHover() {
        return (vel.x<0 && inputAxis.x<-0.1f) // moving left and pushing left?
            || (vel.x>0 && inputAxis.x> 0.1f); // moving right and pushing right?
    }

    // Properties
    private const float HoverDur = 2f; // we can only stay hovering for a few seconds.
    private bool isButtonHeld_Hover;
    public bool HasHoveredWithoutTouchCollider { get; private set; } // when this is true, we can't provide input.
    public bool IsHovering { get; private set; }
    public float HoverTimeLeft { get; private set; }
    // References
    private FlatlineBody myFlatlineBody;


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myFlatlineBody = myBody as FlatlineBody;
        HoverTimeLeft = HoverDur; // start out with fuel.
        base.Start();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    override protected void Jump() { } // Flatline can't jump.
    override protected void WallKick() {
        base.WallKick();
        if (MayStartHover()) {
            StartHover();
        }
    }
    private void StartHover() {
        IsHovering = true;
        HasHoveredWithoutTouchCollider = true;
        ResetMaxYSinceGround();
        // Convert yVel to xVel, and halt yVel.
        //float xVel = vel.magnitude * DirFacing; // assume we wanna travel in the dir we're facing.
        float xVel = vel.x;
        //float xVel = Mathf.Abs(vel.y) * DirFacing;//TEST
        SetVel(new Vector2(xVel, 0));
        // Tell my body!
        myFlatlineBody.OnStartHover();
        // Dispatch event!
        GameManagers.Instance.EventManager.OnPlayerStartHover(this);
    }
    private void StopHover() {
        if (!IsHovering) { return; } // Already not hovering? Do nothin'.
        IsHovering = false;
        // Tell my body!
        myFlatlineBody.OnStopHover();
    }
    private void RechargeHover() {
        HoverTimeLeft = HoverDur;
        // Tell my body!
        myFlatlineBody.OnRechargeHover();
    }



    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    public override void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);
        //if (timeSinceBounce > 0.05f) {//TEST
        HasHoveredWithoutTouchCollider = false;
        //}
        if (side != Sides.T) { // Touched arms or feet? Stop hover. (Our head getting touched shouldn't stop us from hovering.)
            StopHover();
        }
    }
    override protected void LandOnCollidable(Collidable collidable) {
        base.LandOnCollidable(collidable);
        // Touching floor recharges hover!
        RechargeHover();
        // Convert vel??
        if (MayConvertVertVelToHorzFromLand()) {
            // Are we VERY CLOSE to a wall, facing away from it, and NOT pushing towards it?? Convert VERT vel into HORZ vel!
            if (myWhiskers.DistToSurface(Sides.L) < 0.4f && DirFacing==1) {// && !IsInput_L()) {
                ConvertVelYToX(1);
            }
            else if (myWhiskers.DistToSurface(Sides.R) < 0.4f && DirFacing==-1) {// && !IsInput_R()) {
                ConvertVelYToX(-1);
            }
        }
    }
    protected override void OnFeetLeaveCollidable(Collidable collidable) {
        base.OnFeetLeaveCollidable(collidable);
        // User wants to hover??
        if (IsInputToHover()) {
            // We MAY hover, AND we're not touching ANYthing (we don't wanna hover if just left ground to slide up wall)...
            if (MayStartHover() && !myWhiskers.IsTouchingAnySurface()) {
                StartHover();
            }
        }
    }
    override protected void OnArmTouchCollidable(int side, Collidable collidable) {
        base.OnArmTouchCollidable(side, collidable);
        
        // It's a Ground??
        if (timeSinceBounce>0.1f && !isWallSliding() && collidable is BaseGround) {
            int dir = side==Sides.L ? -1 : 1;
            StartWallSlide(dir);
            // Moving SIDEWAYS a little? Convert HORZ vel to VERT vel!
            if (Mathf.Abs(ppvel.x) > 0.1f) {
                ConvertVelXToY();
            }
        }
    }
    
    private void ConvertVelYToX(int dir) {
        vel = new Vector2(Mathf.Abs(ppvel.y)*dir, 0);
    }
    private void ConvertVelXToY() {
        int dirY = vel.y>-0.2f ? 1 : -1; // NOT moving down? Convert to yVel UP! Moving DOWN? Convert to yVel DOWN!
        float yVel = Mathf.Abs(ppvel.x) * dirY;
        // Don't *lose* speed if our yVel is faster than what we'd make it.
        yVel = dirY>0 ? Mathf.Max(vel.y, yVel) : Mathf.Min(vel.y, yVel);
        vel = new Vector2(0, yVel);
    }
    
    public override void OnUseBattery() {
        base.OnUseBattery();
        RechargeHover();
    }

    protected override void OnStartIsInLift() {
        base.OnStartIsInLift();
        StopHover();
    }
    protected override void OnEndIsInLift() {
        base.OnEndIsInLift();
        if (IsInputToHover() && MayStartHover()) {
            StartHover();
        }
    }

    //override protected void DropThruPlatform() {
    //    base.DropThruPlatform();
    //    SetVel(new Vector2(0, vel.y)); // halt x-vel.
    //}



    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    protected override void FixedUpdate() {
        if (InputController.Instance.IsLPush) { OnLPush(); }
        if (InputController.Instance.IsRPush) { OnRPush(); }
        if (InputController.Instance.IsLRelease) { OnLRelease(); }
        if (InputController.Instance.IsRRelease) { OnRRelease(); }
        
        base.FixedUpdate();
        
        UpdateHover();
        ApplyWallSuck();
    }
    private void UpdateHover() {
        if (IsHovering) {
            // 0 yVel!
            SetVel(new Vector2(vel.x, 0));
        
            // End hover?
            HoverTimeLeft -= Time.deltaTime;
            if (IsHoverEmpty) {
                StopHover();
            }
        }
    }
    private void ApplyWallSuck() {
        // Arm's on wall? Do nothin'.
        if (myWhiskers.IsTouchingAnySurface() || IsHovering) { return; }
        // Are we close to a wall??
        const float SuckDist = 0.4f;
        if (myWhiskers.DistToSurface(Sides.L) < SuckDist && !IsInput_R()) {
            SetVel(vel + new Vector2(-0.1f, 0));
            //pos += new Vector2(-0.1f, 0);
        }
        else if (myWhiskers.DistToSurface(Sides.R) < SuckDist && !IsInput_L()) {
            SetVel(vel + new Vector2( 0.1f, 0));
            //pos += new Vector2( 0.1f, 0);
        }
    }



    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Press() { }
    
    private void OnLPush() {
        if (wallSlideDir == 1 && MayWallKick()) {
            WallKick();
        }
        else if (IsInputToHover() && MayStartHover()) {
            StartHover();
        }
    }
    private void OnRPush() {
        if (wallSlideDir == -1 && MayWallKick()) {
            WallKick();
        }
        else if (IsInputToHover() && MayStartHover()) {
            StartHover();
        }
    }
    private void OnLRelease() {
        if (IsHovering && vel.x < 0) {
            StopHover();
        }
    }
    private void OnRRelease() {
        if (IsHovering && vel.x > 0) {
            StopHover();
        }
    }
    


}





/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flatline : Player {
	// Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flatline; }
    override public Vector2 Size { get { return new Vector2(1.6f, 1.6f); } }
	override protected float InputScaleX {
        get {
            if (IsHovering) { return 0; }// || IsHoverEmpty
            //if (HasHoveredWithoutTouchCollider) { return 0; } // No horz input while hovering (until we touch a collider). TEST DISABLED.
            if (!feetOnGround() && !IsInLift) { return 0.009f; } // In the air and NOT a Lift? Reduced input scale.
            return 0.018f;
        }
    }
	override protected float FrictionAir {
        get {
            //if (IsInLift && Mathf.Abs(inputAxis.x) < 0.1f && !isButtonHeld_Hover) { // In Lift and NOT providing input? Friction!
            //    return 0.76f;
            //}
            return 1;
        }
    }
	override protected float FrictionGround {
		get {
			if (Mathf.Abs(inputAxis.x) > 0.1f // Providing input?
             || Time.time < timeWhenLanded+0.25f // Recently landed?
            ) {
                return 0.98f; // Less friction!
            }
			return 0.76f; // Normal friction.
		}
	}
	override protected Vector2 Gravity {
		get {
            Vector2 gravNeutral = new Vector2(0, -0.042f);
            if (isTouchingWall()) { return gravNeutral * 0.2f; } // On a wall? Reduce gravity!
            if (IsHovering) { return Vector2.zero; } // Hovering? No gravity!
			return gravNeutral;
		}
	}
    override protected float MaxVelXFromInput { get { return 0.8f; } }
	override protected float MaxVelXAir { get { return 99f; } }
	override protected float MaxVelXGround { get { return 2f; } }

    override protected float JumpForce { get { return 0; } }
    override protected float WallSlideMinYVel { get { return -999f; } }
    override protected Vector2 WallKickVel { get { return new Vector2(Mathf.Abs(vel.y), 0); } }
    override protected float PostWallKickHorzInputLockDur { get { return -1; } }
    override protected float WallKickExtensionWindow { get { return 0.25f; } }
    override protected bool DoesFarFallHop { get { return false; } }
    
    private bool MayStartHover() {
        return !IsHovering // I'm not ALREADY hovering?
            && Time.frameCount > FrameCountWhenBorn+3 // Don't allow hovering for the first few frames of my life.
            && !feetOnGround() // FEET touching nothing?
            //&& !isTouchingWall() // ARMS touching nothing?
            && !IsInLift // NOT in a Lift?
            && !IsHoverEmpty; // not out of hover-time?
    }
    private bool MayConvertVertVelToHorzFromLand() {
        if (Time.time > timeStoppedWallSlide+0.2f) { return false; } // Nah, been too long since we've wall-slid.
        if (IsInput_D()) { return false; } // Pushing down? Nah, don't convert.
        return true; // Sure, convert!
    }
    public override bool MayUseBattery() {
        if (IsHoverFull) { return false; } // Already recharged? Nah.
        return base.MayUseBattery();
    }
    protected override bool DoBounceOffCollidable(int mySide, Collidable collidable) {
        if (mySide == Sides.T && !IsHovering) { return true; } // My head's ALWAYS bouncy when I'm NOT hovering.
        return base.DoBounceOffCollidable(mySide, collidable);
    }
    public bool IsHoverFull { get { return HoverTimeLeft >= HoverDur; } }
    public bool IsHoverEmpty { get { return HoverTimeLeft <= 0; } }

    // Properties
    private const float HoverDur = 2f; // we can only stay hovering for a few seconds.
    private bool isButtonHeld_Hover;
    public bool HasHoveredWithoutTouchCollider { get; private set; } // when this is true, we can't provide input.
    public bool IsHovering { get; private set; }
    public float HoverTimeLeft { get; private set; }
    // References
    private FlatlineBody myFlatlineBody;


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myFlatlineBody = myBody as FlatlineBody;
        HoverTimeLeft = HoverDur; // start out with fuel.
        base.Start();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    override protected void Jump() { } // Flatline can't jump.
	override protected void WallKick() {
		base.WallKick();
        if (MayStartHover()) {
            StartHover();
        }
    }
    private void StartHover() {
        IsHovering = true;
        HasHoveredWithoutTouchCollider = true;
        ResetMaxYSinceGround();
        // Convert yVel to xVel, and halt yVel.
        //float xVel = vel.magnitude * DirFacing; // assume we wanna travel in the dir we're facing.
        float xVel = vel.x;
        //float xVel = Mathf.Abs(vel.y) * DirFacing;//TEST
        SetVel(new Vector2(xVel, 0));
        // Tell my body!
        myFlatlineBody.OnStartHover();
        // Dispatch event!
        GameManagers.Instance.EventManager.OnPlayerStartHover(this);
    }
    private void StopHover() {
        if (!IsHovering) { return; } // Already not hovering? Do nothin'.
        IsHovering = false;
        // Tell my body!
        myFlatlineBody.OnStopHover();
    }
    private void RechargeHover() {
        HoverTimeLeft = HoverDur;
        // Tell my body!
        myFlatlineBody.OnRechargeHover();
    }



    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    public override void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);
        //if (timeSinceBounce > 0.05f) {//TEST
        HasHoveredWithoutTouchCollider = false;
        //}
        if (side != Sides.T) { // Touched arms or feet? Stop hover. (Our head getting touched shouldn't stop us from hovering.)
            StopHover();
        }
    }
    override protected void LandOnCollidable(Collidable collidable) {
        base.LandOnCollidable(collidable);
        // Touching floor recharges hover!
        RechargeHover();
        // Convert vel??
        if (MayConvertVertVelToHorzFromLand()) {
            // Are we VERY CLOSE to a wall, facing away from it, and NOT pushing towards it?? Convert VERT vel into HORZ vel!
            if (myWhiskers.DistToSurface(Sides.L) < 0.4f && DirFacing==1) {// && !IsInput_L()) {
                ConvertVelYToX(1);
            }
            else if (myWhiskers.DistToSurface(Sides.R) < 0.4f && DirFacing==-1) {// && !IsInput_R()) {
                ConvertVelYToX(-1);
            }
        }
    }
    protected override void OnFeetLeaveCollidable(Collidable collidable) {
        base.OnFeetLeaveCollidable(collidable);
        // We wanna start to hover, and MAY?
        if (isButtonHeld_Hover && MayStartHover()) {
            if (!myWhiskers.IsTouchingAnySurface()) { // Make sure we're not touching ANYthing (we don't wanna hover if just left ground to slide up wall).
                StartHover();
            }
        }
    }
    override protected void OnArmTouchCollidable(int side, Collidable collidable) {
        base.OnArmTouchCollidable(side, collidable);
        
        // It's a Ground??
        if (timeSinceBounce>0.1f && !isWallSliding() && collidable is BaseGround) {
            int dir = side==Sides.L ? -1 : 1;
            StartWallSlide(dir);
            // Moving SIDEWAYS a little? Convert HORZ vel to VERT vel!
            if (Mathf.Abs(ppvel.x) > 0.1f) {
                ConvertVelXToY();
            }
        }
    }
    
    private void ConvertVelYToX(int dir) {
        vel = new Vector2(Mathf.Abs(ppvel.y)*dir, 0);
    }
    private void ConvertVelXToY() {
        int dirY = vel.y>-0.2f ? 1 : -1; // NOT moving down? Convert to yVel UP! Moving DOWN? Convert to yVel DOWN!
        float yVel = Mathf.Abs(ppvel.x) * dirY;
        // Don't *lose* speed if our yVel is faster than what we'd make it.
        yVel = dirY>0 ? Mathf.Max(vel.y, yVel) : Mathf.Min(vel.y, yVel);
        vel = new Vector2(0, yVel);
    }
    
    public override void OnUseBattery() {
        base.OnUseBattery();
        RechargeHover();
    }

    protected override void OnStartIsInLift() {
        base.OnStartIsInLift();
        StopHover();
    }
    protected override void OnEndIsInLift() {
        base.OnEndIsInLift();
        if (isButtonHeld_Hover && MayStartHover()) {
            StartHover();
        }
    }

    //override protected void DropThruPlatform() {
    //    base.DropThruPlatform();
    //    SetVel(new Vector2(0, vel.y)); // halt x-vel.
    //}



    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    protected override void FixedUpdate() {
        base.FixedUpdate();
        
        UpdateHover();
        ApplyWallSuck();
    }
    private void UpdateHover() {
        if (IsHovering) {
            // 0 yVel!
            SetVel(new Vector2(vel.x, 0));
        
            // End hover?
            HoverTimeLeft -= Time.deltaTime;
            if (IsHoverEmpty) {
                StopHover();
            }
        }
    }
    private void ApplyWallSuck() {
        // Arm's on wall? Do nothin'.
        if (myWhiskers.IsTouchingAnySurface() || IsHovering) { return; }
        // Are we close to a wall??
        const float SuckDist = 0.4f;
        if (myWhiskers.DistToSurface(Sides.L) < SuckDist && !IsInput_R()) {
            SetVel(vel + new Vector2(-0.1f, 0));
            //pos += new Vector2(-0.1f, 0);
        }
        else if (myWhiskers.DistToSurface(Sides.R) < SuckDist && !IsInput_L()) {
            SetVel(vel + new Vector2( 0.1f, 0));
            //pos += new Vector2( 0.1f, 0);
        }
    }
    


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Press() {
		if (MayWallKick()) {
			WallKick();
		}
        
        isButtonHeld_Hover = true;
        
        // In the air? Start hover!
        if (MayStartHover()) {
            StartHover();
        }
	}
	override protected void OnButtonJump_Release() {
        isButtonHeld_Hover = false;
        StopHover();
    }


}

*/

