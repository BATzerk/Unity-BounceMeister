﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCharacter : Collidable {
	// Constants
	public const int NumSides = 4; // it's hip to be square.
	// Overrideables
	virtual protected int StartingHealth { get { return 1; } }
    virtual public Vector2 Size { get { return new Vector2(1.5f, 1.8f); } }

	virtual protected float FrictionAir { get { return 0.6f; } }
	virtual protected float FrictionGround { get { return 0.6f; } }
	virtual protected Vector2 Gravity { get { return new Vector2(0, -0.042f); } }

	virtual protected float MaxVelXAir { get { return 0.35f; } }
	virtual protected float MaxVelXGround { get { return 0.25f; } }
	virtual protected float MaxVelYUp { get { return 3; } }
	virtual protected float MaxVelYDown { get { return -3; } }

	// Components
	[SerializeField] private BoxCollider2D bodyCollider=null;
	[SerializeField] protected PlatformCharacterWhiskers myWhiskers=null;
	// Properties
	private bool isDead = false;
	protected int health { get; private set; } // we die when we hit 0.
    protected float timeLastTouchedWall=Mathf.NegativeInfinity;
    protected float timeSinceDamage { get; private set; } // set to Time.time when we take damage.
    // References
    private List<Lift> liftsTouching = new List<Lift>();

    // Getters
    protected bool IsInLift { get; private set; }
//	virtual public bool IsAffectedByLift() { return true; }
	public bool IsDead { get { return isDead; } }
	public bool feetOnGround() { return myWhiskers.OnSurface(Sides.B) && vel.y<0.001f; } // NOTE: We DON'T consider our feet on the ground if we're moving upwards!
	// Getters (Protected)
    protected bool IsInvincible { get { return StartingHealth < 0; } }
    public bool DoUpdate() { // If this is FALSE, I won't do Update nor FixedUpdate.
        return Time.timeScale > 0; // No time? No dice.
    }
	protected bool isTouchingWall() { return myWhiskers.DirTouchingWall() != 0; }
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
    public bool IsMovingAwayFromSide(int side) {
        switch (side) {
            case Sides.L: return vel.x >  0.01f;
            case Sides.R: return vel.x < -0.01f;
            case Sides.B: return vel.y >  0.01f;
            case Sides.T: return vel.y < -0.01f;
            default: return false; // Hmm.
        }
    }

    // Setters
    public void SetVel(Vector2 _vel) {
        vel = _vel;
    }


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        base.Start();
        timeSinceDamage = Mathf.NegativeInfinity;
		health = StartingHealth;
        bodyCollider.size = Size;
        IsInLift = false;
	}
	//virtual protected void SetSize(Vector2 _size) {
	//	this.Size = _size;
	//}


	// ----------------------------------------------------------------
	//  FixedUpdate Methods
	// ----------------------------------------------------------------
	protected void ApplyVel() {
		if (vel != Vector2.zero) {
			Vector2 appliedVel = GetAppliedVel();
			pos += appliedVel;
		}
	}
    protected void ApplyVelFromFloor() {
        if (feetOnGround()) {
            Collidable c = myWhiskers.TEMP_GetFloorCollidable();
            if (c != null) {
                pos += c.vel*0.5f;//*0.5f is HACK! TEMP! TODO: If we like TravelingPlatforms, then improve. Find a way to move Character on a Platform appropriately (it's a neat little challenge).
            }
        }
    }
    protected void ApplyGravity() {
		vel += Gravity;
	}
    virtual protected void ApplyInternalForces() {} // For Plunga's plunge-force, Jetta's jetting, etc.
	protected void ApplyFriction() {
        if (IsInLift) {
			SetVel(new Vector2(vel.x*FrictionAir, vel.y));
        }
		else if (feetOnGround()) {
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
        //if (IsInLift) { return; } // TEST
		float maxXVel = feetOnGround() ? MaxVelXGround : MaxVelXAir;
		float xVel = Mathf.Clamp(vel.x, -maxXVel,maxXVel);
		float yVel = Mathf.Clamp(vel.y, MaxVelYDown,MaxVelYUp);
		SetVel(new Vector2(xVel, yVel));
	}
	protected void UpdateTimeLastTouchedWall() {
		if (isTouchingWall()) {
			timeLastTouchedWall = Time.time;
		}
	}
    protected void ApplyLiftForces() {
        for (int i=0; i<liftsTouching.Count; i++) {
            ChangeVel(liftsTouching[i].Force);
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
	private void ChangeVel(Vector2 delta) {
		vel += delta;
	}
    virtual protected void TakeDamage(int damageAmount) {
        health -= damageAmount;
        timeSinceDamage = Time.time;
        // Am I kaput??
        if (health <= 0) {
            Die();
        }
    }
	virtual protected void Die() {
		isDead = true;
		this.gameObject.SetActive(false); // TEMP super simple for now.
	}

	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	virtual public void OnEnterLift(Lift lift) {
		liftsTouching.Add(lift);
        if (liftsTouching.Count == 1) { OnStartIsInLift(); } // This is the first Lift? Call event!
	}
	virtual public void OnExitLift(Lift lift) {
		liftsTouching.Remove(lift);
        if (liftsTouching.Count == 0) { OnEndIsInLift(); } // This was the last Lift? Call event!
	}
    virtual protected void OnStartIsInLift() {
        IsInLift = true;
    }
    virtual protected void OnEndIsInLift() {
        IsInLift = false;
    }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() { return null; }

}


