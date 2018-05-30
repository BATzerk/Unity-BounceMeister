using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	// Constants
	private const float InputScaleX = 0.5f;
	private const float MaxVelX = 0.4f;
	private const float MaxVelYUp = 4;
	private const float MaxVelYDown = -4;
	private const float JumpForce = 0.8f;
	private const float DelayedJumpWindow = 0.15f; // in SECONDS. The time window where we can press jump just BEFORE landing, and still jump when we land.
	private const float JumpTimeoutWindow = 0.0f; // Note: disabled. in SECONDS. Don't allow jumping twice this quickly.
	private const int MaxJumps = 1; // change to 2 for a double-jump.
	private Vector2 gravity = new Vector2(0, -0.05f);
	// Properties
	private bool onGround=false;
	private Color bodyColorNeutral;
	private float timeWhenDelayedJump; // set when we're in the air and press Jump. If we touch ground before this time, we'll do a delayed jump!
	private float timeWhenCanJump; // set to Time.time + JumpTimeoutWindow when we jump.
	private int numJumpsSinceGround;
	private Vector2 vel;
	private Vector2 size;
	// Components
	[SerializeField] private PlayerBody myBody=null;
	[SerializeField] private PlayerWhiskers myWhiskers=null;
	[SerializeField] private BoxCollider2D bodyCollider=null;

	// Getters/Setters
	public Vector2 Pos { get { return pos; } }
	public Vector2 Vel { get { return vel; } }
	public Vector2 Size { get { return size; } }
	public bool OnGround { get { return onGround; } }

	private Vector2 inputAxis { get { return InputController.Instance.PlayerInput; } }
	private Vector2 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
	private Vector2 GetAppliedVel() {
		Vector2 av = vel;
		float distL = myWhiskers.GroundDistMin(Sides.L);
		float distR = myWhiskers.GroundDistMin(Sides.R);
		float distB = myWhiskers.GroundDistMin(Sides.B);
		float distT = myWhiskers.GroundDistMin(Sides.T);
		// Clamp our vel so we don't intersect anything.
		if (vel.x<0 && vel.x<-distL) {
			av = new Vector2(-distL, av.y);
		}
		else if (vel.x>0 && vel.x>distR) {
			av = new Vector2(distR, av.y);
		}
		if (vel.y<0 && vel.y<-distB) {
			av = new Vector2(av.x, -distB);
		}
		else if (vel.y>0 && vel.y>distT) {
			av = new Vector2(av.x, distT);
		}
		return av;
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start () {
		// Size me, queen!
		SetSize (new Vector2(2.5f, 2.5f)); // NOTE: I don't understand why we gotta cut it by 100x. :P

		vel = Vector2.zero;
	}
	private void SetSize(Vector2 _size) {
		this.size = _size;
		myBody.SetSize(this.size);
		bodyCollider.size = _size;
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		if (Time.timeScale == 0) { return; } // No time? No dice.

		AcceptJumpInput();
	}
	private void AcceptJumpInput() {
		if (Input.GetKeyDown(KeyCode.Space)) { // TEMP hardcoded
			OnJumpPressed();
		}
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate () {
		if (Time.timeScale == 0) { return; } // No time? No dice.
		Vector2 ppos = pos;

		UpdateOnGround();
		ApplyFriction();
		ApplyGravity();
		AcceptMoveInput();
		ApplyTerminalVel();
		myWhiskers.UpdateGroundDists(); // update these dependently now, so we guarantee most up-to-date info.
		ApplyVel();

		// Update vel to be the distance we ended up moving this frame.
		vel = pos - ppos;
	}

	private void ApplyGravity() {
		vel += gravity;
	}
	private void AcceptMoveInput() {
		if (InputController.Instance==null) { return; } // for building at runtime.

		// Horizontal!
		if (inputAxis.x != 0) {
			float moveX = MathUtils.Sign(inputAxis.x);
			float mult = onGround ? 1 : 0.65f;

			float velXDelta = moveX*InputScaleX * mult;
			vel += new Vector2(velXDelta, 0);
		}
		else {
			vel = new Vector2(vel.x*0.8f, vel.y); // TEST
		}
	}
	private void ApplyFriction() {
		if (onGround) {
			vel = new Vector2(vel.x*0.8f, vel.y);
		}
		else {
			vel = new Vector2(vel.x*0.99f, vel.y);
		}
	}
	private void ApplyTerminalVel() {
		vel = new Vector2(Mathf.Clamp(vel.x, -MaxVelX,MaxVelX), Mathf.Clamp(vel.y, MaxVelYDown,MaxVelYUp));
	}
	private void ApplyVel() {
		if (vel != Vector2.zero) {
			Vector2 appliedVel = GetAppliedVel();
			pos += appliedVel;
		}
	}

	private void UpdateOnGround() {
		bool _onGround = myWhiskers.GetOnGround();
		if (onGround && !_onGround) {
			OnLeaveGround();
		}
		else if (!onGround && _onGround) {
			OnTouchGround();
		}
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
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnJumpPressed() {
		// We're on the ground and NOT timed out of jumping! Go!
		if (numJumpsSinceGround<MaxJumps && Time.time>=timeWhenCanJump) {
			Jump();
		}
		else {
			timeWhenDelayedJump = Time.time + DelayedJumpWindow;
		}
	}

	private void OnLeaveGround() {
		onGround = false;
	}
	private void OnTouchGround() {
		onGround = true;
		numJumpsSinceGround = 0;
		if (Time.time <= timeWhenDelayedJump) {
			Jump();
		}
//		// Are we doing a legit bounce?
//		BoxCollider2D groundCollider = groundGO.GetComponent<BoxCollider2D>();
//		float myYBottom = pos.y + bodyCollider.offset.y - bodyCollider.size.y*0.5f;
//		float groundYTop = groundGO.transform.localPosition.y + groundCollider.offset.y + groundCollider.size.y*0.5f;
//		if (myYBottom+0.2f >= groundYTop) { // am I almost ABOVE the ground??
//			// Give me a MINIMUM bounce vel!
//			float frictionX = 0.5f;
//			vel = new Vector2(vel.x*frictionX, Mathf.Max(Mathf.Abs(vel.y), MIN_Y_BOUNCE));
//		}
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