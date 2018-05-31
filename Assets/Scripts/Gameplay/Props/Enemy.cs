using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PlatformCharacter {
	// Constants
	override protected float FrictionAir { get { return 0.6f; } }
	override protected float FrictionGround { get { return 0.6f; } }
	override protected Vector2 Gravity { get { return new Vector2(0, -0.05f); } }
	private const float MovementSpeedX = 0.08f;
	// Properties
	private float dirMoving; // -1 or 1. I like to pace.
	// Components
	private EnemyWhiskers myEnemyWhiskers; // defined in Start, from my inhereted serialized whiskers.

	// Getters (Overrides)
	override protected float HorzMoveInputVelXDelta() {
		return dirMoving*MovementSpeedX;
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();
		myEnemyWhiskers = MyBaseWhiskers as EnemyWhiskers;

		dirMoving = 1;

		// Size me, queen!
		SetSize (new Vector2(2.5f, 2.5f));
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
		myEnemyWhiskers.UpdateGroundDists(); // update these dependently now, so we guarantee most up-to-date info.
		ApplyVel();

		// Update vel to be the distance we ended up moving this frame.
		vel = pos - ppos;
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override protected void OnLeaveGround(int side) {
		base.OnLeaveGround(side);
	}
	override protected void OnTouchGround(int side, Collider2D groundCol) {
		base.OnTouchGround(side, groundCol);

		// A wall?? Reverse my horz direction!
		if (side==Sides.L || side==Sides.R) {
			dirMoving *= -1;
		}
	}



}

