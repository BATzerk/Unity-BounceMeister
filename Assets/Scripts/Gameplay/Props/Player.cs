using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlatformCharacter {
	// Constants
	override protected float FrictionAir { get { return 1f; } }
	override protected float FrictionGround { get { return 0.7f; } }
	override protected Vector2 Gravity { get { return new Vector2(0, -0.042f); } }

	private const float InputScaleX = 0.07f;
	private const float JumpForce = 0.62f;
	private const float WallSlideMinYVel = -0.14f;
	private readonly Vector2 WallJumpVel = new Vector2(3f, 0.52f);
	private readonly Vector2 HitByEnemyVel = new Vector2(4f, 0.5f);

	private const float MaxVelX = 0.3f;
	private const float MaxVelYUp = 3;
	private const float MaxVelYDown = -3;

	private const float DelayedJumpWindow = 0.15f; // in SECONDS. The time window where we can press jump just BEFORE landing, and still jump when we land.
	private const float PostDamageImmunityDuration = 1.2f; // in SECONDS.
	private const float PostWallJumpHorzInputLockDur = 0.14f; // how long until we can provide horizontal input after jumping off a wall.

	// Properties
	private bool isBouncing = false;
	private bool isBounceRecharged = true;
	private bool isPostDamageImmunity = false;
	private float maxYSinceGround=Mathf.NegativeInfinity; // the highest we got since we last made ground contact. Used to determine bounce vel!
	private float timeLastWallJumped;
	private float timeSinceDamage; // invincible until this time! Set to Time.time + PostDamageInvincibleDuration when we're hit.
	private float timeWhenDelayedJump; // set when we're in the air and press Jump. If we touch ground before this time, we'll do a delayed jump!
	private int health = 1; // we die when we hit 0.
	private int numJumpsSinceGround;
//	private int maxHealth = 2;
	// Components
	[SerializeField] private PlayerBody myBody=null;

	// Getters (Public)
	public bool IsBouncing { get { return isBouncing; } }
	public bool IsPostDamageImmunity { get { return isPostDamageImmunity; } }
	// Getters (Overrides)
	override protected float HorzMoveInputVelXDelta() {
		if (InputController.Instance==null) { return 0; } // for building at runtime.
		if (inputAxis.x == 0) { return 0; }
		float dirX = MathUtils.Sign(inputAxis.x);
		float mult = feetOnGround() ? 1 : 0.65f;
		if (Time.time < timeLastWallJumped+PostWallJumpHorzInputLockDur) {
			mult = 0;
		}

		return dirX*InputScaleX * mult;
	}
	// Getters (Private)
	private bool CanTakeDamage() {
		return !isPostDamageImmunity;
	}
	private bool CanStartBounce() {
		return isBounceRecharged;
	}
	private bool IsDontBounceButtonHeld() { return Input.GetKey(KeyCode.DownArrow); }
	private Vector2 inputAxis { get { return InputController.Instance.PlayerInput; } }
	private bool IsBouncyCollidable(Collidable collidable) {
		if (collidable == null) { return false; } // The collidable is undefined? Default to NOT bouncy.
		return collidable.IsBouncy;
//		if (!isBouncing) { return false; } // If I'm not bouncing at ALL, return false. :)
//		Ground ground = collider.GetComponent<Ground>();
//		if (ground != null) {
//			return ground.IsBouncy;
//		}
//		return false; // Nah, it's not a Ground at all. Don't bounce by default.
	}

	private bool isWallSliding() { return wallSlideSide!=0; }
	private int wallSlideSide {
		get {
			if (isBouncing) { return 0; } // No wall-sliding if I'm bouncing, ok?
			if (!feetOnGround()) { // If my feet AREN'T on the ground...!
				if (onSurfaces[Sides.L]) { return -1; }
				if (onSurfaces[Sides.R]) { return  1; }
			}
			return 0; // Nah, not wall-sliding.
		}
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();

		// Reset some things.
		SetPos(pos);
		timeSinceDamage = -1;
		isPostDamageImmunity = false;

		// Size me, queen!
		SetSize (new Vector2(2f, 2f)); // NOTE: I don't understand why we gotta cut it by 100x. :P
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
//		else if (Input.GetKeyDown(KeyCode.DownArrow)) {
//			OnDownPressed();
//		}
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
		// HACK temp
		if (isBouncing) { vel += Gravity*0.8f; } // If we're going down, make us go down faster!
		AcceptHorzMoveInput();
		ApplyTerminalVel();
		myWhiskers.UpdateSurfaceDists(); // update these dependently now, so we guarantee most up-to-date info.
		UpdateWallSlide();
		ApplyVel();
		UpdateMaxYSinceGround();
		// HACK temp
//		if (!feetOnGround() && !isBouncing && vel.y<-0.5f) {
//			StartBouncing();
//		}

		// Update vel to be the distance we ended up moving this frame.
		vel = pos - ppos;
	}

	private void ApplyTerminalVel() {
		vel = new Vector2(Mathf.Clamp(vel.x, -MaxVelX,MaxVelX), Mathf.Clamp(vel.y, MaxVelYDown,MaxVelYUp));
	}
	private void UpdateMaxYSinceGround() {
		maxYSinceGround = Mathf.Max(maxYSinceGround, pos.y);
	}
	private void UpdateWallSlide() {
		if (isWallSliding()) {
			vel = new Vector2(vel.x, Mathf.Max(vel.y, WallSlideMinYVel)); // Give us a minimum yVel!
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
	private void WallJump() {
		vel = new Vector2(-wallSlideSide*WallJumpVel.x, WallJumpVel.y);
		timeLastWallJumped = Time.time;
		numJumpsSinceGround ++;
		GameManagers.Instance.EventManager.OnPlayerJump(this);
	}
	private void StartBouncing() {
		if (isBouncing) { return; } // Already bouncing? Do nothing.
		isBouncing = true;
		myBody.OnStartBouncing();
//		// If we're on the ground when we say we wanna bounce, jump us up so we start actually bouncing!
//		if (feetOnGround) {
//			vel = new Vector2(vel.x, Mathf.Max(vel.y, JumpForce));
//		}
	}
	private void StopBouncing() {
		if (!isBouncing) { return; } // Already not bouncing? Do nothing.
		isBouncing = false;
		myBody.OnStopBouncing();
	}

	private void BounceOffCollidable_Up(Collidable collidable) {
//		StartBouncing(); // Make sure we're bouncing!
		// Find how fast we have to move upward to restore our previous highest height, and set our vel to that!
		float distToRestore = Mathf.Max (0, maxYSinceGround-pos.y);
		distToRestore += 3.2f; // TEST! Give us MORE than we started with!
		float yVel = Mathf.Sqrt(2*-Gravity.y*distToRestore); // 0 = y^2 + 2*g*dist  ->  y = sqrt(2*g*dist)
		yVel += 0.025f; // Hack!! We're not getting all our height back exactly. Fudge it for now.
		vel = new Vector2(vel.x, yVel);
		// Consider us done bouncing! TEST.
		SpendBounce();
		// Inform the collidable if it exists!!
		if (collidable != null) {
			collidable.OnPlayerBounceOnMe(this);
		}
	}
	// TEST! This whole function is a controls experiment.
	private void BounceOffCollidable_Side(Collidable collidable) { // NOTE: We're probably gonna wanna denote WHICH sides of collidables are bouncy...
		StartBouncing(); // Make sure we're bouncing!
		timeLastWallJumped = Time.time;
		vel = new Vector2(-vel.x, Mathf.Max(vel.y, Mathf.Abs(vel.x)*2f+0.1f));
//		vel = new Vector2(-vel.x, JumpForce*0.7f);//vel.y+
		// Inform the collidable if it exists!!
		if (collidable != null) {
			collidable.OnPlayerBounceOnMe(this);
		}
	}

	private void SpendBounce() {
		StopBouncing();
		isBounceRecharged = false; // we need to wait until we're recharged to bounce again.
		myBody.OnSpendBounce();
	}
	private void RechargeBounce() {
		isBounceRecharged = true;
		myBody.OnRechargeBounce();
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
		else if (isWallSliding()) {
			WallJump();
		}
		else if (CanStartBounce()) {
			StartBouncing();
		}
		else {
//			if (isBouncing) {
//				StopBouncing();
//			}
			timeWhenDelayedJump = Time.time + DelayedJumpWindow;
		}
	}
//	private void OnDownPressed() {
//		if (!isBouncing) {
//			StartBouncing();
//		}
//	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override protected void OnTouchSurface(int side, Collider2D surfaceCol) {
		base.OnTouchSurface(side, surfaceCol);

		Collidable collidable = surfaceCol.GetComponent<Collidable>();
		if (side == Sides.B) {
			OnFeetTouchSurface(collidable);
		}
		else {
			OnNonFeetTouchSurface(collidable);
		}

//		// Inform the collidable!
//		if (collidable != null) {
//			collidable.OnCollideWithCollidable(this, side);
//		}
	}
	private void OnFeetTouchSurface(Collidable collidable) {
		numJumpsSinceGround = 0;

		// Should I bounce or jump?
//		bool doBounce = IsBouncyCollidable(collidable)
//						&& !IsDontBounceButtonHeld()
//						&& isBouncing;
		bool doBounce = isBouncing;
		if (doBounce) {
			BounceOffCollidable_Up(collidable);
		}
		else {
			// DON'T bounce? Then stop bouncing right away.
			StopBouncing();
			// TEST. Consider us recharged now!
			RechargeBounce();
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

		// Finally reset maxYSinceGround.
		maxYSinceGround = pos.y;
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

	private void OnCollideWithEnemy(Enemy enemy) {
		int dirToEnemy = MathUtils.Sign(enemy.Pos.x-pos.x, false);
		vel = new Vector2(-dirToEnemy*HitByEnemyVel.x, HitByEnemyVel.y);
		TakeDamage(1);
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