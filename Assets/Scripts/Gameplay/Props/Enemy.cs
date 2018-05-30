using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
	// Constants
	private const float FrictionAir = 0.6f;
	private const float FrictionGround = 0.6f;
	private const float InputScaleX = 0.08f;
	private Vector2 gravity = new Vector2(0, -0.05f);
	// Properties
	private bool[] onGrounds; // index is side.
	private Vector2 vel;
	private Vector2 size;
	private Vector2 inputAxis; // QUICK IMPLEMENTATION for getting these dudies to movies.
	// Components
//	[SerializeField] private PlayerBody myBody=null;
	[SerializeField] private EnemyWhiskers myWhiskers=null;
	[SerializeField] private BoxCollider2D bodyCollider=null;

	// Getters/Setters
	public Vector2 Pos { get { return pos; } }
	public Vector2 Vel { get { return vel; } }
	public Vector2 Size { get { return size; } }
//	public bool OnGround { get { return onGround; } }

	private bool feetOnGround { get { return onGrounds[Sides.B]; } }
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
		onGrounds = new bool[4];

		inputAxis = new Vector2(1f, 0);

		// Size me, queen!
		SetSize (new Vector2(2.5f, 2.5f)); // NOTE: I don't understand why we gotta cut it by 100x. :P

		vel = Vector2.zero;
	}
	private void SetSize(Vector2 _size) {
		this.size = _size;
//		myBody.SetSize(this.size);
		bodyCollider.size = _size;
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
		AcceptDirectionalInput();
		myWhiskers.UpdateGroundDists(); // update these dependently now, so we guarantee most up-to-date info.
		ApplyVel();

		// Update vel to be the distance we ended up moving this frame.
		vel = pos - ppos;
	}

	private void ApplyGravity() {
		vel += gravity;
	}
	private void AcceptDirectionalInput() {
		if (InputController.Instance==null) { return; } // for building at runtime.

		// Moving horizontally!
		if (inputAxis.x != 0) {
			float velXDelta = inputAxis.x*InputScaleX;
			vel += new Vector2(velXDelta, 0);
		}
	}
	private void ApplyFriction() {
		if (feetOnGround) {
			vel = new Vector2(vel.x*FrictionGround, vel.y);
		}
		else {
			vel = new Vector2(vel.x*FrictionAir, vel.y);
		}
	}
	private void ApplyVel() {
		if (vel != Vector2.zero) {
			Vector2 appliedVel = GetAppliedVel();
			pos += appliedVel;
		}
	}

	private void UpdateOnGrounds() {
		for (int side=0; side<EnemyWhiskers.NumSides; side++) {
			Collider2D ground = myWhiskers.GetGroundTouching(side);
			float sideSpeed = GetSideSpeed(side);
			bool touchingGround = ground!=null;//QQQ test && sideSpeed>=0; // I'm "touching" this ground if it exists and I'm not moving *away* from it!
			if (onGrounds[side] && !touchingGround) {
				OnLeaveGround(side);
			}
			else if (!onGrounds[side] && touchingGround) {
				OnTouchGround(side, ground);
			}
		}
	}
	/** Returns the relative speed we're traveling in at this side. */
	private float GetSideSpeed(int side) {
		switch (side) {
		case Sides.L: return -vel.x;
		case Sides.R: return  vel.x;
		case Sides.B: return -vel.y;
		case Sides.T: return  vel.y;
		default: Debug.LogError("Side not recognized: " + side); return 0;
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
//	private void Jump() {
//		vel = new Vector2(vel.x, JumpForce);
//	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	private void OnLeaveGround(int side) {
		onGrounds[side] = false;
	}
	private void OnTouchGround(int side, Collider2D groundCol) {
		onGrounds[side] = true;

		// A wall?? Reverse my horz direction!
		if (side==Sides.L || side==Sides.R) {
			inputAxis = new Vector2(-inputAxis.x, inputAxis.y);
		}

		// Inform the ground!
//		Collidable collidable = groundCol.GetComponent<Collidable>();
//		if (collidable != null) {
//			collidable.OnCollideWithPlayer(this);
//		}
	}



}

