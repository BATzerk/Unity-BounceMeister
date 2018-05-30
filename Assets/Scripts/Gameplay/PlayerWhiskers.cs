using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiskers : MonoBehaviour {
	// Constants
	private int NumSides = 4; // it's hip to be square.
	// References
	[SerializeField] private Player myPlayer=null;
	// Properties
	[SerializeField] LayerMask myLayerMask; // we only want to detect ground and ignore everything else.
	private float[] groundDists; // by side.
	private RaycastHit2D hit;
	private Vector2[] whiskerDirs;

	// Getters
	private Vector2 WhiskerPos(int index) {
		return myPlayer.Pos + new Vector2(whiskerDirs[index].x*myPlayer.Size.x*0.5f, whiskerDirs[index].y*myPlayer.Size.y*0.5f); // *0.5f because Size is diameter and we want radius.
	}

	public float GroundDist(int side) { return groundDists[side]; }
	public bool GetOnGround() {
		UpdateGroundDist(Sides.B); // Just update the bottom.
		return GroundDist(Sides.B) <= 0;
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


	// ----------------------------------------------------------------
	//  Gizmos!
	// ----------------------------------------------------------------
	void OnDrawGizmos() {
		if (whiskerDirs==null) { return; } // Safety check.

		float length = 1f;
		for (int i=0; i<whiskerDirs.Length; i ++) {
			Vector2 dir = whiskerDirs[i];
			Vector2 startPos = WhiskerPos(i);
			bool isTouchingGround = groundDists[i] < 0.1f;//IsTouchingGroundAtSide(i);
			Gizmos.color = isTouchingGround ? Color.green : Color.red;
			Gizmos.DrawLine(startPos, startPos + dir * length);
		}
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Awake() {
		groundDists = new float[NumSides];
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
		Vector2 dir = whiskerDirs[side];
		Vector2 pos = WhiskerPos(side);
		hit = Physics2D.Raycast(pos, dir, Mathf.Infinity, myLayerMask);
		groundDists[side] = Mathf.Infinity; // Default to forever away.
		if (hit.collider != null) {
			float dist = Vector2.Distance(hit.point, pos);
			if (LayerMask.LayerToName(hit.collider.gameObject.layer) == LayerNames.Ground) {
				groundDists[side] = dist;
			}
		}
	}



}

