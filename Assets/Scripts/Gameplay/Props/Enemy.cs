﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PlatformCharacter {
	// Constants
	override protected float FrictionAir { get { return 0.6f; } }
	override protected float FrictionGround { get { return 0.6f; } }
	override protected Vector2 Gravity { get { return new Vector2(0, -0.05f); } }
	private const float MovementSpeedX = 0.08f;
	// Properties
	[SerializeField] private int health = 1;
	[SerializeField] private float dirMoving = 1; // -1 or 1. I like to pace.

	// Getters (Overrides)
	override protected float HorzMoveInputVelXDelta() {
		return dirMoving*MovementSpeedX;
	}
	// Getters (Private)
	private bool IsInvincible { get { return health < 0; } }


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();

		// Size me, queen!
		SetSize (new Vector2(2.5f, 2.5f));
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
		AcceptHorzMoveInput();
		myWhiskers.UpdateSurfaceDists(); // update these dependently now, so we guarantee most up-to-date info.
		ApplyVel();

		// Update vel to be the distance we ended up moving this frame.
		vel = pos - ppos;
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GetHit() {
		health --;
		if (health <= 0) {
			Die();
		}
	}


	// ----------------------------------------------------------------
	//  Events (Physics)
	// ----------------------------------------------------------------
	override protected void OnTouchSurface(int side, Collider2D collider) {
		base.OnTouchSurface(side, collider);

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
	override public void OnPlayerBounceOnMe(Player player) {
		if (IsInvincible) { return; } // Invincible? Do nothin'.
		GetHit();
	}



}
