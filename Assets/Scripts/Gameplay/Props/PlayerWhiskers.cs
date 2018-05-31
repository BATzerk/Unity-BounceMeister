using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiskers : PlatformCharacterWhiskers {
	/*
	// Constants
	private const int NumWhiskersPerSide = 3; // this MUST match SideOffsetLocs! Just made its own variable for easy/readable access.
	private float[] SideOffsetLocs = new float[]{-0.45f, 0f, 0.45f}; // 3 whiskers per side: left, center, right.
	// References
	[SerializeField] private Player myPlayer=null;
	// Properties
	[SerializeField] LayerMask lm_ground; // All sides always care about Ground.
	[SerializeField] LayerMask lm_platform; // Only the bottom side cares about Platforms.
	private Collider2D[,] collidersAroundMe; // by side,index.
	private float[,] groundDists; // by side,index. This is *all* whisker data.
//	private float[] groundDistsMin; // by side. We'll use this value more often; we just wanna know how close we're considered to the ground.
	private int[] minDistsIndexes; // by side. WHICH whisker at this side is the closest!
	private RaycastHit2D hit; // only out here so we don't have to make a ton every frame.
	private Vector2[] whiskerDirs;

	// Getters
	private Vector2 playerSize { get { return myPlayer.Size; } }
	private Vector2 WhiskerPos(int side, int index) {
		Vector2 pos = myPlayer.Pos;
		float sideOffsetLoc = SideOffsetLocs[index];
		if (side==Sides.L || side==Sides.R) {
			pos += new Vector2(whiskerDirs[side].x*playerSize.x*0.5f, playerSize.y*sideOffsetLoc);
		}
		else {
			pos += new Vector2(playerSize.x*sideOffsetLoc, whiskerDirs[side].y*playerSize.y*0.5f);
		}
		return pos;
	}
	/** It's most efficient only to search as far as the Player is going to move this frame. * /
	private float GetRaycastSearchDist(int side) {
		const float bloat = 0.2f; // how much farther than the player's exact velocity to look. For safety.
		switch (side) {
		case Sides.L: return Mathf.Max(0, -myPlayer.Vel.x) + bloat;
		case Sides.R: return Mathf.Max(0,  myPlayer.Vel.x) + bloat;
		case Sides.B: return Mathf.Max(0, -myPlayer.Vel.y) + bloat;
		case Sides.T: return Mathf.Max(0,  myPlayer.Vel.y) + bloat;
		default: Debug.LogError("Side undefined in GetRaycastSearchDist!: " + side); return 0;
		}
	}
	private LayerMask GetLayerMask(int side) {
		if (side == Sides.B) { return lm_ground | lm_platform; } // Bottom side? Return ground AND platforms!
		return lm_ground; // All other sides only care about ground.
	}
//	/// Redundant with my other raycast function. These could be combined.
//	private Collider2D GroundColAtSide(int side, int index) {
//		Vector2 dir = whiskerDirs[side];
//		Vector2 pos = WhiskerPos(side, index);
//		float raycastSearchDist = GetRaycastSearchDist(side);
//		LayerMask mask = GetLayerMask(side);
//		hit = Physics2D.Raycast(pos, dir, raycastSearchDist, mask);
//		return hit.collider;
//	}

	public float GroundDistMin(int side) {
		if (minDistsIndexes[side] == -1) { return Mathf.Infinity; } // No closest whisker (none collide)? They're all infinity, then.
		return groundDists[side, minDistsIndexes[side]];
	}
	public Collider2D GetGroundBottomTouching() {
		UpdateGroundDist(Sides.B); // Just update the bottom.
		if (minDistsIndexes[Sides.B] == -1) { return null; } // No closest whisker (none collide)? Return null.
		return collidersAroundMe[Sides.B, minDistsIndexes[Sides.B]];
//		return GroundColAtSide(Sides.B, 1); // HACK! TEMP! TODO: Also save vals of each collider, AND the minDists's indexes. Update all ground cols at this side, then Use the min's index to return the right collider.
//		return GroundDistMin(Sides.B) <= 0;
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
//		groundDistsMin[side] = Mathf.Infinity; // Gotta default the min dist to infinity (last frame doesn't matter anymore).
		minDistsIndexes[side] = -1; // Default this to -1: There is no closest, because they're all infinity.
		for (int index=0; index<NumWhiskersPerSide; index++) {
			UpdateWhiskerRaycast(side, index); // update the distances and colliders.
			float dist = groundDists[side,index]; // use the dist we just updated.
			if (GroundDistMin(side) > dist) { // Update the min distance, too.
//				groundDistsMin[side] = dist;
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
