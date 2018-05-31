﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWhiskers : PlatformCharacterWhiskers {
	/*
	// References
	[SerializeField] private Enemy myEnemy=null;

	// Getters
	/** It's most efficient only to search as far as the Player is going to move this frame. * /
	private float GetRaycastSearchDist(int side) {
		const float bloat = 0.2f; // how much farther than the player's exact velocity to look. For safety.
		switch (side) {
		case Sides.L: return Mathf.Max(0, -myEnemy.Vel.x) + bloat;
		case Sides.R: return Mathf.Max(0,  myEnemy.Vel.x) + bloat;
		case Sides.B: return Mathf.Max(0, -myEnemy.Vel.y) + bloat;
		case Sides.T: return Mathf.Max(0,  myEnemy.Vel.y) + bloat;
		default: Debug.LogError("Side undefined in GetRaycastSearchDist!: " + side); return 0;
		}
	}
	private LayerMask GetLayerMask(int side) {
		if (side == Sides.B) { return lm_ground | lm_platform; } // Bottom side? Return ground AND platforms!
		return lm_ground; // All other sides only care about ground.
	}

	public float GroundDistMin(int side) {
		if (minDistsIndexes[side] == -1) { return Mathf.Infinity; } // No closest whisker (none collide)? They're all infinity, then.
		return groundDists[side, minDistsIndexes[side]];
	}
	public Collider2D GetGroundTouching(int side) {
		if (collidersAroundMe==null) { return null; } // Safety check for runtime compile.
		UpdateGroundDist(side); // Just update the bottom.
		if (minDistsIndexes[side] == -1) { return null; } // No closest whisker (none collide)? Return null.
		return collidersAroundMe[side, minDistsIndexes[Sides.B]];
	}


	// ----------------------------------------------------------------
	//  Gizmos!
	// ----------------------------------------------------------------
	void OnDrawGizmos() {
		if (whiskerDirs==null || groundDists==null) { return; } // Safety check.

		for (int side=0; side<whiskerDirs.Length; side++) {
			float length = GetRaycastSearchDist(side);
			Vector2 dir = whiskerDirs[side];
			for (int index=0; index<NumWhiskersPerSide; index++) {
				Vector2 startPos = WhiskerPos(side, index);
				bool isTouchingGround = groundDists[side,index] < 0.1f;
				Gizmos.color = isTouchingGround ? Color.green : Color.red;
				Gizmos.DrawLine(startPos, startPos + dir * length);
			}
		}
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Awake() {
		groundDists = new float[NumSides,NumWhiskersPerSide];
		collidersAroundMe = new Collider2D[NumSides,NumWhiskersPerSide];
		minDistsIndexes = new int[NumSides];
		whiskerDirs = new Vector2[NumSides];
		whiskerDirs[Sides.L] = Vector2Int.L.ToVector2();
		whiskerDirs[Sides.R] = Vector2Int.R.ToVector2();
		whiskerDirs[Sides.T] = Vector2Int.T.ToVector2();
		whiskerDirs[Sides.B] = Vector2Int.B.ToVector2();
	}
	private void Start() {
		UpdateGroundDists(); // Just for consistency.
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	//	private void FixedUpdate() {
	//		UpdateGroundDists();
	//	}
	public void UpdateGroundDists() {
		for (int i=0; i<whiskerDirs.Length; i ++) {
			UpdateGroundDist(i);
		}
	}
	public void UpdateGroundDist(int side) {
		if (groundDists==null) { return; } // Safety check (for runtime compile).
		minDistsIndexes[side] = -1; // Default this to -1: There is no closest, because they're all infinity.
		for (int index=0; index<NumWhiskersPerSide; index++) {
			UpdateWhiskerRaycast(side, index); // update the distances and colliders.
			float dist = groundDists[side,index]; // use the dist we just updated.
			if (GroundDistMin(side) > dist) { // Update the min distance, too.
				minDistsIndexes[side] = index;
			}
		}
	}
	private void UpdateWhiskerRaycast(int side, int index) {
		Vector2 dir = whiskerDirs[side];
		Vector2 pos = WhiskerPos(side, index);
		float raycastSearchDist = GetRaycastSearchDist(side);
		LayerMask mask = GetLayerMask(side);
		hit = Physics2D.Raycast(pos, dir, raycastSearchDist, mask);
		float dist = Mathf.Infinity; // default to infinity in case we don't hit any ground.
		if (hit.collider != null) {
			dist = Vector2.Distance(hit.point, pos);
		}
		groundDists[side,index] = dist;
		collidersAroundMe[side,index] = hit.collider;
	}
	*/


}


