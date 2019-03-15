using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class PlatformCharacterWhiskers : MonoBehaviour {
	// Overridables
	abstract protected string[] GetLayerMaskNames_LRTB();
	abstract protected string[] GetLayerMaskNames_B();
	// Constants
	private const float TouchDistThreshold = 0.02f; // if we're this close to a surface, we count it as touching. This COULD be 0 and still work, but I like the grace for slightly-more-generous wall-detection.
	private const int NumSides = PlatformCharacter.NumSides;
	private const int NumWhiskersPerSide = 3; // this MUST match SideOffsetLocs! Just made its own variable for easy/readable access.
	private float[] SideOffsetLocs = new float[]{-0.45f, 0f, 0.45f}; // 3 whiskers per side: left, center, right.
	// References
	[SerializeField] private PlatformCharacter myCharacter=null;
	// Properties
	LayerMask lm_LRTB; // The LMs that care about every side (L, R, T, B). E.g. Ground.
	LayerMask lm_B;    // The LMs that care about the bottom side. E.g. Platforms.
	private Collider2D[,] collidersAroundMe; // by side,index.
	private HashSet<Collider2D>[] collidersTouching;
	private HashSet<Collider2D>[] pcollidersTouching;
	private bool[] onSurfaces; // index is side.
	private float[,] surfaceDists; // by side,index. This is *all* whisker data.
	private int[] minDistsIndexes; // by side. WHICH whisker at this side is the closest!
	private RaycastHit2D hit; // only out here so we don't have to make a ton every frame.
	private Vector2[] whiskerDirs;

	// Getters
	private Vector2 charSize { get { return myCharacter.Size; } }
	private Vector2 WhiskerPos(int side, int index) {
		Vector2 pos = myCharacter.PosGlobal;
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

	public bool OnSurface(int side) { return onSurfaces[side]; }
	public float SurfaceDistMin(int side) {
		if (surfaceDists==null) { return 0; } // Safety check for runtime compile.
		if (minDistsIndexes[side] == -1) { return Mathf.Infinity; } // No closest whisker (none collide)? They're all infinity, then.
		return surfaceDists[side, minDistsIndexes[side]];
	}

	public bool MayEatGems() {
		foreach (Collider2D col in collidersTouching[Sides.B]) {
			BaseGround baseGround = col.GetComponent<BaseGround>();
			if (baseGround != null && baseGround.CanEatGems) {
				return true; // This one's good!
			}
		}
		return false; // Wow, nah, we're not touching any gem-friendly grounds.
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
				bool isTouching = surfaceDists[side,index] < TouchDistThreshold;
				Gizmos.color = isTouching ? Color.green : Color.red;
				Gizmos.DrawLine(startPos, startPos + dir * length);
			}
		}
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Awake() {
		lm_B = LayerMask.GetMask(GetLayerMaskNames_B());
		lm_LRTB = LayerMask.GetMask(GetLayerMaskNames_LRTB());
//		// Combine our bitmask arrays into single ones for easy access.
//		lm_B = 0;
//		lm_LRTB = 0;
//		string[] names_B = GetLayerMaskNames_B();
//		string[] names_LRTB = GetLayerMaskNames_LRTB();
//		foreach (string name in names_B) {
//			LayerMask mask = LayerMask.NameToLayer(name);
//			lm_B = lm_B | mask; // Add each one from the bottom-masks array to the single bottom-bitmask.
//		}
//		foreach (string name in names_LRTB) {
//			LayerMask mask = LayerMask.NameToLayer(name);
//			lm_LRTB = lm_LRTB | mask; // Add each one from the all-sides-masks array to the single all-sides-bitmask.
//			lm_B = lm_B | mask; // ALSO add this all-sides-mask to the bottom-masks array, too! (In case we make a mistake and forget to specify this mask in both arrays in the editor.)
//		}

		surfaceDists = new float[NumSides,NumWhiskersPerSide];
		collidersAroundMe = new Collider2D[NumSides,NumWhiskersPerSide];
		collidersTouching = new HashSet<Collider2D>[4];
		pcollidersTouching = new HashSet<Collider2D>[4];
		for (int side=0; side<collidersTouching.Length; side++) {
			collidersTouching[side] = new HashSet<Collider2D>();
		}
		onSurfaces = new bool[NumSides];
		minDistsIndexes = new int[NumSides];
		whiskerDirs = new Vector2[NumSides];
		whiskerDirs[Sides.L] = Vector2Int.L.ToVector2();
		whiskerDirs[Sides.R] = Vector2Int.R.ToVector2();
		whiskerDirs[Sides.T] = Vector2Int.T.ToVector2();
		whiskerDirs[Sides.B] = Vector2Int.B.ToVector2();
	}


	// ----------------------------------------------------------------
	//  DEPENDENT Updates
	// ----------------------------------------------------------------
	public void UpdateSurfaces() {
		if (pcollidersTouching==null) { return; } // Safety check for runtime compile.
		for (int side=0; side<whiskerDirs.Length; side ++) {
			// Remember the previous colliders, and clear out the new list!
			pcollidersTouching[side] = new HashSet<Collider2D>(collidersTouching[side]);
			collidersTouching[side].Clear();

			UpdateSurface(side);
		}

		// Now that EVERY side's been updated, check: Have we STOPPED or STARTED touching an old/new collider?
		for (int side=0; side<whiskerDirs.Length; side++) {
			foreach (Collider2D col in pcollidersTouching[side]) {
				if (!collidersTouching[side].Contains(col)) {
					myCharacter.OnWhiskersLeaveCollider(side, col);
				}
			}
		}
		for (int side=0; side<whiskerDirs.Length; side++) {
			foreach (Collider2D col in collidersTouching[side]) {
				if (!pcollidersTouching[side].Contains(col)) {
					myCharacter.OnWhiskersTouchCollider(side, col);
				}
			}
		}
	}
	private void UpdateSurface(int side) {
		if (surfaceDists==null) { return; } // Safety check (for runtime compile).
		minDistsIndexes[side] = -1; // Default this to -1: There is no closest, because they're all infinity.
		for (int index=0; index<NumWhiskersPerSide; index++) {
			UpdateWhiskerRaycast(side, index); // update the distances and colliders.
			float dist = surfaceDists[side,index]; // use the dist we just updated.
			if (SurfaceDistMin(side) > dist) { // Update the min distance, too.
				minDistsIndexes[side] = index;
			}
		}
		// Update onSurfaces!
		onSurfaces[side] = collidersTouching[side].Count > 0;
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
		// If we're (just about) touching this collider...!
		if (dist < TouchDistThreshold) {
			if (hit.collider != null && !collidersTouching[side].Contains(hit.collider)) {
				collidersTouching[side].Add(hit.collider);
			}
		}
//		// Is the collider for this raycast DIFFERENT?? Tell my character we've touched/left surfaces!!
//		if (pCollider != hit.collider) {
//			if (pCollider != null) {
//				myCharacter.OnWhiskersLeaveCollider(side, pCollider);
//			}
//			if (hit.collider != null) {
//				myCharacter.OnWhiskersTouchCollider(side, hit.collider);
//			}
//		}
	}



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
/*
	public Collider2D GetSurfaceTouching(int side) {
		if (collidersAroundMe==null) { return null; } // Safety check for runtime compile.
//		UpdateSurfaceDist(side); // TO DO: Check if this has any effect!!
		if (minDistsIndexes[side] == -1) { return null; } // No closest whisker (none collide)? Return null.
		float distMin = SurfaceDistMin(side);
		if (distMin < 0.1f) { // Only count if we're like riiight up against it!
			return collidersAroundMe[side, minDistsIndexes[side]];
		}
		return null;
	}
	*/

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