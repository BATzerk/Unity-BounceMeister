using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PlatformCharacter {
	// Constants
    const int NumCoinsInMe = 3;
	override protected int StartingHealth { get { return 1; } }
    //override public Vector2 Size { get { return new Vector2(2f, 1.5f); } }

	override protected float FrictionAir { get { return 0.6f; } }
	override protected float FrictionGround { get { return 0.6f; } }
	override protected Vector2 Gravity { get { return new Vector2(0, -0.05f); } }

	private const float MovementSpeedX = 0.06f;
	// Properties
	[SerializeField] private float dirMoving = 1; // -1 or 1. I like to pace.

	// Getters (Overrides)
	override protected float HorzMoveInputVelXDelta() {
		return dirMoving*MovementSpeedX;
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	//override protected void Start () {
	//	base.Start();

	//	// Size me, queen!
	//	SetSize(new Vector2(2.5f, 2.5f));
	//}
    public void Initialize(Room _myRoom, EnemyData data) {
        base.BaseInitialize(_myRoom, data);
    }


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate () {
        if (!DoUpdate()) { return; }
        if (GameTimeController.IsRoomFrozen) { return; } // Time's frozen? Register NOTHING! Raor!
        
        Vector2 ppos = pos;

        ApplyVelFromSurfaces();
        ApplyFriction();
		ApplyGravity();
		AcceptDirectionalMoveInput();
        ApplyTerminalVel();
        ApplyLiftForces();
		myWhiskers.UpdateSurfaces(); // update these dependently now, so we guarantee most up-to-date info.
		ApplyVel();

		// Update vel to be the distance we ended up moving this frame.
		SetVel(pos - ppos);
	}



	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
//	override protected void OnTouchSurface(int side, Collider2D collider) {
//		base.OnTouchSurface(side, collider);
//
//		// A wall?? Reverse my horz direction!
//		if (side==Sides.L || side==Sides.R) {
//			dirMoving *= -1;
//		}
//	}
	override public void OnWhiskersTouchCollider(int side, Collider2D col) {
		base.OnWhiskersTouchCollider(side, col);

		// A wall?? Reverse my horz direction!
		if (side==Sides.L || side==Sides.R) {
			dirMoving *= -1;
		}
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
//	override public void OnCollideWithCollidable(Collidable collidable, int otherColSide) {
//		if (IsInvincible) { return; } // Invincible? Do nothin'.
//		// Other collidable's bottom hit me?
//		if (otherColSide == Sides.B) {
//			// Player?!
//			Player player = collidable as Player;
//			if (player != null) {
//				if (player.IsBouncing) {
//					if (player.Vel.y < -0.6f) {
//						GetHit();
//					}
//				}
//			}
//		}
//	}
	override public void OnPlayerFeetBounceOnMe(Player player) {
		if (IsInvincible) { return; } // Invincible? Do nothin'.
		TakeDamage(1);
	}
    override protected void Die() {
        base.Die();
        // Spit out COINS!
        for (int i=0; i<NumCoinsInMe; i++) {
            SpawnCoinInMe();
        }
    }

    private void SpawnCoinInMe() {
        Coin newCoin = Instantiate(ResourcesHandler.Instance.Coin).GetComponent<Coin>();
        newCoin.Initialize(MyRoom, this.transform.localPosition);
        //newCoin.transform.SetParent(this.transform.parent); // make its parent whatever mine is, too.
        //newCoin.transform.localPosition = this.transform.localPosition;
        //newCoin.transform.localPosition += new Vector3( // Put it randomly somewhere inside my box area (instead of putting them all exactly in the center).
            //Random.Range(-myCollider.size.x*0.3f, myCollider.size.x*0.3f) * this.transform.localScale.x,
            //Random.Range(-myCollider.size.y*0.3f, myCollider.size.y*0.3f) * this.transform.localScale.y);
    }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData ToData() {
        return new EnemyData {
            pos = pos
        };
    }



}

