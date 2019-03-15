﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCharacter : Collidable {
	// Constants
	public const int NumSides = 4; // it's hip to be square.
	// Overrideables
	virtual protected int StartingHealth { get { return 1; } }

	virtual protected float FrictionAir { get { return 0.6f; } }
	virtual protected float FrictionGround { get { return 0.6f; } }
	virtual protected Vector2 Gravity { get { return new Vector2(0, -0.05f); } }

	virtual protected float MaxVelXAir { get { return 0.35f; } }
	virtual protected float MaxVelXGround { get { return 0.25f; } }
	virtual protected float MaxVelYUp { get { return 3; } }
	virtual protected float MaxVelYDown { get { return -3; } }

	// Components
	[SerializeField] private BoxCollider2D bodyCollider=null;
	[SerializeField] protected PlatformCharacterWhiskers myWhiskers=null;
	// Properties
	private bool isDead = false;
	protected int health; // we die when we hit 0. TODO: Make this private, and have Player and Enemy extend MY GetHit() function.
	protected Vector2 vel=Vector2.zero;
	private Vector2 size;
	protected float timeLastTouchedWall=Mathf.NegativeInfinity;
	protected int sideLastTouchedWall;

	// Getters (Public)
	public Vector2 Vel { get { return vel; } }
	public Vector2 Size { get { return size; } }
	public bool IsInLift { get; set; }

//	virtual public bool IsAffectedByLift() { return true; }
	public bool IsDead { get { return isDead; } }
	public bool feetOnGround() { return myWhiskers.OnSurface(Sides.B) && vel.y<=0; } // NOTE: We DON'T consider our feet on the ground if we're moving upwards!
	// Getters (Protected)
	protected bool IsInvincible { get { return StartingHealth < 0; } }
    public bool DoUpdate() { // If this is FALSE, I won't do Update nor FixedUpdate.
        return Time.timeScale > 0; // No time? No dice.
    }
    protected int sideTouchingWall() {
		if (myWhiskers.OnSurface(Sides.L)) { return -1; }
		if (myWhiskers.OnSurface(Sides.R)) { return  1; }
		return 0;
	}
	protected bool isTouchingWall() { return sideTouchingWall() != 0; }
	virtual protected float HorzMoveInputVelXDelta() {
		return 0;
	}
	protected Vector2 GetAppliedVel() {
		Vector2 av = vel;
		float distL = myWhiskers.SurfaceDistMin(Sides.L);
		float distR = myWhiskers.SurfaceDistMin(Sides.R);
		float distB = myWhiskers.SurfaceDistMin(Sides.B);
		float distT = myWhiskers.SurfaceDistMin(Sides.T);
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

	// Setters
	public void SetVel(Vector2 _vel) {
		vel = _vel;
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	virtual protected void Start () {
		health = StartingHealth;
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
		if (feetOnGround()) {
			SetVel(new Vector2(vel.x*FrictionGround, vel.y));
		}
		else {
			SetVel(new Vector2(vel.x*FrictionAir, vel.y));
		}
	}
	protected void AcceptHorzMoveInput() {
		vel += new Vector2(HorzMoveInputVelXDelta(), 0);
	}
	protected void ApplyTerminalVel() {
		float maxXVel = feetOnGround() ? MaxVelXGround : MaxVelXAir;
		float xVel = Mathf.Clamp(vel.x, -maxXVel,maxXVel);
		float yVel = Mathf.Clamp(vel.y, MaxVelYDown,MaxVelYUp);
		SetVel(new Vector2(xVel, yVel));
	}
	protected void UpdateTimeLastTouchedWall() {
		if (isTouchingWall()) {
			timeLastTouchedWall = Time.time;
		}
		if (sideTouchingWall() != 0) {
			sideLastTouchedWall = sideTouchingWall();
		}
	}

//	protected void UpdateOnSurfaces() {
//		for (int side=0; side<NumSides; side++) {
//			Collider2D surfaceCollider = myWhiskers.GetSurfaceTouching(side);
//			float sideSpeed = GetSideSpeed(side);
//			bool isTouching = surfaceCollider!=null && sideSpeed>=0; // I'm "touching" this surface if it exists and I'm NOT moving *away* from it!
//			onSurfaces[side] = isTouching;
////			if (onSurfaces[side] && !isTouching) {
////				OnLeaveSurface(side, surfaceCollider);
////			}
////			else if (!onSurfaces[side] && isTouching) {
////				OnTouchSurface(side, surfaceCollider);
////			}
//		}
//	}

	virtual public void OnWhiskersTouchCollider(int side, Collider2D col) {
		Collidable collidable = col.GetComponent<Collidable>();
		if (collidable != null) {
			collidable.OnCharacterTouchMe(side, this);
		}
	}
	virtual public void OnWhiskersLeaveCollider(int side, Collider2D col) {
		Collidable collidable = col.GetComponent<Collidable>();
		if (collidable != null) {
			collidable.OnCharacterLeaveMe(side, this);
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void ChangeVel(Vector2 delta) {
		vel += delta;
	}
	virtual protected void Die() {
		isDead = true;
		this.gameObject.SetActive(false); // TEMP super simple for now.
	}

	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	virtual public void OnEnterLift() {
		IsInLift = true;
	}
	virtual public void OnExitLift() {
		IsInLift = false;
	}


}


