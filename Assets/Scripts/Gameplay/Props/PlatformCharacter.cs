using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCharacter : Collidable {
	// Constants
	public const int NumSides = 4; // it's hip to be square.
	// Overrideables
	virtual protected int StartingHealth { get { return 1; } }

	virtual protected float FrictionAir { get { return 0.6f; } }
	virtual protected float FrictionGround { get { return 0.6f; } }
	virtual protected Vector2 Gravity { get { return new Vector2(0, -0.034f); } }

	virtual protected float MaxVelXAir { get { return 0.35f; } }
	virtual protected float MaxVelXGround { get { return 0.25f; } }
	virtual protected float MaxVelYUp { get { return 3; } }
	virtual protected float MaxVelYDown { get { return -3; } }
    virtual protected float GetCurrMaxVelX() {
        return IsGrounded() ? MaxVelXGround : MaxVelXAir;
    }
    
//  virtual public bool IsAffectedByLift() { return true; }

	// Components
	[SerializeField] private BoxCollider2D bodyCollider=null;
	[SerializeField] protected PlatformCharacterWhiskers myWhiskers=null;
	// Properties
	private bool isDead = false;
	protected int health { get; private set; } // we die when we hit 0.
    protected float timeLastTouchedWall=Mathf.NegativeInfinity;
    protected float timeSinceDamage { get; private set; } // set to Time.time when we take damage.
    public Vector2 Size { get; private set; } // Set in Start by the size of my collider!
    // References
    private List<Lift> liftsTouching = new List<Lift>();

    // Getters
    protected bool IsInLift { get; private set; }
	public bool IsDead { get { return isDead; } }
	public bool IsGrounded() { return myWhiskers.OnSurface(Sides.B) && vel.y<0.001f; } // NOTE: We DON'T consider our feet on the ground if we're moving upwards!
    protected bool IsAgainstWall() { return myWhiskers.IsAgainstWall(); }
    protected bool IsInvincible { get { return StartingHealth < 0; } }
    public bool DoUpdate() { // If this is FALSE, I won't do Update nor FixedUpdate.
        return Time.timeScale > 0; // No time? No dice.
    }
	virtual protected float HorzMoveInputVelXDelta() {
		return 0;
	}
	///** Returns the relative speed we're traveling in at this side. */
	//private float GetSideSpeed(int side) {
	//	switch (side) {
	//	case Sides.L: return -vel.x;
	//	case Sides.R: return  vel.x;
	//	case Sides.B: return -vel.y;
	//	case Sides.T: return  vel.y;
	//	default: Debug.LogError("Side not recognized: " + side); return 0;
	//	}
	//}
    private Collidable GetCollidable(Collider2D col) {
        return col==null ? null : col.GetComponent<Collidable>();
    }
    private Vector2 GetRelativeVel(Collider2D coll2D) {
        return GetRelativeVel(GetCollidable(coll2D));
    }
    private Vector2 GetRelativeVel(Collidable coll) {
        if (coll == null) { return vel; } // no collidable. Just return my vel.
        return vel - coll.vel; // Return my vel, relative to this collidable!
    }
    /** By default, we ignore colliders we're moving away from!
     * This prevents A) Registering contact when passing up thru a Platform, B) Whiskers-touching-2-things issues, like recharging plunge or cancelling preserving wall-kick vel. */
    virtual public bool IgnoreColl(int side, Collider2D coll) {
        return IsMovingAwayFromColl(side, coll);
    }
    protected bool IsMovingAwayFromColl(int side, Collider2D coll2D) {
        Vector2 velRel = GetRelativeVel(coll2D);
        return IsMovingAwayFromSide(side, velRel);
    }
    private bool IsMovingAwayFromSide(int side, Vector2 velRel) {
        switch (side) {
            case Sides.L: return velRel.x >  0.01f;
            case Sides.R: return velRel.x < -0.01f;
            case Sides.B: return velRel.y >  0.01f;
            case Sides.T: return velRel.y < -0.01f;
            default: return false; // Hmm.
        }
    }


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        base.Start();
        Size = bodyCollider.size + new Vector2(bodyCollider.edgeRadius, bodyCollider.edgeRadius)*2;
        timeSinceDamage = Mathf.NegativeInfinity;
		health = StartingHealth;
        IsInLift = false;
	}


	// ----------------------------------------------------------------
	//  FixedUpdate Methods
	// ----------------------------------------------------------------
	protected void ApplyVel() {
		if (vel != Vector2.zero) {
			Vector2 appliedVel = myWhiskers.GetAppliedVel();
			pos += appliedVel;
            //print("vel: " + vel.y + "    applied: " + appliedVel.y);
		}
	}
    virtual protected void ApplyVelFromSurfaces() {
        if (IsGrounded()) { // TODO: Do for ALL sides! Not just feet!
            Collidable obj = myWhiskers.TEMP_GetFloorCollidable();
            if (obj != null) {
                pos += obj.vel;
            }
        }
    }
    protected void ApplyGravity() {
		ChangeVel(Gravity);
	}
    virtual protected void ApplyInternalForces() {} // For Plunga's plunge-force, Jetta's jetting, etc.
	virtual protected void ApplyFriction() {
        if (IsInLift) {
			SetVel(new Vector2(vel.x*FrictionAir, vel.y));
        }
		else if (IsGrounded()) {
			SetVel(new Vector2(vel.x*FrictionGround, vel.y));
		}
		else {
			SetVel(new Vector2(vel.x*FrictionAir, vel.y));
		}
	}
	virtual protected void AcceptDirectionalMoveInput() {
		ChangeVel(new Vector2(HorzMoveInputVelXDelta(), 0));
	}
	protected void ApplyTerminalVel() {
        //if (IsInLift) { return; } // TEST
		float maxXVel = GetCurrMaxVelX();
		float xVel = Mathf.Clamp(vel.x, -maxXVel,maxXVel);
		float yVel = Mathf.Clamp(vel.y, MaxVelYDown,MaxVelYUp);
		SetVel(xVel, yVel);
	}
	protected void UpdateTimeLastTouchedWall() {
		if (IsAgainstWall()) {
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
		Collidable collidable = GetCollidable(col);
		if (collidable != null) {
			collidable.OnCharacterTouchMe(side, this);
		}
	}
	virtual public void OnWhiskersLeaveCollider(int side, Collider2D col) {
        Collidable collidable = GetCollidable(col);
		if (collidable != null) {
			collidable.OnCharacterLeaveMe(side, this);
            //// We're NOT moving away from it? Add its vel to OUR vel!
            //if (IsMovingAwayFromColl(side, collidable)) {
            //    ChangeVel(collidable.vel);
            //}
        }
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
    //public void SetVel(Vector2 _vel) { vel = _vel; }
	protected void ChangeVel(Vector2 delta) { SetVel(vel + delta); }
    protected void ChangeVel(float _x,float _y) { ChangeVel(new Vector2(_x,_y)); }
    
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
    override public PropData ToData() { return null; }


}


