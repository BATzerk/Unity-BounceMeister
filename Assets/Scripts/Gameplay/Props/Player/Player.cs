using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Player : PlatformCharacter {
    // Overrides
    override public bool DoSaveInRoomFile() { return false; }
    abstract public PlayerTypes PlayerType();
    // Physics Values
    protected float InputEffectX = 0.1f;
    protected float JumpForce = 0.58f;
    protected float WallSlideMinYVel = -0.11f;
    protected float WallSlideMaxYVel = Mathf.Infinity;
    protected Vector2 WallKickForce = new Vector2(0.35f, 0.46f);
	private readonly Vector2 HitByEnemyForce = new Vector2(0.5f, 0.5f);

    protected float MaxVelXFromInput = 0.35f; // we can travel faster than this, but NOT by just pushing left/right.

    protected float PostWallKickHorzHaltTime = 99f; // how many SECONDS after wall-kick when we auto-halt our xVel.
    protected float PostWallKickHorzInputLockDur = 0.22f; // affects isPreservingWallKickVel. How long until we can provide horz-input after wall-kicking.
    protected float WallKickExtensionWindow = 0.15f; // how long after touching a wall when we'll still allow wall-kicking!
    private const float WallPushAwayFallWindow = 0.12f; // how long we hold down pushing away from a wall before we register the push! For easier wall-kicking for folks who push away just before jumping.
    private const float CoyoteTimeJumpWindow = 0.07f; // in SECONDS. How long after leaving ground where we can STILL jump. Named after Wile E. Coyote!
	private const float DelayedJumpWindow = 0.12f; // in SECONDS. The time window where we can press jump just BEFORE landing, and still jump when we land.
	private const float PostDamageImmunityDuration = 1.2f; // in SECONDS.
    protected bool DoesFarFallHop = true; // Say FALSE if we don't want to bounce lightly when landing on Ground from a tall height.
    protected bool DoesFunHop = true; // Say FALSE if we don't want to hophophop after eating a Snack.
    
    override protected void InitMyPhysicsValues() {
        base.InitMyPhysicsValues();
        
        StartingHealth = 1;
        
        MaxVelXAir = 0.35f;
        MaxVelXGround = 0.25f;
        MaxVelYUp = 3;
        MaxVelYDown = -3;
    }
    
    
	// Components
	[SerializeField] protected PlayerBody myBody=null;
	// Properties
	private bool isPostDamageImmunity = false;
    private bool pfeetOnGround; // last frame's feetOnGround truthiness.
    private bool isPostWallKickInputLock = false; // if TRUE, we're horz locked out of input.
	protected bool isPreservingWallKickVel = false; // if TRUE, we don't apply air friction. Set to false when A) Time's past PostWallKickHorzInputLockDur, or B) Non-head touches any collider (so landing on ground, or side-hitting wall).
    private float maxYSinceGround=Mathf.NegativeInfinity; // the highest we got since we last made ground contact. Used to determine bounce vel!
	private float timeLastWallKicked=Mathf.NegativeInfinity;
    protected float timeStoppedWallSlide { get; private set; }
    protected float timeWhenLanded { get; private set; } // time when feet last landed on a collider.
    private float timeWhenAteEdible=Mathf.NegativeInfinity;
    private float timeWhenDelayedJump=Mathf.NegativeInfinity; // for jump AND wall-kick. Set when in air and press Jump. If we touch ground/wall before this time, we'll do a delayed jump or wall-kick!
    private float timeLastBounced=Mathf.NegativeInfinity;
    private float timeLastGrounded=Mathf.NegativeInfinity;
    private float timePushingAwayFromWall=Mathf.NegativeInfinity;
    public int DirFacing { get; private set; }
	protected int wallSlideDir { get; private set; } // 0 for not wall-sliding; -1 for wall on left; 1 for wall on right.
    private Vector2 pLeftStick; // previous InputController LeftStick.
	protected Vector2 pvel { get; private set; } // previous velocity.
    protected Vector2 ppvel { get; private set; } // HACKY workaround for getting vel from hitting a wall. ppvel is ACTUALLY how fast we were going before we hit the wall. ANY use of ppv is a haack.
    // References
    private Rect camBoundsLocal; // for detecting when we exit the room!
	private List<Edible> ediblesHolding = new List<Edible>(); // like in Celeste. I hold Edibles (i.e. Gem, Snack) until I'm standing somewhere safe to "eat" (aka collect) them.

	// Getters (Public)
	virtual public bool MayUseBattery() { return false; }
	public bool IsPostDamageImmunity { get { return isPostDamageImmunity; } }
    // Getters (Protected, Physics)
    override protected float FrictionAir() { return isPreservingWallKickVel ? 1f : FrictionGround(); } // No air friction while we're preserving our precious wall-kick vel.
    override protected float FrictionGround() {
        if (Mathf.Abs(LeftStick.x) > 0.1f) { return 0.7f; } // Providing input? Less friction!
        return 0.5f; // No input? Basically halt.
    }
    virtual protected float InputXScale() { return 1; } // for conditional scaling of our horz input! Currently only for Flatline.
    virtual protected float ExtraBounceDistToRestore() { return 0; }
	// Getters (Protected)
    protected InputController inputController { get { return InputController.Instance; } }
    protected bool IsInput_D() { return LeftStick.y < -0.5f; }
    protected bool IsInput_U() { return LeftStick.y >  0.5f; }
    protected bool IsInput_L() { return LeftStick.x < -0.5f; }
    protected bool IsInput_R() { return LeftStick.x >  0.5f; }
    protected Vector2 LeftStick { get { return inputController.LeftStick; } }
    protected int DirXToSide(int dirX) { return dirX<0 ? myWhiskers.SideLeft : myWhiskers.SideRight; }
    protected bool isWallSliding() { return wallSlideDir!=0; }
	virtual protected bool MayJump() {
        if (IsGrounded()) { return true; } // Grounded? Sure!
        if (vel.y < 0 && Time.time < timeLastGrounded+CoyoteTimeJumpWindow) { return true; } // Falling, and juuust left ground like a millisecond ago? Ok!
        return false; // Nah.
    }
	virtual protected bool MayWallKick() {
		if (IsGrounded()) { return false; } // Obviously no.
        if (IsAgainstWall()) { return true; } // Touching a wall? Sure!
		if (Time.time < timeLastTouchedWall+WallKickExtensionWindow // Not touching wall, BUT recently was, AND I'm still very close to it??
            && myWhiskers.DistToSideLastTouchedWall() < 0.5f) { return true; }
        return false;
	}
    virtual protected Vector2 VelForWallKick() {
        return new Vector2(-myWhiskers.DirLastTouchedWall*WallKickForce.x, Mathf.Max(vel.y, WallKickForce.y));
    }
	virtual protected bool MayWallSlide() {
		return !IsGrounded() && !IsInLift;
	}
    virtual protected bool MayEatEdibles() {
		return myWhiskers.AreFeetOnEatEdiblesGround();
    }
    virtual protected bool MaySetGroundedRespawnPos() { return true; } // Override if you don't wanna set GroundedRespawnPos while plunging, etc.
    override protected float HorzMoveInputVelXDelta() {
		if (Mathf.Abs(LeftStick.x) < 0.05f) { return 0; } // No input? No input.
        // Wall-sliding? No movement.
        if (isWallSliding() && timePushingAwayFromWall<WallPushAwayFallWindow) {
            return 0;
        }
        float dirX = MathUtils.Sign(LeftStick.x);
		// TESTing out controls!
		float mult = 1;//feetOnGround() ? 1 : 0.65f;
		if (!MathUtils.IsSameSign(dirX, vel.x)) { // Pushing the other way? Make us go WAY the other way, ok?
			//mult = 3;Note: DISABLED.
			// If we're pushing AGAINST our velocity AND we just kicked off a wall, don't allow the input, ok?
			if (isPostWallKickInputLock) {//Time.time < timeLastWallKicked+PostWallKickHorzInputLockDur) {
				mult = 0;
			}
		}
        // We're maxed-out vel? Don't accept more input in this dir.
        if (vel.x >  MaxVelXFromInput && LeftStick.x>0) { mult = 0; }
        if (vel.x < -MaxVelXFromInput && LeftStick.x<0) { mult = 0; }

		return dirX*InputEffectX*InputXScale() * mult;
	}
    protected float timeSinceBounce { get { return Time.time - timeLastBounced; } }
	// Getters (Private)
	private bool CanTakeDamage() {
		return !isPostDamageImmunity;
	}
    private bool IsBouncyCollidable(Collidable collidable) { return collidable != null && collidable.IsBouncy; }
	virtual protected bool DoBounceOffColl(int mySide, Collidable coll) {
        if (mySide==Sides.B && IsInput_D()) { return false; } // Pushing down? No bounce up.
        //if (mySide==Sides.T && IsInput_U()) { return false; } // Pushing up? No bounce down.
        if (!MayBounceOffColl(coll)) { return false; } // We explicitly may NOT bounce off this.
        return IsBouncyCollidable(coll); // Ok, I'll bounce only if this fella's bouncy.
	}
    protected bool MayBounceOffColl(Collidable coll) {
        return coll!=null && coll.MayBounce;
    }
    // Setters
    public void SetDirFacing(int _dir) {
        DirFacing = _dir;
    }
    public List<Edible> GetEdiblesHolding() { return ediblesHolding; }
    public void SetEdiblesHolding(List<Edible> _edibles) {
        for (int i=0; i<_edibles.Count; i++) {
            OnTouchEdible(_edibles[i]);
        }
    }
    
    //protected Vector2 GravityForce = 
    //override protected Vector2 Gravity { get { return base.Gravity * FlipDir; } }
    //override protected float JumpForce { get { return base.JumpForce * FlipDir; } }
    //override public bool IsGrounded() {
    //    return myWhiskers.OnSurface(FlipDir<0 ? Sides.T : Sides.B);
    //}
    //override protected float WallSlideMinYVel { get { return FlipDir<0 ? Mathf.NegativeInfinity : -0.11f; } }
    //override protected float WallSlideMaxYVel { get { return FlipDir<0 ? 0.11f : Mathf.Infinity; } }
    //override protected Vector2 WallKickForce { get { return new Vector2(0.35f, 0.46f*FlipDir); } }
    //protected override Vector2 GetVelForWallKick() {
    //    if (FlipDir < 0) {
    //        return new Vector2(-myWhiskers.DirLastTouchedWall*WallKickForce.x, Mathf.Min(vel.y, WallKickForce.y));
    //    }
    //    return base.GetVelForWallKick();
    //}
    //private static int FlipDir=1; // 1 or -1.

 //   private void OnDrawGizmos() {
	//	if (myRoom==null) { return; }
	//	Gizmos.color = Color.cyan;
	//	Gizmos.DrawWireCube (myRoom.PosGlobal+camBoundsLocal.center, new Vector3(camBoundsLocal.size.x,camBoundsLocal.size.y, 10));
	//}

	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	//override protected void Start () {
	//	base.Start();

	//	SetSize (new Vector2(1.5f, 1.8f));
	//}
	public void Initialize(Room _myRoom, PlayerData data) {
		base.BaseInitialize(_myRoom, data);
        
        // Give huge linear/angular drag to my rigidbody.
        Rigidbody2D myRB = GetComponent<Rigidbody2D>();
        myRB.drag = 999999;
        myRB.angularDrag = 999999;
        
        DirFacing = data.dirFacing;
        SetVel(data.vel);
        timeStoppedWallSlide = Mathf.NegativeInfinity;

		// Set camBoundsLocal!
		const float boundsBloat = 0f; // I have to like *really* be off-screen for this to register.
		camBoundsLocal = MyRoom.GetCameraBoundsLocal();
		camBoundsLocal.size += new Vector2(boundsBloat,boundsBloat)*2f;
		camBoundsLocal.position -= new Vector2(boundsBloat,boundsBloat);
        
        GameManagers.Instance.DataManager.UnlockPlayerType(PlayerType()); // TEMP: Also unlock this PlayerType here (and only here).
    }

 //   override protected void SetSize(Vector2 _size) {
	//	base.SetSize(_size);
	//	myBody.SetSize(_size);
	//}
	public void SetPosLocal(Vector2 _posLocal) {
		pos = _posLocal;
	}
	public void SetPosGlobal(Vector2 _posGlobal) {
		SetPosLocal(_posGlobal - MyRoom.PosGlobal);
	}



    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
	virtual protected void Update() {
		if (!DoUpdate()) { return; } // Not supposed to Update? No dice.

		AcceptButtonInput();
		UpdatePostDamageImmunity();
	}
	virtual protected void AcceptButtonInput() {
		if (inputController.IsJump_Press) {
			OnButtonJump_Press();
		}
		else if (inputController.IsJump_Release) {
			OnButtonJump_Release();
		}
        else if (inputController.IsAction_Press) {
            OnButtonAction_Press();
        }
		else if (inputController.LeftStick.y < -0.7f) {
			OnDown_Held();
		}
	}
	private void UpdatePostDamageImmunity() {
		if (isPostDamageImmunity) {
			if (Time.time >= timeSinceDamage+PostDamageImmunityDuration) {
				EndPostDamageImmunity();
			}
		}
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	virtual protected void FixedUpdate () {
		//if (inputController == null) { return; } // Safety check for runtime compile.
        if (!DoUpdate()) { return; } // Not supposed to Update? No dice.

        //myWhiskers.UpdateSurfaces(); // TEST
        ApplyVelFromSurfaces();
        
        Vector2 ppos = pos;
        ppvel = pvel;
        pvel = vel;

		ApplyFriction();
		ApplyGravity();
        ApplyInternalForces();
		AcceptDirectionalMoveInput();
        RegisterJoystickPushReleases();
		ApplyTerminalVel();
        ApplyLiftForces(); // Note: This happens AFTER TerminalVel.
        
        myWhiskers.UpdateSurfaces(); // Update surfaces BEFORE wall-slide and applying vel.
        UpdateWallSlide();
        ApplyVel();
        // Update vel to be the distance we ended up moving this frame.
        SetVel(pos - ppos);
        
        //myWhiskers.UpdateSurfaces(); // TEST update surfaces AGAIN.
        
        UpdateDirFacing();
		UpdateTimeLastTouchedWall();
        UpdateTimePushingAwayFromWall();
		DetectJumpApex();
		UpdateMaxYSinceGround();
		UpdatePostWallKickVelValues();

        pfeetOnGround = IsGrounded();

		UpdateExitedRoom();
	}

	private void UpdateWallSlide() {
		if (isWallSliding()) {
            float velY = Mathf.Clamp(vel.y, WallSlideMinYVel,WallSlideMaxYVel);
			SetVel(new Vector2(0, velY)); // Halt x-vel and give us a minimum yVel!
		}
		// Note: We want to do this check constantly, as we may want to start wall sliding while we're already against a wall.
		// We're NOT wall-sliding...
		if (!isWallSliding()) {
			// Should we START wall-sliding??
			if (MayWallSlide()) {
				if (myWhiskers.OnSurface(myWhiskers.SideLeft) && vel.x<=0) {
					StartWallSlide(-1);
				}
				else if (myWhiskers.OnSurface(myWhiskers.SideRight) && vel.x>=0) {
					StartWallSlide(1);
				}
			}
		}
	}

//	private int wallSlideDir {
//		get {
//			if (isPlunging) { return 0; } // No wall-sliding if I'm plunging, ok?
//			if (!feetOnGround()) { // If my feet AREN'T on the ground...!
//				if (onSurfaces[Sides.L]) { return -1; }
//				if (onSurfaces[Sides.R]) { return  1; }
//			}
//			return 0; // Nah, not wall-sliding.
//		}
//	}
    private void UpdateDirFacing() {
        // If I'm NOT wall-sliding, then make my DirFacing be what dir input is pushing.
        if (!isWallSliding()) {
            float inputX = LeftStick.x;//HorzMoveInputVelXDelta();
            if (Mathf.Abs(inputX) > 0.001f) {
                DirFacing = MathUtils.Sign(inputX);
            }
        }
    }

    private void UpdateTimePushingAwayFromWall() {
        if (isWallSliding() && LeftStick.x*wallSlideDir<0) { // wall-sliding, AND pushing away from wall?? Increment value!
            timePushingAwayFromWall += Time.deltaTime;
        }
        else {
            timePushingAwayFromWall = 0;
        }
    }
    private void UpdatePostWallKickVelValues() {
        if (isPostWallKickInputLock) {
            if (Time.time >= timeLastWallKicked+PostWallKickHorzInputLockDur) { // If our lock-input window is over...
                isPostWallKickInputLock = false;
            }
        }
		if (isPreservingWallKickVel) {
			if (Time.time >= timeLastWallKicked+PostWallKickHorzHaltTime) { // If our preserve-wall-kick-vel window is over...
				isPreservingWallKickVel = false;
			}
            // If we're pushing AGAINST our vel, CANCEL preserving wall-kick vel.
            if (!isPostWallKickInputLock && LeftStick.x*MathUtils.Sign(vel.x) < -0.5f) {
                isPreservingWallKickVel = false;
            }
		}
	}
	virtual protected void UpdateMaxYSinceGround() {
		maxYSinceGround = Mathf.Max(maxYSinceGround, pos.y);
	}
	private void UpdateExitedRoom() {
		if (FramesAlive < 10) { return; } // Fudgey safety check: Don't check if we've exited a room for the first few frames.
		// I'm outside the room!
		if (!camBoundsLocal.Contains(PosLocal)) {
			int sideEscaped = MathUtils.GetSidePointIsOn(camBoundsLocal, PosLocal);
			GameManagers.Instance.EventManager.OnPlayerEscapeRoomBounds(sideEscaped);
		}
	}
	private void DetectJumpApex() {
		if (pvel.y > 0 && vel.y<=0) {
			OnHitJumpApex();
		}
	}
	virtual protected void OnHitJumpApex() { }


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	virtual protected void Jump() {
        StopWallSlide();
		SetVel(new Vector2(vel.x, JumpForce));
		timeWhenDelayedJump = -1; // reset this just in case.
        GameManagers.Instance.EventManager.OnPlayerJump(this);
	}
	virtual protected void WallKick() {
        StopWallSlide();
        SetVel(VelForWallKick());
        timeWhenDelayedJump = -1; // reset this just in case.
        timeLastWallKicked = Time.time;
		isPreservingWallKickVel = true;
        isPostWallKickInputLock = true;
        ResetMaxYSinceGround();
		GameManagers.Instance.EventManager.OnPlayerWallKick(this);
	}


	virtual protected void StartWallSlide(int dir) {
		wallSlideDir = dir;
        DirFacing = wallSlideDir * -1;
        myBody.OnStartWallSlide();
    }
	protected void StopWallSlide() {
        if (!isWallSliding()) { return; } // Not wall-sliding? Do nothing.
		wallSlideDir = 0;
        timeStoppedWallSlide = Time.time;
        myBody.OnStopWallSlide();
    }

    override protected void TakeDamage(int damageAmount) {
        base.TakeDamage(damageAmount);
        // I've still got juice in me: Start post-damage immunity!
        if (!IsDead) {
            StartPostDamageImmunity();
        }
    }

    private void StartPostDamageImmunity() {
		isPostDamageImmunity = true;
	}
	private void EndPostDamageImmunity() {
		isPostDamageImmunity = false;
		myBody.OnEndPostDamageImmunity();
	}
    
    protected void ResetMaxYSinceGround() {
        maxYSinceGround = pos.y;
    }
    
    virtual protected void DropThruPlatform() {
        pos += new Vector2(0, -0.2f);
        ChangeVel(new Vector2(0, -0.16f));
        myBody.OnDropThruPlatform();
    }
    



	// ----------------------------------------------------------------
	//  Events (Input)
	// ----------------------------------------------------------------
    private void RegisterJoystickPushReleases() {
        if (LeftStick.x < -0.1f && pLeftStick.x >= -0.1f) { OnLPush(); }
        if (LeftStick.x >  0.1f && pLeftStick.x <=  0.1f) { OnRPush(); }
        if (LeftStick.x >= -0.1f && pLeftStick.x < -0.1f) { OnLRelease(); }
        if (LeftStick.x  <  0.1f && pLeftStick.x >= 0.1f) { OnRRelease(); }
        
        pLeftStick = LeftStick;
    }
    virtual protected void OnLPush() { }
    virtual protected void OnRPush() { }
    virtual protected void OnLRelease() { }
    virtual protected void OnRRelease() { }
    virtual protected void OnButtonAction_Press() {}
    virtual protected void OnButtonJump_Press() {
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
	virtual protected void OnButtonJump_Release() { }
	virtual protected void OnDown_Held() {
        // On a Platform? Drop down through it!
        if (myWhiskers.AreFeetOnlyOnCanDropThruPlatform()) {
            DropThruPlatform();
        }
    }
    
	protected void ScheduleDelayedJump() {
		timeWhenDelayedJump = Time.time + DelayedJumpWindow;
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override public void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);
        Collidable collidable = col.GetComponent<Collidable>();
        
        // Touching any side EXCEPT my head immediately stops our wall-kick-vel preservation. (Exception is so that we don't halt x-vel just by bumping our head.)
        if (side != myWhiskers.SideHead) {
            isPreservingWallKickVel = false;
            isPostWallKickInputLock = false;
        }
        // Enemy?
        Enemy enemy = collidable as Enemy;
        if (enemy != null) {
            OnCollideWithEnemy(side, enemy);
        }

        // Do my own stuff!
        if (false) {}
        else if (side==myWhiskers.SideFeet) { OnFeetTouchCollidable(collidable); }
        else if (side==myWhiskers.SideHead) { OnHeadTouchCollidable(collidable); }
        else                                { OnArmTouchCollidable(side, collidable); }
        
        //ResetMaxYSinceGround(); // Reset maxYSinceGround when touch any collider.
	}
	override public void OnWhiskersLeaveCollider(int side, Collider2D col) {
		base.OnWhiskersLeaveCollider(side, col);
        Collidable collidable = col.GetComponent<Collidable>();
        
        //ResetMaxYSinceGround(); // Reset maxYSinceGround when leave any collider.

        // Feet?
        if (side == myWhiskers.SideFeet) {
            OnFeetLeaveCollidable(collidable);
        }

		// We ARE wall-sliding!
		if (isWallSliding()) {
			// Should we stop wall-sliding??
			if (wallSlideDir==-1 && !myWhiskers.OnSurface(myWhiskers.SideLeft)) {//side==Sides.L) {
				StopWallSlide();
			}
			else if (wallSlideDir==1 && !myWhiskers.OnSurface(myWhiskers.SideRight)) {//side==Sides.R) {
				StopWallSlide();
			}
		}
	}
	virtual protected void OnFeetTouchCollidable(Collidable collidable) {
		if (MayEatEdibles()) {
			EatEdiblesHolding();
		}
        
		// Bounce!
		if (DoBounceOffColl(myWhiskers.SideFeet, collidable)) {
			BounceOffCollidable_Up(collidable);
		}
		// Otherwise...
		else {
            // Feet WEREN'T on ground?? Consider this a landing!
            if (!pfeetOnGround) {
			    LandOnCollidable(collidable);
            }
		}
	}
    private void OnHeadTouchCollidable(Collidable collidable) {
        // Bounce!
        if (DoBounceOffColl(myWhiskers.SideHead, collidable)) {
            BounceOffCollidable_Down(collidable);
        }
        // Land.
        else {
            ResetMaxYSinceGround();
        }
    }
	virtual protected void OnArmTouchCollidable(int side, Collidable collidable) {
        // Bouncy collidable?
        if (DoBounceOffColl(side, collidable)) {
            BounceOffCollidable_Side(collidable);
        }
        // Delayed wall-kick? Do it right away!
        else if (MayWallKick() && collidable is BaseGround && Time.time <= timeWhenDelayedJump) {//isWallSliding() && 
            WallKick();
        }
    }
    virtual protected void OnFeetLeaveCollidable(Collidable collidable) {
        timeLastGrounded = Time.time;
        // Left Ground?
        BaseGround ground = collidable as BaseGround;
        if (ground != null) {
            // Left respawn-friendly Ground, AND we can set GroundedRespawnPos??
            if (ground.IsPlayerRespawn && MaySetGroundedRespawnPos()) {
                SetGroundedRespawnPos(ground);
            }
        }
    }
    /// Intelligently sets what GroundedRespawnPos SHOULD be based on our position relative to this Ground. I.e. not on its edge, or in the air.
    private void SetGroundedRespawnPos(BaseGround ground) {
        Rect gr = ground.GetMyRect();
        float marginX = 2f; // how much farther from the edge we wanna prevent spawning from.
        marginX = Mathf.Min(marginX, gr.width*0.5f); // limit marginX for small grounds/platforms.
        float posX = pos.x;
        float posY = gr.y + gr.height*0.5f + Size.y*0.5f + 0.01f; // top 'o the ground.
        posX = Mathf.Max(gr.x-gr.width*0.5f + marginX, posX);
        posX = Mathf.Min(gr.x+gr.width*0.5f - marginX, posX);

        GameManagers.Instance.DataManager.playerGroundedRespawnData = new PlayerData {
            pos = new Vector2(posX,posY),
            type = PlayerType(),
            dirFacing = DirFacing,
            rotation = 0,
            vel = Vector2.zero,
        };
    }


    
    virtual protected void BounceOffCollidable_Up(Collidable collidable) {
        // We WERE grounded? Do special Portal-2-bouncy-gel-like bounce!
        if (pfeetOnGround) {
            float yVel = Mathf.Abs(vel.x) * 1.4f;
            SetVel(new Vector2(vel.x, yVel));
        }
        // We WEREN'T grounded before? Do normal bounce!
        else {
            // Find how fast we have to move upward to restore our previous highest height, and set our vel to that!
            float distToRestore = Mathf.Max (0, maxYSinceGround-pos.y);
            distToRestore += ExtraBounceDistToRestore(); // Give us __ more height than we started with.
            float yVel = Mathf.Sqrt(2*-Gravity().y*distToRestore); // 0 = y^2 + 2*g*dist  ->  y = sqrt(2*g*dist)
            yVel *= 0.982f; // hacky fudge: we're getting too much height back.
            yVel = Mathf.Max(0.2f, yVel); // have at LEAST this much bounce. (i.e. no teeeeny-tiny bouncing.)
            SetVel(new Vector2(vel.x, yVel));
        }
        timeLastBounced = Time.time;
        // Inform the collidable!!
        if (collidable != null) {
            collidable.OnPlayerFeetBounceOnMe(this);
        }
    }
    virtual protected void BounceOffCollidable_Down(Collidable collidable) {
        timeLastBounced = Time.time;
        SetVel(new Vector2(vel.x, -Mathf.Abs(ppvel.y)));
    }
	private void BounceOffCollidable_Side(Collidable collidable) {
        timeLastBounced = Time.time;
        timeLastWallKicked = Time.time;
		SetVel(new Vector2(-ppvel.x, Mathf.Max(ppvel.y, Mathf.Abs(ppvel.x)+0.1f)));
	}
    /// Called when our feet WEREN'T touching ground, but now they are (and we're NOT bouncing).
	virtual protected void LandOnCollidable(Collidable collidable) {
		// Finally reset maxYSinceGround.
		ResetMaxYSinceGround();
        timeWhenLanded = Time.time;
        StopWallSlide();
        // Do that delayed jump we planned?
        if (Time.time <= timeWhenDelayedJump) {
			Jump();
		}
        // Otherwise, if we may do a fun-hop...
        else if (MayDoAFunHop()) {
            // Do FarFallHop?
            if (ShouldDoFallFarHop()) {
                FarFallHop(collidable);
            }
            else if (Time.time <= timeWhenAteEdible+0.8f) {
                DoOneJustAteHappyHop();
            }
        }
	}
    /// Returns TRUE if we can do a far-fall hop or just-ate-happy-hop.
    private bool MayDoAFunHop() {
        if (!DoesFunHop) { return false; } // No, I don't do that sort of thing.
        //if (!IsGrounded()) { return false; } // Not on ground? Can't hop.
        Collidable collOn = myWhiskers.TEMP_GetFloorCollidable();
        if (collOn!=null) {
            Ground g = collOn as Ground;
            if (g!=null && g.PreventHappyHop) { return false; } // This Ground specifically says we can't.
            DispGround dg = collOn as DispGround;
            if (dg != null && dg.DieFromPlayerLeave) { return false; } // DispGround that'll disappear if we leave it? Don't hop!
        }
        return true; // Looks good!
    }
    private bool ShouldDoFallFarHop() {
        return DoesFarFallHop && ppvel.y < -1.05f;
    }
    
    /// When we land from great height, it feels nice to thump on the ground. This does that.
    private void FarFallHop(Collidable collidable) {
        if (ppvel.y < -1.22f) {
            SetVel(new Vector2(vel.x, Mathf.Abs(ppvel.y)*0.22f));
        }
        else {
            SetVel(new Vector2(vel.x, Mathf.Abs(ppvel.y)*0.16f));
        }
    }

	private void OnCollideWithEnemy(int side, Enemy enemy) {
        // TODO: Ask the ENEMY if that side is dangerous, instead of if my side is vulnerable.
        if (side!=Sides.B && CanTakeDamage()) { // It's NOT my feet, and I CAN take damage...
    		int dirToEnemy = MathUtils.Sign(enemy.PosGlobal.x-PosGlobal.x, false);
    		SetVel(new Vector2(-dirToEnemy*HitByEnemyForce.x, HitByEnemyForce.y));
    		TakeDamage(1);
        }
	}
    
//	// We ARE wall-sliding!
//	if (isWallSliding()) {
//		// Should we stop wall-sliding??
//		if (wallSlideDir==-1 && !onSurfaces[Sides.L]) {
//			StopWallSlide();
//		}
//		else if (wallSlideDir==1 && !onSurfaces[Sides.R]) {
//			StopWallSlide();
//		}
//	}
//	// We're NOT wall-sliding...
//	else {
//		// Should we START wall-sliding??
//		if (!feetOnGround() && !isPlunging) {
//			if (onSurfaces[Sides.L] && vel.x<-0.001f) {
//				StartWallSlide(-1);
//			}
//			else if (onSurfaces[Sides.R] && vel.x>0.001f) {
//				StartWallSlide(1);
//			}
//		}
//	}
//	private void OnTriggerExit2D(Collider2D col) {
//		Collidable collidable = col.GetComponent<Collidable>();
//		if (collidable != null) {
//
//		}
//	}

	public void OnTouchEdible(Edible edible) {
        ediblesHolding.Add(edible);
        edible.OnPlayerPickMeUp(this);
		if (MayEatEdibles()) {
            EatEdiblesHolding();
		}
	}
	public void EatEdiblesHolding() {
        if (ediblesHolding.Count == 0) { return; } // No edibles? Do nothin'.
		foreach (Edible obj in ediblesHolding) {
			obj.GetEaten();
		}
		ediblesHolding.Clear();
        timeWhenAteEdible = Time.time;
        myBody.OnEatEdiblesHolding();
        if (MayDoAFunHop()) {
            DoOneJustAteHappyHop();
        }
    }
    private void DoOneJustAteHappyHop() {
        SetVel(new Vector2(vel.x*0.2f, Mathf.Max(vel.y, -Gravity().y*6f))); // Hop happily if we just ate a Snack!
	}




	// ----------------------------------------------------------------
	//  Events (Health)
	// ----------------------------------------------------------------
	public void OnTouchHarm() {
		TakeDamage(1);
	}

	override protected void Die() {
		myBody.OnDie();
		GameManagers.Instance.EventManager.OnPlayerDie(this);
		base.Die();
        // TEMP super simple just disable my whole GO.
        this.gameObject.SetActive(false);
	}

	virtual public void OnUseBattery() { }
    

    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData ToData() {
        return new PlayerData {
            pos = pos,
            vel = vel,
            dirFacing = DirFacing,
            type = PlayerType(),
        };
    }


}


//	private const float RunAccel = 1000f;
//	private const float RunReduce = 400f;
//	float moveX = inputAxis.x;
//	float velXTarget = MaxVelX*moveX;
//	float mult = onGround ? 1 : 0.65f;
//	bool isPushingOppositeVelX = !MathUtils.IsSameSign(moveX, vel.x);
//	if (Mathf.Abs(vel.x)>MaxVelX && isPushingOppositeVelX) {
//		velXDelta = 
//			inputAxis.x*INPUT_SCALE_X
//			vel.x = Calc.Approach(vel.x, MaxVelX*moveX, RunReduce*mult); // Reduce back from beyond the max
//	}
//	else {
//		vel.x = Calc.Approach(vel.x, MaxVelX*moveX, RunAccel*mult); // Approach max speed
//	}

//	private void UpdateSize() {
//		float sizeLoc = Mathf.InverseLerp(70,-70, vel.y);
//		Vector2 targetSize = Vector2.Lerp(sizeUpward, sizeDownward, sizeLoc);
//		currentSize = Vector2.Lerp(currentSize, targetSize, 0.8f); // ease!
//		ApplyCurrentSize();
//	}


//private void UpdateIsPreservingWallKickVel() {
//    if (isPreservingWallKickVel) {
//        if (vel.y < 0  // Once we reach the height of our wall-kick, halt our velocity! It's tighter to control.
//         || feetOnGround()) {
//            isPreservingWallKickVel = false;
//        }
//        else if (Time.time-PostWallKickHorzInputLockDur > timeLastWallKicked // If it's been at least a few grace frames since we wall-kicked...
//            && !MathUtils.IsSameSign(vel.x,inputAxis.x)) { // Pushing against my vel? Stop preserving the vel!
//            isPreservingWallKickVel = false;
//        }
//    }
//}