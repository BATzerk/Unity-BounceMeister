using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCharacter : MonoBehaviour {
	// Constants
	public const int NumSides = 4; // it's hip to be square.
	// Overrideables
	virtual protected float FrictionAir { get { return 0.6f; } }
	virtual protected float FrictionGround { get { return 0.6f; } }
	virtual protected Vector2 Gravity { get { return new Vector2(0, -0.05f); } }
	// Components
	[SerializeField] private BoxCollider2D bodyCollider=null;
	[SerializeField] private PlatformCharacterWhiskers myWhiskers=null;
	// Properties
	protected Vector2 vel;
	private Vector2 size;
	protected bool[] onGrounds; // index is side.

	// Getters
	public Vector2 Pos { get { return pos; } }
	public Vector2 Vel { get { return vel; } }
	public Vector2 Size { get { return size; } }

	protected bool feetOnGround { get { return onGrounds[Sides.B]; } }
	protected PlatformCharacterWhiskers MyBaseWhiskers { get { return myWhiskers; } } // So my extensions can associate their own specific whiskers from this reference.
	protected Vector2 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
	virtual protected float HorzMoveInputVelXDelta() {
		return 0;
	}
	protected Vector2 GetAppliedVel() {
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
	//  Start
	// ----------------------------------------------------------------
	virtual protected void Start () {
		onGrounds = new bool[NumSides];
		vel = Vector2.zero;
	}
	virtual protected void SetSize(Vector2 _size) {
		this.size = _size;
		bodyCollider.size = _size;
	}


	// ----------------------------------------------------------------
	//  FixedUpdate Methods
	// ----------------------------------------------------------------
	protected void ApplyVel() {
		if (vel != Vector2.zero) {
			Vector2 appliedVel = GetAppliedVel();
			pos += appliedVel;
		}
	}
	protected void ApplyGravity() {
		vel += Gravity;
	}
	protected void ApplyFriction() {
		if (feetOnGround) {
			vel = new Vector2(vel.x*FrictionGround, vel.y);
		}
		else {
			vel = new Vector2(vel.x*FrictionAir, vel.y);
		}
	}
	protected void AcceptHorzMoveInput() {
		vel += new Vector2(HorzMoveInputVelXDelta(), 0);
	}

	protected void UpdateOnGrounds() {
		for (int side=0; side<NumSides; side++) {
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



	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	virtual protected void OnLeaveGround(int side) {
		onGrounds[side] = false;
	}
	virtual protected void OnTouchGround(int side, Collider2D groundCol) {
		onGrounds[side] = true;

		// Inform the ground!
		//		Collidable collidable = groundCol.GetComponent<Collidable>();
		//		if (collidable != null) {
		//			collidable.OnCollideWithPlayer(this);
		//		}
	}


}


