using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlatformCharacter {
	// Constants
	override protected float FrictionAir { get { return isPreservingWallKickVel ? 1f : 0.7f; } } // No air friction while we're preserving our precious wall-kick vel.
	override protected float FrictionGround {
		get {
			if (Mathf.Abs(inputAxis.x) > 0.1f) { return 0.7f; } // Providing input? Less friction!
			return 0.5f; // No input? Basically halt.
		}
	}
	private Vector2 GravityNeutral = new Vector2(0, -0.042f);
	private Vector2 GravityPlunging = new Vector2(0, -0.084f); // gravity is much stronger when we're plunging!
	override protected Vector2 Gravity { get { return isPlunging ? GravityPlunging : GravityNeutral; } }

	private const float InputScaleX = 0.1f;
	private const float JumpForce = 0.58f;
	private const float WallSlideMinYVel = -0.1f;
	private readonly Vector2 WallKickVel = new Vector2(1f, 0.52f);
	private readonly Vector2 HitByEnemyVel = new Vector2(4f, 0.5f);

	private const float MaxVelXAir = 0.35f;
	private const float MaxVelXGround = 0.25f;
	private const float MaxVelYUp = 3;
	private const float MaxVelYDown = -3;

	private const float DelayedJumpWindow = 0.1f; // in SECONDS. The time window where we can press jump just BEFORE landing, and still jump when we land.
	private const float PostDamageImmunityDuration = 1.2f; // in SECONDS.
	private const float PostWallKickHorzInputLockDur = 0.3f; // how long until we can provide horizontal input after jumping off a wall.

	// Properties
	private bool isPlunging = false;
	private bool isPlungeRecharged = true;
	private bool isPostDamageImmunity = false;
	private bool isPreservingWallKickVel = false; // if TRUE, we don't apply air friction. Set to false if we provide opposite-dir input, and when we land.
	private bool groundedSincePlunge; // TEST for interactions with Batteries.
	private float maxYSinceGround=Mathf.NegativeInfinity; // the highest we got since we last made ground contact. Used to determine bounce vel!
	private float timeLastWallKicked=Mathf.NegativeInfinity;
	private float timeSinceDamage=Mathf.NegativeInfinity; // invincible until this time! Set to Time.time + PostDamageInvincibleDuration when we're hit.
	private float timeWhenDelayedJump=Mathf.NegativeInfinity; // set when we're in the air and press Jump. If we touch ground before this time, we'll do a delayed jump!
	private int health = 1; // we die when we hit 0.
	private int numJumpsSinceGround;
	private int wallSlideSide = 0; // 0 for not wall-sliding; -1 for wall on left; 1 for wall on right.
	// Components
	[SerializeField] private PlayerBody myBody=null;

	// Getters (Public)
	public bool IsPlungeRecharged { get { return isPlungeRecharged; } }
	public bool IsPostDamageImmunity { get { return isPostDamageImmunity; } }
	// Getters (Overrides)
//	override public bool IsAffectedByLift() { return !isPlunging; } // We're immune to Lifts while plunging!
	override protected float HorzMoveInputVelXDelta() {
		if (InputController.Instance==null) { return 0; } // for building at runtime.
		if (inputAxis.x == 0) { return 0; }
		float dirX = MathUtils.Sign(inputAxis.x);
		// TESTing out controls!
		float mult = feetOnGround() ? 1 : 1;//0.65f;
		if (!MathUtils.IsSameSign(dirX, vel.x)) { // Pushing the other way? Make us go WAY the other way, ok?
			mult = 3;
			// If we're pushing AGAINST our velocity AND we just kicked off a wall, don't allow the input, ok?
			if (Time.time < timeLastWallKicked+PostWallKickHorzInputLockDur) {
				mult = 0;
			}
		}

		return dirX*InputScaleX * mult;
	}
	// Getters (Private)
	private Vector2 inputAxis { get { return InputController.Instance.PlayerInput; } }
	private bool isWallSliding() { return wallSlideSide!=0; }
	private bool CanTakeDamage() {
		return !isPostDamageImmunity;
	}
	private bool CanStartPlunge() {
		if (feetOnGround()) { return false; } // I can't plunge if I'm on the ground.
		if (IsInLift) { return false; }
		return isPlungeRecharged;
	}
	private bool IsBouncyCollidable(Collidable collidable) {
		if (collidable == null) { return false; } // The collidable is undefined? Default to NOT bouncy.
		return collidable.IsBouncy;
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();

		SetSize (new Vector2(1.5f, 1.8f));
	}
	public void Initialize(Level _myLevel) {
		this.transform.SetParent(_myLevel.transform);
		this.transform.localScale = Vector3.one;
		this.transform.localPosition = Vector3.zero;
		this.transform.localEulerAngles = Vector3.zero;
	}

	override protected void SetSize(Vector2 _size) {
		base.SetSize(_size);
		myBody.SetSize(_size);
	}
	public void SetPos(Vector2 _pos) {
		pos = _pos;
		vel = Vector2.zero;
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		if (Time.timeScale == 0) { return; } // No time? No dice.

		AcceptJumpInput();
		UpdatePostDamageImmunity();
	}
	private void AcceptJumpInput() {
//		if (Input.GetKeyDown(KeyCode.UpArrow)) {
//			OnJumpPressed();
//		}
		// TEMP! todo: Use pinputAxis within InputController in a *FixedUpdate* loop to determine if we've just pushed up/down.
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) {
			OnUpPressed();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			OnDownPressed();
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
	private void FixedUpdate () {
		if (Time.timeScale == 0) { return; } // No time? No dice.
		Vector2 ppos = pos;

		UpdateOnSurfaces();
		ApplyFriction();
		ApplyGravity();
		AcceptHorzMoveInput();
		ApplyTerminalVel();
		myWhiskers.UpdateSurfaceDists(); // update these dependently now, so we guarantee most up-to-date info.
		UpdateWallSlide();
		ApplyVel();
		UpdateMaxYSinceGround();

		UpdateIsPreservingWallKickVel();
		// TEST auto-plunge
//		if (!feetOnGround() && !isPlunging && vel.y<-0.5f) {
//			StartBouncing();
//		}

		// Update vel to be the distance we ended up moving this frame.
		vel = pos - ppos;
	}

	private void ApplyTerminalVel() {
		float maxXVel = feetOnGround() ? MaxVelXGround : MaxVelXAir;
		float xVel = Mathf.Clamp(vel.x, -maxXVel,maxXVel);
		float yVel = Mathf.Clamp(vel.y, MaxVelYDown,MaxVelYUp);
		vel = new Vector2(xVel, yVel);
	}
	private void UpdateMaxYSinceGround() {
		// TEST!!
		if (!groundedSincePlunge) { return; }
		maxYSinceGround = Mathf.Max(maxYSinceGround, pos.y);
	}
	private void UpdateWallSlide() {
		// We ARE wall-sliding!
		if (isWallSliding()) {
			vel = new Vector2(vel.x, Mathf.Max(vel.y, WallSlideMinYVel)); // Give us a minimum yVel!
			// Should we stop wall-sliding??
			if (wallSlideSide==-1 && !onSurfaces[Sides.L]) {
				StopWallSlide();
			}
			else if (wallSlideSide==1 && !onSurfaces[Sides.R]) {
				StopWallSlide();
			}
		}
		// We're NOT wall-sliding...
		else {
			// Should we START wall-sliding??
			if (!feetOnGround() && !isPlunging) {
				if (onSurfaces[Sides.L] && vel.x<-0.001f) {
					StartWallSlide(-1);
				}
				else if (onSurfaces[Sides.R] && vel.x>0.001f) {
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
			if (isPlunging) { // Just started plunging? Forget about retaining my wall-kick vel!
				isPreservingWallKickVel = false;
			}
			else if (vel.y < 0) {
				isPreservingWallKickVel = false; // TEST
			}
			else if (Time.time-0.1f > timeLastWallKicked // If it's been at least a few grace frames since we wall-kicked...
				&& !MathUtils.IsSameSign(vel.x, inputAxis.x)) { // Pushing against my vel? Stop preserving the vel!
				isPreservingWallKickVel = false;
			}
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GroundJump() {
		vel = new Vector2(vel.x, JumpForce);
		timeWhenDelayedJump = -1; // reset this just in case.
		numJumpsSinceGround ++;
		GameManagers.Instance.EventManager.OnPlayerJump(this);
	}
	private void WallKick() {
		vel = new Vector2(-sideTouchingWall()*WallKickVel.x, Mathf.Max(vel.y, WallKickVel.y));
		timeLastWallKicked = Time.time;
		isPreservingWallKickVel = true;
		numJumpsSinceGround ++;
		maxYSinceGround = pos.y; // TEST!!
		GameManagers.Instance.EventManager.OnPlayerWallKick(this);
	}

	private void StartPlunge() {
		if (isPlunging) { return; } // Already plunging? Do nothing.
		StopWallSlide(); // can't both plunge AND wall-slide.
		isPlunging = true;
		isPlungeRecharged = false; // spent!
		groundedSincePlunge = false;
		myBody.OnStartPlunge();
		vel = new Vector2(vel.x, Mathf.Min(vel.y, 0)); // lose all upward momentum!
		GameManagers.Instance.EventManager.OnPlayerStartPlunge(this);
	}
	private void StopPlunge() {
		if (!isPlunging) { return; } // Not plunging? Do nothing.
		isPlunging = false;
		myBody.OnStopPlunge();
	}
	private void RechargePlunge() {
		isPlungeRecharged = true;
		myBody.OnRechargePlunge();
		GameManagers.Instance.EventManager.OnPlayerRechargePlunge(this);
	}

	private void StartWallSlide(int side) {
		wallSlideSide = side;
	}
	private void StopWallSlide() {
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
	private void OnUpPressed() {
		// We're on the ground and NOT timed out of jumping! Go!
		if (feetOnGround()) {//numJumpsSinceGround<MaxJumps && Time.time>=timeWhenCanJump
			GroundJump();
		}
		else if (isTouchingWall()) {// isWallSliding()
			WallKick();
		}
		else if (CanStartPlunge()) {
			StartPlunge();
		}
		else {
//			if (isPlunging) {
//				CancelPlunge();
//			}
			timeWhenDelayedJump = Time.time + DelayedJumpWindow;
		}
	}
	private void OnDownPressed() {
		if (CanStartPlunge()) {
			StartPlunge();
		}
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override protected void OnTouchSurface(int side, Collider2D surfaceCol) {
		base.OnTouchSurface(side, surfaceCol);
		isPreservingWallKickVel = false; // touching any surface immediately stops our wall-kick-vel preservation.

		Collidable collidable = surfaceCol.GetComponent<Collidable>();
		// Tell the collidable!
		collidable.OnPlayerTouchMe(this, side);

		// Do my own stuff!
		if (side == Sides.B) {
			OnFeetTouchSurface(collidable);
		}
		else {
			OnNonFeetTouchSurface(collidable);
		}
	}
	private void OnFeetTouchSurface(Collidable collidable) {
		numJumpsSinceGround = 0;

		bool doBounce = isPlunging || IsBouncyCollidable(collidable);

		// Bounce!
		if (doBounce) {
			BounceOffCollidable_Up(collidable);
		}
		// Land!
		else {
			LandOnCollidable(collidable);
		}
	}
	private void OnNonFeetTouchSurface(Collidable collidable) {
		// Enemy??
		Enemy enemy = collidable as Enemy;
		if (enemy != null && CanTakeDamage()) {
			OnCollideWithEnemy(enemy);
		}
//		else {
//			// Should I bounce or jump?
//			bool doBounce = IsBouncyCollidable(collidable);// && !IsDontBounceButtonHeld()
//			if (doBounce) {
//				BounceOffCollidable_Side(collidable);
//			}
//		}
	}

	private void BounceOffCollidable_Up(Collidable collidable) {
		// Bouncing up off a surface stops the plunge.
		StopPlunge();
		// Find how fast we have to move upward to restore our previous highest height, and set our vel to that!
		float distToRestore = Mathf.Max (0, maxYSinceGround-pos.y);
		distToRestore += 3.2f; // TEST! Give us MORE than we started with!
		float yVel = Mathf.Sqrt(2*-Gravity.y*distToRestore); // 0 = y^2 + 2*g*dist  ->  y = sqrt(2*g*dist)
		yVel += 0.025f; // Hack!! We're not getting all our height back exactly. Fudge it for now.
		vel = new Vector2(vel.x, yVel);
		// Inform the collidable!!
		if (collidable != null) {
			collidable.OnPlayerBounceOnMe(this);
		}
	}
	// TEST! This whole function is a controls experiment.
	private void BounceOffCollidable_Side(Collidable collidable) { // NOTE: We're probably gonna wanna denote WHICH sides of collidables are bouncy...
		timeLastWallKicked = Time.time;
		vel = new Vector2(-vel.x, Mathf.Max(vel.y, Mathf.Abs(vel.x)*2f+0.1f));
//		vel = new Vector2(-vel.x, JumpForce*0.7f);//vel.y+
		// Inform the collidable!!
		if (collidable != null) {
			collidable.OnPlayerBounceOnMe(this);
		}
	}
	private void LandOnCollidable(Collidable collidable) {
		// Landing on a surface stops the plunge.
		StopPlunge();
		groundedSincePlunge = true;
		// Finally reset maxYSinceGround.
		maxYSinceGround = pos.y;
		// Is this collidable refreshing? Recharge my plunge!
		if (collidable==null || collidable.DoRechargePlayer) {
			RechargePlunge();
		}
		// Do that delayed jump we planned?
		if (Time.time <= timeWhenDelayedJump) {
			GroundJump();
		}
//			else { // TEMP TEST!
//				vel = new Vector2(vel.x, -vel.y * 0.2f);
//				if (Mathf.Abs(vel.y) < 0.05f) {
//					vel = new Vector2(vel.x, 0);
//				}
//			}
	}

	private void OnCollideWithEnemy(Enemy enemy) {
		int dirToEnemy = MathUtils.Sign(enemy.Pos.x-pos.x, false);
		vel = new Vector2(-dirToEnemy*HitByEnemyVel.x, HitByEnemyVel.y);
		TakeDamage(1);
	}

//	private void OnTriggerExit2D(Collider2D col) {
//		Collidable collidable = col.GetComponent<Collidable>();
//		if (collidable != null) {
//
//		}
//	}

	override public void OnEnterLift() {
		base.OnEnterLift();
		if (isPlunging) {
			StopPlunge();
		}
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

	public void OnUseBattery() {
		RechargePlunge();
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