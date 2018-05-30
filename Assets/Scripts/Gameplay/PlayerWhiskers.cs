using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiskers : MonoBehaviour {
	// Constants
	private const int NumSides = 4; // it's hip to be square.
	private const int NumWhiskersPerSide = 3; // this MUST match SideOffsetLocs! Just made its own variable for easy/readable access.
	private float[] SideOffsetLocs = new float[]{-0.45f, 0f, 0.45f}; // 3 whiskers per side: left, center, right.
	// References
	[SerializeField] private Player myPlayer=null;
	// Properties
	[SerializeField] LayerMask myLayerMask; // set to "Ground". We only want to detect ground and ignore everything else.
	private float[,] groundDists; // by side,index. This is *all* whisker data.
	private float[] groundDistsMin; // by side. We'll use this value more often; we just wanna know how close we're considered to the ground.
	private RaycastHit2D hit;
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
	/** It's most efficient only to search as far as the Player is going to move this frame. */
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
	private float GetWhiskerRaycastDistToGround(int side, int index) {
		Vector2 dir = whiskerDirs[side];
		Vector2 pos = WhiskerPos(side, index);
		float raycastSearchDist = GetRaycastSearchDist(side);
		hit = Physics2D.Raycast(pos, dir, raycastSearchDist, myLayerMask);
		if (hit.collider != null) {
			if (LayerMask.LayerToName(hit.collider.gameObject.layer) == LayerNames.Ground) {
				return Vector2.Distance(hit.point, pos);
			}
		}
		// Didn't hit any ground? Ok, return infinity.
		return Mathf.Infinity;
	}

	public float GroundDistMin(int side) { return groundDistsMin[side]; }
	public bool GetOnGround() {
		UpdateGroundDist(Sides.B); // Just update the bottom.
		return GroundDistMin(Sides.B) <= 0;
	}


	// ----------------------------------------------------------------
	//  Gizmos!
	// ----------------------------------------------------------------
	void OnDrawGizmos() {
		if (whiskerDirs==null) { return; } // Safety check.

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
		groundDistsMin = new float[NumSides];
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
		groundDistsMin[side] = Mathf.Infinity; // Gotta default the min dist to infinity (last frame doesn't matter anymore).
		for (int index=0; index<NumWhiskersPerSide; index++) {
			float dist = GetWhiskerRaycastDistToGround(side, index);
			groundDists[side,index] = dist;
			if (groundDistsMin[side] > dist) { // Update the min distance, too.
				groundDistsMin[side] = dist;
			}
		}
	}



}



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