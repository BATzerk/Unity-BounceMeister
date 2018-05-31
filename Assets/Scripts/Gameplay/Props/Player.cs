using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlatformCharacter {
	// Constants
	override protected float FrictionAir { get { return 0.6f; } }
	override protected float FrictionGround { get { return 0.6f; } }
	override protected Vector2 Gravity { get { return new Vector2(0, -0.05f); } }
	private const float InputScaleX = 0.5f;
	private const float MaxVelX = 0.4f;
	private const float MaxVelYUp = 4;
	private const float MaxVelYDown = -4;
	private const float JumpForce = 0.8f;
	private const float DelayedJumpWindow = 0.15f; // in SECONDS. The time window where we can press jump just BEFORE landing, and still jump when we land.
	private const float JumpTimeoutWindow = 0.0f; // Note: disabled. in SECONDS. Don't allow jumping twice this quickly.
	// Properties
	private bool isBouncing=false;
	private float maxYSinceGround; // the highest we got since we last made ground contact. Used to determine bounce vel!
	private float timeWhenDelayedJump; // set when we're in the air and press Jump. If we touch ground before this time, we'll do a delayed jump!
	private float timeWhenCanJump; // set to Time.time + JumpTimeoutWindow when we jump.
	private int numJumpsSinceGround;
	// Components
	[SerializeField] private PlayerBody myBody=null;
	private PlayerWhiskers myPlayerWhiskers; // defined in Start, from my inhereted serialized whiskers.

	// Getters (Public)
	public bool IsBouncing { get { return isBouncing; } }
	// Getters (Overrides)
	override protected float HorzMoveInputVelXDelta() {
		if (InputController.Instance==null) { return 0; } // for building at runtime.
		if (inputAxis.x == 0) { return 0; }
		float dirX = MathUtils.Sign(inputAxis.x);
		float mult = feetOnGround ? 1 : 0.65f;

		return dirX*InputScaleX * mult;
	}
	// Getters (Private)
	private Vector2 inputAxis { get { return InputController.Instance.PlayerInput; } }
	private bool IsBouncyCollider(Collider2D collider) {
		if (!isBouncing) { return false; } // If I'm not bouncing at ALL, return false. :)
		Ground ground = collider.GetComponent<Ground>();
		if (ground != null) {
			return ground.IsBouncy;
		}
		return false; // Nah, it's not a Ground at all. Don't bounce by default.
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();
		myPlayerWhiskers = MyBaseWhiskers as PlayerWhiskers;

		// Size me, queen!
		SetSize (new Vector2(2.5f, 2.5f)); // NOTE: I don't understand why we gotta cut it by 100x. :P
	}
	override protected void SetSize(Vector2 _size) {
		base.SetSize(_size);
		myBody.SetSize(_size);
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		if (Time.timeScale == 0) { return; } // No time? No dice.

		AcceptJumpInput();
	}
	private void AcceptJumpInput() {
		if (Input.GetKeyDown(KeyCode.UpArrow)) { // TEMP hardcoded
			OnJumpPressed();
		}
		// TEMP! todo: Use pinputAxis within InputController in a *FixedUpdate* loop to determine if we've just pushed up/down.
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			OnUpPressed();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			OnDownPressed();
		}
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate () {
		if (Time.timeScale == 0) { return; } // No time? No dice.
		Vector2 ppos = pos;

		UpdateOnGrounds();
		ApplyFriction();
		ApplyGravity();
		AcceptHorzMoveInput();
		ApplyTerminalVel();
		myPlayerWhiskers.UpdateGroundDists(); // update these dependently now, so we guarantee most up-to-date info.
		ApplyVel();
		UpdateMaxYSinceGround();

		// Update vel to be the distance we ended up moving this frame.
		vel = pos - ppos;
	}

	private void ApplyTerminalVel() {
		vel = new Vector2(Mathf.Clamp(vel.x, -MaxVelX,MaxVelX), Mathf.Clamp(vel.y, MaxVelYDown,MaxVelYUp));
	}
	private void UpdateMaxYSinceGround() {
		maxYSinceGround = Mathf.Max(maxYSinceGround, pos.y);
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void Jump() {
		vel = new Vector2(vel.x, JumpForce);
		timeWhenDelayedJump = -1; // reset this just in case.
		timeWhenCanJump = Time.time + JumpTimeoutWindow;
		numJumpsSinceGround ++;
		GameManagers.Instance.EventManager.OnPlayerJump(this);
//		OnLeaveGround(); // Call this manually now!
	}
	private void StartBouncing() {
		if (isBouncing) { return; } // Already bouncing? Do nothing.
		isBouncing = true;
		myBody.OnStartBouncing();
		if (feetOnGround) { // slipped this in as a test: if we're on the ground when we say we wanna bounce, jump us up so we start actually bouncing!
			Jump();
		}
	}
	private void StopBouncing() {
		if (!isBouncing) { return; } // Already not bouncing? Do nothing.
		isBouncing = false;
		myBody.OnStopBouncing();
	}

	private void BounceOffGround() {
		// Find how fast we have to move upward to restore our previous highest height, and set our vel to that!
		float distToRestore = Mathf.Max (0, maxYSinceGround-pos.y);
		float yVel = Mathf.Sqrt(2*-Gravity.y*distToRestore); // 0 = y^2 + 2*g*dist  ->  y = sqrt(2*g*dist)
		yVel += 0.025f; // Hack!! We're not getting all our height back exactly. Fudge it for now.
		vel = new Vector2(vel.x, yVel);
//		OnLeaveGround(); // Call this manually now!
	}


	// ----------------------------------------------------------------
	//  Events (Input)
	// ----------------------------------------------------------------
	private void OnJumpPressed() {
		// We're on the ground and NOT timed out of jumping! Go!
		if (feetOnGround && Time.time>=timeWhenCanJump) {//numJumpsSinceGround<MaxJumps
			Jump();
		}
		else {
			timeWhenDelayedJump = Time.time + DelayedJumpWindow;
		}
	}
	private void OnUpPressed() {
		if (isBouncing) {
			StopBouncing();
		}
	}
	private void OnDownPressed() {
		if (!isBouncing) {
			StartBouncing();
		}
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override protected void OnLeaveGround(int side) {
		base.OnLeaveGround(side);
	}
	override protected void OnTouchGround(int side, Collider2D groundCol) {
		base.OnTouchGround(side, groundCol);
		numJumpsSinceGround = 0;

		// Inform the ground!
		Collidable collidable = groundCol.GetComponent<Collidable>();
		if (collidable != null) {
			collidable.OnCollideWithPlayer(this);
		}

		// Should I bounce or jump?
		bool doBounce = IsBouncyCollider(groundCol);
		if (doBounce) {
			BounceOffGround();
		}
		else {
			// DON'T bounce? Then stop bouncing right away.
			StopBouncing();
			// Do that delayed jump we planned?
			if (Time.time <= timeWhenDelayedJump) {
				Jump();
			}
		}


		// Finally reset maxYSinceGround.
		maxYSinceGround = pos.y;
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