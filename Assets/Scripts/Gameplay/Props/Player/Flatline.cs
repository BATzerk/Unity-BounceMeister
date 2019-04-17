using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flatline : Player {
	// Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flatline; }
    override public Vector2 Size { get { return new Vector2(1.6f, 1.6f); } }
	override protected float InputScaleX {
        get {
            if (IsHovering || HoverTimeLeft<HoverDur) { return 0; } // No horz input while hovering.
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
            if (isTouchingWall()) { return GravityNeutral * 0.2f; } // On a wall? Reduce gravity!
            if (IsHovering) { return Vector2.zero; } // Hovering? No gravity!
			return GravityNeutral * 1;
		}
	}
	override protected float MaxVelXAir { get { return 99f; } }
	override protected float MaxVelXGround { get { return 0.8f; } }

	override protected float JumpForce { get { return 0; } }
	override protected float WallSlideMinYVel { get { return -999f; } }
    override protected Vector2 WallKickVel { get { return new Vector2(Mathf.Abs(vel.y), 0); } }
    override protected float PostWallKickHorzInputLockDur { get { return 999f; } }
    
    private bool MayStartHover() {
        return !feetOnGround() // FEET touching nothing?
            && !isTouchingWall() // ARMS touching nothing?
            && HoverTimeLeft > 0; // not out of hover-time?
    }
    
    // Properties
    private const float HoverDur = 2f; // we can only stay hovering for a few seconds.
    private bool isButtonHeld_Hover;
    public bool IsHovering { get; private set; }
    public float HoverTimeLeft { get; private set; }
    private float timeWhenLanded; // time when feet last landed on a collider.
    private Vector2 ppvel; // HACKY workaround for getting vel from hitting a wall. ppvel is ACTUALLY how fast we were going before we hit the wall.
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
        // No yVel.
        SetVel(new Vector2(vel.x, 0));
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


	//// ----------------------------------------------------------------
	////  Events
	//// ----------------------------------------------------------------
	//override protected void StartWallSlide(int dir) {
		//base.StartWallSlide(dir);
    //}

    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    public override void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);
        RechargeHover(); // ANY contact recharges our hover!
        StopHover();
    }
    override protected void LandOnCollidable(Collidable collidable) {
        base.LandOnCollidable(collidable);
        timeWhenLanded = Time.time;
        // Are we VERY CLOSE to a wall, and facing away from it?? Convert VERT vel into HORZ vel!
        if (myWhiskers.SurfaceDistMin(Sides.L) < 0.4f && DirFacing==1) {
            ConvertVertVelToHorz(1);
        }
        else if (myWhiskers.SurfaceDistMin(Sides.R) < 0.4f && DirFacing==-1) {
            ConvertVertVelToHorz(-1);
        }
    }
    private void ConvertVertVelToHorz(int dir) {
        float vertSpeed = Mathf.Abs(ppvel.y);
        if (vertSpeed > 0.1f) { // Add small threshold check, so we can get close into a corner if we want to.
            vel = new Vector2(vertSpeed*dir, 0);
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
        if (!isWallSliding() && collidable is BaseGround) {
            int dir = side==Sides.L ? -1 : 1;
            StartWallSlide(dir);
            // Modify our yVel.
            if (vel.y < -0.2f) { // Moving DOWN? Keep going down.
                
            }
            else { // Moving UP? Convert HORZ vel to VERT vel!
                int dirY = vel.y>-0.2f ? 1 : -1; // NOT moving down? Convert to yVel UP! Moving DOWN? Convert to yVel DOWN!
                float yVel = Mathf.Abs(ppvel.x) * dirY;
                yVel = Mathf.Max(vel.y, yVel); // if we're already going up fast, keep dat.
                vel = new Vector2(0, yVel);
            }
        }
    }
    
    
    
    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    protected override void FixedUpdate() {
        ppvel = pvel;
        
        base.FixedUpdate();
        
        UpdateHover();
    }
    private void UpdateHover() {
        if (IsHovering) {
            // 0 yVel!
            SetVel(new Vector2(vel.x, 0));
        
            // End hover?
            HoverTimeLeft -= Time.deltaTime;
            if (HoverTimeLeft <= 0) {
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



