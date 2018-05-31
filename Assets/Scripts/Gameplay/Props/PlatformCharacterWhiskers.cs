using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCharacterWhiskers : MonoBehaviour {
	// Constants
	private const int NumSides = PlatformCharacter.NumSides;
	private const int NumWhiskersPerSide = 3; // this MUST match SideOffsetLocs! Just made its own variable for easy/readable access.
	private float[] SideOffsetLocs = new float[]{-0.45f, 0f, 0.45f}; // 3 whiskers per side: left, center, right.
	// References
	[SerializeField] private PlatformCharacter myCharacter=null;
	// Properties
	[SerializeField] LayerMask[] lms_LRTB=null; // The LMs that care about every side (L, R, T, B). E.g. Ground.
	[SerializeField] LayerMask[] lms_B=null; // The LMs that care about the bottom side. E.g. Platforms.
	LayerMask lm_LRTB; // Made from lms_LRTB in Start.
	LayerMask lm_B;    // Made from lms_B in Start.
	private Collider2D[,] collidersAroundMe; // by side,index.
	private float[,] surfaceDists; // by side,index. This is *all* whisker data.
	private int[] minDistsIndexes; // by side. WHICH whisker at this side is the closest!
	private RaycastHit2D hit; // only out here so we don't have to make a ton every frame.
	private Vector2[] whiskerDirs;

	// Getters
	private Vector2 charSize { get { return myCharacter.Size; } }
	private Vector2 WhiskerPos(int side, int index) {
		Vector2 pos = myCharacter.Pos;
		float sideOffsetLoc = SideOffsetLocs[index];
		if (side==Sides.L || side==Sides.R) {
			pos += new Vector2(whiskerDirs[side].x*charSize.x*0.5f, charSize.y*sideOffsetLoc);
		}
		else {
			pos += new Vector2(charSize.x*sideOffsetLoc, whiskerDirs[side].y*charSize.y*0.5f);
		}
		return pos;
	}
	/** It's most efficient only to search as far as the Player is going to move this frame. */
	private float GetRaycastSearchDist(int side) {
		const float bloat = 0.2f; // how much farther than the player's exact velocity to look. For safety.
		switch (side) {
		case Sides.L: return Mathf.Max(0, -myCharacter.Vel.x) + bloat;
		case Sides.R: return Mathf.Max(0,  myCharacter.Vel.x) + bloat;
		case Sides.B: return Mathf.Max(0, -myCharacter.Vel.y) + bloat;
		case Sides.T: return Mathf.Max(0,  myCharacter.Vel.y) + bloat;
		default: Debug.LogError("Side undefined in GetRaycastSearchDist!: " + side); return 0;
		}
	}
	private LayerMask GetLayerMask(int side) {
		if (side == Sides.B) { return lm_B; } // Bottom side? Return that respective bitmask!
		return lm_LRTB; // All sides? Return THAT respective bitmask!
//		if (side == Sides.B) { return lm_ground | lm_platform; } // Bottom side? Return ground AND platforms!
//		return lm_ground; // All other sides only care about ground.
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

	public float SurfaceDistMin(int side) {
		if (surfaceDists==null) { return 0; } // Safety check for runtime compile.
		if (minDistsIndexes[side] == -1) { return Mathf.Infinity; } // No closest whisker (none collide)? They're all infinity, then.
		return surfaceDists[side, minDistsIndexes[side]];
	}
	public Collider2D GetSurfaceTouching(int side) {
		if (collidersAroundMe==null) { return null; } // Safety check for runtime compile.
		UpdateSurfaceDist(side); // Just update the bottom.
		if (minDistsIndexes[side] == -1) { return null; } // No closest whisker (none collide)? Return null.
		return collidersAroundMe[side, minDistsIndexes[side]];
	}


	// ----------------------------------------------------------------
	//  Gizmos!
	// ----------------------------------------------------------------
	void OnDrawGizmos() {
		if (whiskerDirs==null || surfaceDists==null) { return; } // Safety check.

		for (int side=0; side<whiskerDirs.Length; side++) {
			float length = GetRaycastSearchDist(side);
			Vector2 dir = whiskerDirs[side];
			for (int index=0; index<NumWhiskersPerSide; index++) {
				Vector2 startPos = WhiskerPos(side, index);
				bool isTouching = surfaceDists[side,index] < 0.1f;
				Gizmos.color = isTouching ? Color.green : Color.red;
				Gizmos.DrawLine(startPos, startPos + dir * length);
			}
		}
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Awake() {
		// Combine our bitmask arrays into single ones for easy access.
		lm_B = 0;
		lm_LRTB = 0;
		foreach (LayerMask mask in lms_B) {
			lm_B = lm_B | mask; // Add each one from the bottom-masks array to the single bottom-bitmask.
		}
		foreach (LayerMask mask in lms_LRTB) {
			lm_LRTB = lm_LRTB | mask; // Add each one from the all-sides-masks array to the single all-sides-bitmask.
			lm_B = lm_B | mask; // ALSO add this all-sides-mask to the bottom-masks array, too! (In case we make a mistake and forget to specify this mask in both arrays in the editor.)
		}

		surfaceDists = new float[NumSides,NumWhiskersPerSide];
		collidersAroundMe = new Collider2D[NumSides,NumWhiskersPerSide];
		minDistsIndexes = new int[NumSides];
		whiskerDirs = new Vector2[NumSides];
		whiskerDirs[Sides.L] = Vector2Int.L.ToVector2();
		whiskerDirs[Sides.R] = Vector2Int.R.ToVector2();
		whiskerDirs[Sides.T] = Vector2Int.T.ToVector2();
		whiskerDirs[Sides.B] = Vector2Int.B.ToVector2();
	}
	private void Start() {
		UpdateSurfaceDists(); // Just for consistency.
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	//	private void FixedUpdate() {
	//		UpdateGroundDists();
	//	}
	public void UpdateSurfaceDists() {
		for (int i=0; i<whiskerDirs.Length; i ++) {
			UpdateSurfaceDist(i);
		}
	}
	public void UpdateSurfaceDist(int side) {
		if (surfaceDists==null) { return; } // Safety check (for runtime compile).
		//		groundDistsMin[side] = Mathf.Infinity; // Gotta default the min dist to infinity (last frame doesn't matter anymore).
		minDistsIndexes[side] = -1; // Default this to -1: There is no closest, because they're all infinity.
		for (int index=0; index<NumWhiskersPerSide; index++) {
			UpdateWhiskerRaycast(side, index); // update the distances and colliders.
			float dist = surfaceDists[side,index]; // use the dist we just updated.
			if (SurfaceDistMin(side) > dist) { // Update the min distance, too.
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
		surfaceDists[side,index] = dist;
		collidersAroundMe[side,index] = hit.collider;
	}



}


//			if (LayerMask.LayerToName(hit.collider.gameObject.layer) == LayerNames.Ground) {
//			}

//	public bool IsTouchingGround() {
////		UpdateGroundDists();
//		for (int i=0; i<NumSides; i ++) {
//			if (IsTouchingGroundAtSide(i)) { return true; }
//		}
//		return false;
//	}
//	public bool IsTouchingGroundAtSide(Vector2 dir) {
//		return IsTouchingGroundAtSide(dir.ToVector2Int());
//	}
//	public bool IsTouchingGroundAtSide(Vector2Int dir) {
//		return IsTouchingGroundAtSide(MathUtils.GetSide(dir));
//	}
//	public bool IsTouchingGroundAtSide(int side) {
//		return DistToGroundAtSide(side) < 0.3f;
//	}
//	public float DistToGroundAtSide(int side) {
//		Vector2 dir = whiskerDirs[side];
//		Vector2 pos = WhiskerPos(side);
//		hit = Physics2D.Raycast(pos, dir, Mathf.Infinity, myLayerMask);
//		if (hit.collider != null && !hit.collider.isTrigger) {
//			float dist = Vector2.Distance(hit.point, pos);
//			if (LayerMask.LayerToName(hit.collider.gameObject.layer) == LayerNames.Ground) {
//				return dist;
//			}
//		}
//		return Mathf.Infinity;
//	}