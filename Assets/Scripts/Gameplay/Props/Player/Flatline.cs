using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flatline : Player {
	// Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flatline; }
    override public Vector2 Size { get { return new Vector2(1.6f, 1.6f); } }
	override protected float InputScaleX {
        get {
            if (hasHoveredWithoutTouchCollider) { return 0; } // No horz input while hovering (until we touch a collider).
            //if (!feetOnGround()) { return 0.01f; } // In the air? Reduced input scale.
            return 0.018f;
        }
    }
	override protected float FrictionAir { get { return 1; } }
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
	override protected float MaxVelXGround { get { return 0.8f; } }

	override protected float JumpForce { get { return 0; } }
	override protected float WallSlideMinYVel { get { return -999f; } }
    override protected Vector2 WallKickVel { get { return new Vector2(Mathf.Abs(vel.y), 0); } }
    override protected float PostWallKickHorzInputLockDur { get { return 999f; } }
    //override protected float WallKickExtensionWindow { get { return 0.3f; } }
    
    private bool MayStartHover() {
        return !feetOnGround() // FEET touching nothing?
            && !isTouchingWall() // ARMS touching nothing?
            && !IsHoverEmpty; // not out of hover-time?
    }
    public override bool MayUseBattery() {
        if (IsHoverFull) { return false; } // Already recharged? Nah.
        return base.MayUseBattery();
    }
    public bool IsHoverFull { get { return HoverTimeLeft >= HoverDur; } }
    public bool IsHoverEmpty { get { return HoverTimeLeft <= 0; } }

    // Properties
    private const float HoverDur = 2f; // we can only stay hovering for a few seconds.
    private bool isButtonHeld_Hover;
    private bool hasHoveredWithoutTouchCollider; // when this is true, we can't provide input.
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
        StartHover();
    }
    private void StartHover() {
        if (IsHovering) { return; } // Already hovering? Do nothin'.
        IsHovering = true;
        hasHoveredWithoutTouchCollider = true;
        ResetMaxYSinceGround();
        // Convert yVel to xVel, and halt yVel.
        //float xVel = vel.magnitude * DirFacing; // assume we wanna travel in the dir we're facing.
        float xVel = vel.x;
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
        hasHoveredWithoutTouchCollider = false;
        // Tell my body!
        myFlatlineBody.OnRechargeHover();
    }
    
    private bool MayConvertVertVelToHorzFromLand() {
        if (Time.time > timeStoppedWallSlide+0.2f) { return false; } // Nah, been too long since we've wall-slid.
        if (inputAxis.y < -0.8f) { return false; } // Pushing down? Nah, don't convert.
        return true; // Sure, convert!
    }



    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    public override void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);
        if (timeSinceBounce > 0.05f) {//TEST
        hasHoveredWithoutTouchCollider = false;
        }
        StopHover();
    }
    override protected void LandOnCollidable(Collidable collidable) {
        base.LandOnCollidable(collidable);
        // Touching floor recharges hover!
        RechargeHover();
        // Convert vel??
        if (MayConvertVertVelToHorzFromLand()) {
            // Are we VERY CLOSE to a wall, facing away from it, and NOT pushing towards it?? Convert VERT vel into HORZ vel!
            if (myWhiskers.SurfaceDistMin(Sides.L) < 0.4f && DirFacing==1 && inputAxis.x>-0.7f) {
                ConvertVertVelToHorz(1);
            }
            else if (myWhiskers.SurfaceDistMin(Sides.R) < 0.4f && DirFacing==-1 && inputAxis.x<0.7f) {
                ConvertVertVelToHorz(-1);
            }
        }
    }
    protected override void OnFeetLeaveCollidable(Collidable collidable) {
        base.OnFeetLeaveCollidable(collidable);
        // We're not touching ANYthing?!
        if (!myWhiskers.IsTouchingAnySurface()) {
            if (isButtonHeld_Hover) {
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
            // Modify our yVel.
            if (vel.y > -0.2f) { // Moving UP? Convert HORZ vel to VERT vel!
                ConvertHorzVelToVert();
            }
        }
    }
    
    private void ConvertVertVelToHorz(int dir) {
        vel = new Vector2(Mathf.Abs(ppvel.y)*dir, 0);
    }
    private void ConvertHorzVelToVert() {
        int dirY = vel.y>-0.2f ? 1 : -1; // NOT moving down? Convert to yVel UP! Moving DOWN? Convert to yVel DOWN!
        float yVel = Mathf.Abs(ppvel.x) * dirY;
        yVel = Mathf.Max(vel.y, yVel); // if we're already going up fast, keep dat.
        vel = new Vector2(0, yVel);
    }
    
    public override void OnUseBattery() {
        base.OnUseBattery();
        RechargeHover();
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


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Down() {
		if (MayWallKick()) {
			WallKick();
		}
        
        isButtonHeld_Hover = true;
        
        // In the air? Start hover!
        if (MayStartHover()) {
            StartHover();
        }
	}
	override protected void OnButtonJump_Up() {
        isButtonHeld_Hover = false;
        StopHover();
    }


}



