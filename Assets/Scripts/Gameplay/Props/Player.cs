using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Player : PlatformCharacter {
	// Constants
	override protected int StartingHealth { get { return 1; } }
	override protected float FrictionAir { get { return isPreservingWallKickVel ? 1f : FrictionGround; } } // No air friction while we're preserving our precious wall-kick vel.
	override protected float FrictionGround {
		get {
			if (Mathf.Abs(inputAxis.x) > 0.1f) { return 0.7f; } // Providing input? Less friction!
			return 0.5f; // No input? Basically halt.
		}
	}
	protected Vector2 GravityNeutral = new Vector2(0, -0.042f);
	virtual protected float InputScaleX { get { return 0.1f; } }

	virtual protected float JumpForce { get { return 0.58f; } }
	virtual protected float WallSlideMinYVel { get { return -0.12f; } }
	private readonly Vector2 WallKickVel = new Vector2(0.5f, 0.52f);
	private readonly Vector2 HitByEnemyVel = new Vector2(0.5f, 0.5f);

	override protected float MaxVelXAir { get { return 0.35f; } }
	override protected float MaxVelXGround { get { return 0.25f; } }
	override protected float MaxVelYUp { get { return 3; } }
	override protected float MaxVelYDown { get { return -3; } }

	private const float DelayedJumpWindow = 0.1f; // in SECONDS. The time window where we can press jump just BEFORE landing, and still jump when we land.
	private const float PostDamageImmunityDuration = 1.2f; // in SECONDS.
	private const float PostWallKickHorzInputLockDur = 0.22f; // affects isPreservingWallKickVel. How long until we can provide horz-input after wall-kicking.
    private const float WallKickExtensionWindow = 0.08f; // how long after touching a wall when we'll still allow wall-kicking!

	// Components
	[SerializeField] protected PlayerBody myBody=null;
	// Properties
	private bool isPostDamageImmunity = false;
	protected bool isPreservingWallKickVel = false; // if TRUE, we don't apply air friction. Set to false when A) Time's past PostWallKickHorzInputLockDur, B) Our yVel is negative, or C) We're on the ground.
    private float maxYSinceGround=Mathf.NegativeInfinity; // the highest we got since we last made ground contact. Used to determine bounce vel!
	private float timeLastWallKicked=Mathf.NegativeInfinity;
	private float timeSinceDamage=Mathf.NegativeInfinity; // invincible until this time! Set to Time.time + PostDamageInvincibleDuration when we're hit.
	private float timeWhenDelayedJump=Mathf.NegativeInfinity; // for jump AND wall-kick. Set when in air and press Jump. If we touch ground/wall before this time, we'll do a delayed jump or wall-kick!
	private int numJumpsSinceGround;
	private int wallSlideSide = 0; // 0 for not wall-sliding; -1 for wall on left; 1 for wall on right.
	private int hackTEMP_framesAlive=0;
	private Vector2 pvel; // previous velocity.
    static public Vector2 GroundedRespawnPos=Vector2Extensions.NaN; // I'll respawn at this pos. Set when we leave a Ground that has IsPlayerRespawn.
    // References
    private Rect camBoundsLocal; // for detecting when we exit the level!
	private List<Gem> gemsHolding = new List<Gem>(); // like in Celeste. I hold Gems until I'm standing somewhere safe to "eat" (aka collect) them.

	// Getters (Public)
	virtual public bool CanUseBattery() { return false; }
	public bool IsPostDamageImmunity { get { return isPostDamageImmunity; } }
	// Getters (Protected)
	protected bool MayJump() {
		return feetOnGround();//numJumpsSinceGround<MaxJumps && Time.time>=timeWhenCanJump
	}
	protected bool MayWallKick() {
		if (feetOnGround()) { return false; } // Obviously no.
		return isTouchingWall() || Time.time < timeLastTouchedWall+WallKickExtensionWindow;
	}
	virtual protected bool MayWallSlide() {
		return !feetOnGround();
	}
	private bool MayEatGems() {
		return myWhiskers.AreFeetOnEatGemGround();
    }
    virtual protected bool MaySetGroundedRespawnPos() { return true; } // Override if you don't wanna set GroundedRespawnPos while plunging, etc.
    override protected float HorzMoveInputVelXDelta() {
		if (InputController.Instance==null) { return 0; } // for building at runtime.
		if (inputAxis.x == 0) { return 0; }
		float dirX = MathUtils.Sign(inputAxis.x);
		// TESTing out controls!
		float mult = 1;//feetOnGround() ? 1 : 0.65f;
		if (!MathUtils.IsSameSign(dirX, vel.x)) { // Pushing the other way? Make us go WAY the other way, ok?
			mult = 3;
			// If we're pushing AGAINST our velocity AND we just kicked off a wall, don't allow the input, ok?
			if (isPreservingWallKickVel) {//Time.time < timeLastWallKicked+PostWallKickHorzInputLockDur) {
				mult = 0;
			}
		}

		return dirX*InputScaleX * mult;
	}
	// Getters (Private)
	protected Vector2 inputAxis { get { return InputController.Instance.PlayerInput; } }
	private bool isWallSliding() { return wallSlideSide!=0; }
	private bool CanTakeDamage() {
		return !isPostDamageImmunity;
	}
	virtual protected bool DoBounceOffCollidable(Collidable collidable) {
		return IsBouncyCollidable(collidable);
	}
	private bool IsBouncyCollidable(Collidable collidable) {
		if (collidable == null) { return false; } // The collidable is undefined? Default to NOT bouncy.
		return collidable.IsBouncy;
	}

	// Debug
	private void OnDrawGizmos() {
		if (myLevel==null) { return; }
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube (myLevel.PosGlobal+camBoundsLocal.center, new Vector3(camBoundsLocal.size.x,camBoundsLocal.size.y, 10));
	}

	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();

		SetSize (new Vector2(1.5f, 1.8f));
	}
	public void Initialize(Level _myLevel, PlayerData data) {
		base.BaseInitialize(_myLevel, data);

		// Set camBoundsLocal!
		const float boundsBloat = 0f; // I have to like *really* be off-screen for this to register.
		camBoundsLocal = myLevel.GetCameraBoundsLocal();
		camBoundsLocal.size += new Vector2(boundsBloat,boundsBloat)*2f;
		camBoundsLocal.position -= new Vector2(boundsBloat,boundsBloat);

        // Clear out GroundedRespawnPos.
        GroundedRespawnPos = Vector2Extensions.NaN;
    }

    override protected void SetSize(Vector2 _size) {
		base.SetSize(_size);
		myBody.SetSize(_size);
	}
	public void SetPosLocal(Vector2 _posLocal) {
		pos = _posLocal;
		SetVel(Vector2.zero);
	}
	public void SetPosGlobal(Vector2 _posGlobal) {
		SetPosLocal(_posGlobal - myLevel.PosGlobal);
	}



    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
	private void Update () {
		if (!DoUpdate()) { return; } // Not supposed to Update? No dice.

		AcceptButtonInput();
		UpdatePostDamageImmunity();
	}
	virtual protected void AcceptButtonInput() {
//		if (Input.GetKeyDown(KeyCode.UpArrow)) {
//			OnJumpPressed();
//		}
		if (Input.GetButtonDown("Jump")) {
			OnButtonJump_Down();
		}
		else if (Input.GetButtonUp("Jump")) {
			OnButtonJump_Up();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			OnDown_Down();
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
		if (InputController.Instance == null) { return; } // Safety check for runtime compile.
        if (!DoUpdate()) { return; } // Not supposed to Update? No dice.

        Vector2 ppos = pos;
		pvel = vel;

        ApplyVelFromFloor();
		ApplyFriction();
		ApplyGravity();
		AcceptHorzMoveInput();
		ApplyTerminalVel();
        ApplyLiftForces(); // Note: This happens AFTER TerminalVel.
		myWhiskers.UpdateSurfaces(); // update these dependently now, so we guarantee most up-to-date info.
		UpdateWallSlide();
		ApplyVel();

		UpdateTimeLastTouchedWall();
		DetectJumpApex();
		UpdateMaxYSinceGround();
		UpdateIsPreservingWallKickVel();
		// TEST auto-plunge
//		if (!feetOnGround() && !isPlunging && vel.y<-0.5f) {
//			StartBouncing();
//		}

		// Update vel to be the distance we ended up moving this frame.
		SetVel(pos - ppos);

		UpdateExitedLevel();
	}

	private void UpdateWallSlide() {
		if (isWallSliding()) {
			SetVel(new Vector2(vel.x, Mathf.Max(vel.y, WallSlideMinYVel))); // Give us a minimum yVel!
		}
		// Note: We want to do this check constantly, as we may want to start wall sliding while we're already against a wall.
		// We're NOT wall-sliding...
		if (!isWallSliding()) {
			// Should we START wall-sliding??
			if (MayWallSlide()) {
				if (myWhiskers.OnSurface(Sides.L) && vel.x<-0.01f) {
					StartWallSlide(-1);
				}
				else if (myWhiskers.OnSurface(Sides.R) && vel.x>0.01f) {
					StartWallSlide(1);
				}
			}
		}
	}

//	private int wallSlideSide {
//		get {
//			if (isPlunging) { return 0; } // No wall-sliding if I'm plunging, ok?
//			if (!feetOnGround()) { // If my feet AREN'T on the ground...!
//				if (onSurfaces[Sides.L]) { return -1; }
//				if (onSurfaces[Sides.R]) { return  1; }
//			}
//			return 0; // Nah, not wall-sliding.
//		}
//	}
	private void UpdateIsPreservingWallKickVel() {
		if (isPreservingWallKickVel) {
			if (Time.time >= timeLastWallKicked+PostWallKickHorzInputLockDur) { // If our preserve-wall-kick-vel window is over...
				isPreservingWallKickVel = false;
			}
		}
	}
	virtual protected void UpdateMaxYSinceGround() {
		maxYSinceGround = Mathf.Max(maxYSinceGround, pos.y);
	}
	private void UpdateExitedLevel() {
		if (hackTEMP_framesAlive++ < 10) { return; } // This is a hack.
		// I'm outside the level!
		if (!camBoundsLocal.Contains(PosLocal)) {
			int sideEscaped = MathUtils.GetSidePointIsOn(camBoundsLocal, PosLocal);
			GameManagers.Instance.EventManager.OnPlayerEscapeLevelBounds(sideEscaped);
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
		SetVel(new Vector2(vel.x, JumpForce));
		timeWhenDelayedJump = -1; // reset this just in case.
		numJumpsSinceGround ++;
		GameManagers.Instance.EventManager.OnPlayerJump(this);
	}
	virtual protected void WallKick() {
		SetVel(new Vector2(-myWhiskers.SideLastTouchedWall*WallKickVel.x, Mathf.Max(vel.y, WallKickVel.y)));
        timeWhenDelayedJump = -1; // reset this just in case.
        timeLastWallKicked = Time.time;
		isPreservingWallKickVel = true;
		numJumpsSinceGround ++;
		maxYSinceGround = pos.y; // TEST!!
		GameManagers.Instance.EventManager.OnPlayerWallKick(this);
	}


	virtual protected void StartWallSlide(int side) {
		wallSlideSide = side;
    }
	protected void StopWallSlide() {
		wallSlideSide = 0;
	}

	private void StartPostDamageImmunity() {
		isPostDamageImmunity = true;
	}
	private void EndPostDamageImmunity() {
		isPostDamageImmunity = false;
		myBody.OnEndPostDamageImmunity();
	}



	// ----------------------------------------------------------------
	//  Events (Input)
	// ----------------------------------------------------------------
//	private void OnJumpPressed() {
//		// We're on the ground and NOT timed out of jumping! Go!
//		if (feetOnGround && Time.time>=timeWhenCanJump) {//numJumpsSinceGround<MaxJumps
//			Jump();
//		}
//		else {
//			timeWhenDelayedJump = Time.time + DelayedJumpWindow;
//		}
//	}
	abstract protected void OnButtonJump_Down();
	virtual protected void OnButtonJump_Up() { }
	virtual protected void OnDown_Down() { }
    
	protected void ScheduleDelayedJump() {
		timeWhenDelayedJump = Time.time + DelayedJumpWindow;
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override public void OnWhiskersTouchCollider(int side, Collider2D col) {
		base.OnWhiskersTouchCollider(side, col);

		isPreservingWallKickVel = false; // touching any surface immediately stops our wall-kick-vel preservation.

		// Do my own stuff!
		Collidable collidable = col.GetComponent<Collidable>();
		if (side == Sides.B) {
			OnFeetTouchCollidable(collidable);
		}
		else {
			OnNonFeetTouchCollidable(collidable);
		}

		// We're NOT wall-sliding...
//		if (!isWallSliding()) {
		// Should we START wall-sliding??
//		if (!feetOnGround() && !isPlunging) {
//			if (side==Sides.L && vel.x<-0.01f) {
//				StartWallSlide(-1);
//			}
//			else if (side==Sides.R && vel.x>0.01f) {
//				StartWallSlide(1);
//			}
//		}
//		}
	}
	override public void OnWhiskersLeaveCollider(int side, Collider2D col) {
		base.OnWhiskersLeaveCollider(side, col);
        Collidable collidable = col.GetComponent<Collidable>();

        // Feet?
        if (side == Sides.B) {
            OnFeetLeaveCollidable(collidable);
        }

		// We ARE wall-sliding!
		if (isWallSliding()) {
			// Should we stop wall-sliding??
			if (wallSlideSide==-1 && !myWhiskers.OnSurface(Sides.L)) {//side==Sides.L) {
				StopWallSlide();
			}
			else if (wallSlideSide==1 && !myWhiskers.OnSurface(Sides.R)) {//side==Sides.R) {
				StopWallSlide();
			}
		}
	}
	private void OnFeetTouchCollidable(Collidable collidable) {
		numJumpsSinceGround = 0;
		if (MayEatGems()) {
			EatGemsHolding();
		}

		bool doBounce = DoBounceOffCollidable(collidable);
		// Bounce!
		if (doBounce) {
			BounceOffCollidable_Up(collidable);
		}
		// Land!
		else {
			LandOnCollidable(collidable);
		}
	}
	private void OnNonFeetTouchCollidable(Collidable collidable) {
		// Enemy??
		Enemy enemy = collidable as Enemy;
		if (enemy != null && CanTakeDamage()) {
			OnCollideWithEnemy(enemy);
		}

        // Delayed wall-kick? Do it right away!
        if (Time.time <= timeWhenDelayedJump) {
            WallKick();
        }

        //		else {
        //			// Should I bounce or jump?
        //			bool doBounce = IsBouncyCollidable(collidable);// && !IsDontBounceButtonHeld()
        //			if (doBounce) {
        //				BounceOffCollidable_Side(collidable);
        //			}
        //		}
    }
    private void OnFeetLeaveCollidable(Collidable collidable) {
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
        Rect gr = ground.MyRect();
        float marginX = 2f; // how much farther from the edge we wanna prevent spawning from.
        marginX = Mathf.Min(marginX, gr.width*0.5f); // limit marginX for small grounds/platforms.
        float posX = pos.x;
        float posY = gr.y + gr.height*0.5f + Size.y*0.5f + 0.01f; // top 'o the ground.
        posX = Mathf.Max(gr.x-gr.width*0.5f + marginX, posX);
        posX = Mathf.Min(gr.x+gr.width*0.5f - marginX, posX);

        GroundedRespawnPos = new Vector2(posX, posY);
    }



    virtual protected void BounceOffCollidable_Up(Collidable collidable) {
		// Find how fast we have to move upward to restore our previous highest height, and set our vel to that!
		float distToRestore = Mathf.Max (0, maxYSinceGround-pos.y);
		distToRestore += 3.2f; // TEST! Give us MORE than we started with!
		float yVel = Mathf.Sqrt(2*-Gravity.y*distToRestore); // 0 = y^2 + 2*g*dist  ->  y = sqrt(2*g*dist)
		yVel += 0.025f; // Hack!! We're not getting all our height back exactly. Fudge it for now.
		SetVel(new Vector2(vel.x, yVel));
		// Inform the collidable!!
		if (collidable != null) {
			collidable.OnPlayerBounceOnMe(this);
		}
	}
	// TEST! This whole function is a controls experiment.
	private void BounceOffCollidable_Side(Collidable collidable) { // NOTE: We're probably gonna wanna denote WHICH sides of collidables are bouncy...
		timeLastWallKicked = Time.time;
		SetVel(new Vector2(-vel.x, Mathf.Max(vel.y, Mathf.Abs(vel.x)*2f+0.1f)));
//		SetVel(new Vector2(-vel.x, JumpForce*0.7f));//vel.y+
		// Inform the collidable!!
		if (collidable != null) {
			collidable.OnPlayerBounceOnMe(this);
		}
	}
	virtual protected void LandOnCollidable(Collidable collidable) {
		// Finally reset maxYSinceGround.
		maxYSinceGround = pos.y;
		// Do that delayed jump we planned?
		if (Time.time <= timeWhenDelayedJump) {
			Jump();
		}
//			else { // TEMP TEST!
//				SetVel(new Vector2(vel.x, -vel.y * 0.2f));
//				if (Mathf.Abs(vel.y) < 0.05f) {
//					SetVel(new Vector2(vel.x, 0));
//				}
//			}
	}

	private void OnCollideWithEnemy(Enemy enemy) {
		int dirToEnemy = MathUtils.Sign(enemy.PosGlobal.x-PosGlobal.x, false);
		SetVel(new Vector2(-dirToEnemy*HitByEnemyVel.x, HitByEnemyVel.y));
		TakeDamage(1);
	}
    
//	// We ARE wall-sliding!
//	if (isWallSliding()) {
//		// Should we stop wall-sliding??
//		if (wallSlideSide==-1 && !onSurfaces[Sides.L]) {
//			StopWallSlide();
//		}
//		else if (wallSlideSide==1 && !onSurfaces[Sides.R]) {
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

	public void OnTouchGem(Gem gem) {
		if (MayEatGems()) {
			gem.GetEaten();
		}
		else {
			gemsHolding.Add(gem);
			gem.OnPlayerPickMeUp(this);
		}
	}
	private void EatGemsHolding() {
		foreach (Gem gem in gemsHolding) {
			gem.transform.SetParent(this.transform.parent); // pop the Gem back onto the Level.
			gem.GetEaten();
		}
		gemsHolding.Clear();
	}




	// ----------------------------------------------------------------
	//  Events (Health)
	// ----------------------------------------------------------------
	private void TakeDamage(int damageAmount) {
		health -= damageAmount;
		timeSinceDamage = Time.time;
		// Am I kaput??
		if (health <= 0) {
			Die();
		}
		// I've still got juice in me!
		else {
			StartPostDamageImmunity();
		}
	}

	public void OnTouchSpikes(Spikes spikes) {
		TakeDamage(1);
	}

	override protected void Die() {
		myBody.OnDie();
		GameManagers.Instance.EventManager.OnPlayerDie(this);
		base.Die();
	}

	virtual public void OnUseBattery() { }


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