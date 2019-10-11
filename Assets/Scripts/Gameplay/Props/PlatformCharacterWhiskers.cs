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
	private readonly float[] SideOffsetLocs = new float[]{-0.4f, 0f, 0.4f}; // 3 whiskers per side: left, center, right.
    // References
    [SerializeField] private PlatformCharacter myCharacter=null;
    // Properties
    public int SideFeet { get; private set; }
    public int SideHead { get; private set; }
    public readonly int SideLeft  = Sides.L;
    public readonly int SideRight = Sides.R;
    LayerMask lm_LRTB; // The LMs that care about every side (L, R, T, B). E.g. Ground.
    LayerMask lm_B;    // The LMs that care about the bottom side. E.g. Platforms.
    private LayerMask lm_triggerColls; // triggers I will treat as colliders. Useful for Platforms, which we don't want Rigidbody collisions, but DO want *my* manual whisker collisions.
	private Collider2D[,] collsAroundMe; // by side, index.
	private HashSet<Collider2D>[] collsTouching;
	private HashSet<Collider2D>[] pcollsTouching;
    public int DirLastTouchedWall { get; private set; } // for wall-kicking perhaps pixels away from a wall.
    private bool[] onSurfaces; // index is side.
	private float[,] surfaceDists; // by side,index. This is *all* whisker data.
	private int[] minDistsIndexes; // by side. WHICH whisker at this side is the closest!
    private float[] collSpeedsRel; // by side. The (highest) speed a collidable's moving TOWARDS me. Usually 0's, except for TravelMinds.
    private RaycastHit2D h; // out here so we don't make a ton every frame.
    private RaycastHit2D[] hits; // out here so we don't make a ton every frame.
	private Vector2[] whiskerDirs;

    // Getters
    //public Collidable TEMP_GetFloorCollidable() {
    //    for (int i=0; i<NumWhiskersPerSide; i++) {
    //        Collider2D coll = collsAroundMe[Sides.B,i];
    //        if (coll == null) { continue; }
    //        Collidable collidable = coll.GetComponent<Collidable>();
    //        if (collidable != null) { return collidable; }
    //    }
    //    return null;
    //}
    public Collidable TEMP_GetFloorCollidable() {
        foreach (Collider2D coll in collsTouching[SideFeet]) {
            Collidable collidable = coll.GetComponent<Collidable>();
            if (collidable != null) { return collidable; }
        }
        return null;
    }

    private Vector2 charSize { get { return myCharacter.Size; } }
    private RaycastHit2D[] GetRaycast(int side, int wi) {
        return Physics2D.RaycastAll(
            WhiskerPos(side, wi),
            whiskerDirs[side],
            GetRaycastSearchDist(side),
            GetLayerMask(side));
    }
	private Vector2 WhiskerPos(int side, int index) {
		Vector2 pos = myCharacter.PosGlobal;
		float sideOffsetLoc = SideOffsetLocs[index];
		if (side==SideLeft || side==SideRight) {
			pos += new Vector2(whiskerDirs[side].x*charSize.x*0.5f, charSize.y*sideOffsetLoc);
		}
		else {
			pos += new Vector2(charSize.x*sideOffsetLoc, whiskerDirs[side].y*charSize.y*0.5f);
		}
		return pos;
    }
    /** It's more efficient only to search as far as the Player is going to move this frame. */
    private float GetRaycastSearchDist(int side) {
		const float bloat = 5f; //0.2f NOTE: Increased this lots so we can ALSO know what else is around us! // how much farther than the player's exact velocity to look. For safety.
		switch (side) {
    		case Sides.L: return Mathf.Max(0, -myCharacter.vel.x) + bloat;
    		case Sides.R: return Mathf.Max(0,  myCharacter.vel.x) + bloat;
    		case Sides.B: return Mathf.Max(0, -myCharacter.vel.y) + bloat;
    		case Sides.T: return Mathf.Max(0,  myCharacter.vel.y) + bloat;
    		default: Debug.LogError("Side undefined in GetRaycastSearchDist!: " + side); return 0;
		}
	}
	private LayerMask GetLayerMask(int side) {
		if (side == SideFeet) { return lm_B; } // Bottom side? Return that respective bitmask!
		return lm_LRTB; // All sides? Return THAT respective bitmask!
//		if (side == Sides.B) { return lm_ground | lm_platform; } // Bottom side? Return ground AND platforms!
//		return lm_ground; // All other sides only care about ground.
	}
    private bool DoCollideWithColl(Collider2D col) {
        if (col == null) { return false; } // Check the obvious.
        if (!col.isTrigger) { return true; } // NOT a trigger? Yeah, we collide!
        if (LayerUtils.IsLayerInLayermask(col.gameObject.layer, lm_triggerColls)) { return true; } // It's a trigger, BUT its layer is in my triggers-I-collide-with mask!
        return false; // Nah, don't collide.
    }
    private float DistToColl(RaycastHit2D hit, Vector2 whiskPos) {
        if (hit.collider == null) { return Mathf.Infinity; } // Hit nothing? Default to infinity.
        return Vector2.Distance(hit.point, whiskPos);
    }
    
    public bool OnSurface(int side) { return onSurfaces[side]; }
    public bool IsAgainstWall() { return DirTouchingWall() != 0; }
    public bool IsTouchingAnySurface() { return onSurfaces[Sides.L] || onSurfaces[Sides.R] || onSurfaces[Sides.B] || onSurfaces[Sides.T]; }
    public int DirTouchingWall() {
        if (OnSurface(SideLeft))  { return -1; }
        if (OnSurface(SideRight)) { return  1; }
        return 0;
    }
    /// Returns SMALLEST surfaceDist value on this side.
	public float DistToSurface(int side) {
		if (minDistsIndexes[side] == -1) { return Mathf.Infinity; } // No closest whisker (none collide)? They're all infinity, then.
		return surfaceDists[side, minDistsIndexes[side]];
	}
    /// Returns speed this Collidable is traveling towards this side of mine. (E.g. coll's vel is (-2,0) and side is R, I'll return 2.)
    private float GetCollSpeedRel(int side, Collidable collidable) {
        switch (side) {
            case Sides.L: return  collidable.vel.x;
            case Sides.R: return -collidable.vel.x;
            case Sides.B: return  collidable.vel.y;
            case Sides.T: return -collidable.vel.y;
        }
        return Mathf.NegativeInfinity;
    }
    public Vector2 GetAppliedVel() {
        Vector2 vel = myCharacter.vel;
        Vector2 av = vel;
        float distL = DistToSurface(Sides.L) - collSpeedsRel[Sides.L]; // we can only go as far as the DIST to the coll, MINUS its SPEED towards me.
        float distR = DistToSurface(Sides.R) - collSpeedsRel[Sides.R];
        float distB = DistToSurface(Sides.B) - collSpeedsRel[Sides.B];
        float distT = DistToSurface(Sides.T) - collSpeedsRel[Sides.T];
        // Clamp our vel so we don't intersect anything.
        if (vel.x<0 && vel.x<-distL) {
            av = new Vector2(-distL, av.y);
        }
        else if (vel.x>0 && vel.x>distR) {
            av = new Vector2(distR, av.y);
        }
        if (vel.y<0 && vel.y<-distB) {
            av = new Vector2(av.x, -distB);
        }
        else if (vel.y>0 && vel.y>distT) {
            av = new Vector2(av.x, distT);
        }
        return av;
    }

    public bool AreFeetOnEatEdiblesGround() {
        foreach (Collider2D col in collsTouching[SideFeet]) {
            BaseGround baseGround = col.GetComponent<BaseGround>();
            if (baseGround != null && baseGround.MayPlayerEatHere) {
                return true; // This one's good!
            }
        }
        return false; // Wow, nah, we're not touching any gem-friendly grounds.
    }
    public bool AreFeetOnlyOnCanDropThruPlatform() {
        bool isOnOkPlatform = false; // will say otherwise next.
        foreach (Collider2D col in collsTouching[SideFeet]) { // For every collider our feet are touching...
            Platform platform = col.GetComponent<Platform>();
            if (platform!=null && platform.CanDropThru) { // This one works!
                isOnOkPlatform = true; // Yes, we are!
            }
            else { // Ooh, there's something we CAN'T drop thru. Return false immediately.
                return false;
            }
        }
        return isOnOkPlatform; // We didn't run into any non-ok platforms! Return if we're on an ok one too.
    }
    public float DistToSideLastTouchedWall() {
        if (DirLastTouchedWall == 0) { return Mathf.Infinity; } // Safety check.
        int sideLastTouchedWall = DirLastTouchedWall<0 ? SideLeft : SideRight;
        return DistToSurface(sideLastTouchedWall);
    }
    private bool IsWhiskerTouchingSurface(int side, int index) {
        return surfaceDists[side,index] < TouchDistThreshold;//onSurfaces[side];
    }
    public bool IsFrontFootTouchingSurface() {
        int frontWhiskerIndex = myCharacter.vel.x<0 ? 0 : NumWhiskersPerSide-1;
        return IsWhiskerTouchingSurface(SideFeet, frontWhiskerIndex);
    }
    
    
    
    public void Test_SetTopAndBottomWhiskersFlipped(bool isFlipped) {
        SideFeet = isFlipped ? Sides.T : Sides.B;
        SideHead = isFlipped ? Sides.B : Sides.T;
        //whiskerDirs[Sides.T] = Vector2Int.T.ToVector2();
        //whiskerDirs[Sides.B] = Vector2Int.B.ToVector2();
        //if (isFlipped) {
        //    whiskerDirs[Sides.T] *= -1;
        //    whiskerDirs[Sides.B] *= -1;
        //}
    }


	// ----------------------------------------------------------------
	//  Gizmos!
	// ----------------------------------------------------------------
	void OnDrawGizmos() {
		if (whiskerDirs==null || surfaceDists==null) { return; } // Safety check.

		for (int side=0; side<NumSides; side++) {
			float length = GetRaycastSearchDist(side);
			Vector2 dir = whiskerDirs[side];
			for (int index=0; index<NumWhiskersPerSide; index++) {
				Vector2 startPos = WhiskerPos(side, index);
				bool isTouching = IsWhiskerTouchingSurface(side,index);
				Gizmos.color = isTouching ? Color.green : Color.red;
				Gizmos.DrawLine(startPos, startPos + dir * length);
			}
		}
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Awake() {
        SideFeet = Sides.B;
        SideHead = Sides.T;
        
		lm_B = LayerMask.GetMask(GetLayerMaskNames_B());
		lm_LRTB = LayerMask.GetMask(GetLayerMaskNames_LRTB());
        lm_triggerColls = LayerMask.GetMask(Layers.Platform);

		surfaceDists = new float[NumSides,NumWhiskersPerSide];
        for (int side=0; side<NumSides; side++) {
            for (int w=0; w<NumWhiskersPerSide; w++) {
                surfaceDists[side,w] = Mathf.Infinity; // default dists to surfaces to infinity first.
            }
        }
		collsAroundMe = new Collider2D[NumSides,NumWhiskersPerSide];
		collsTouching = new HashSet<Collider2D>[4];
		pcollsTouching = new HashSet<Collider2D>[4];
		for (int side=0; side<collsTouching.Length; side++) {
			collsTouching[side] = new HashSet<Collider2D>();
		}
		onSurfaces = new bool[NumSides];
		minDistsIndexes = new int[NumSides];
        collSpeedsRel = new float[NumSides];
		whiskerDirs = new Vector2[NumSides];
		whiskerDirs[Sides.L] = Vector2Int.L.ToVector2();
		whiskerDirs[Sides.R] = Vector2Int.R.ToVector2();
		whiskerDirs[Sides.T] = Vector2Int.T.ToVector2();
		whiskerDirs[Sides.B] = Vector2Int.B.ToVector2();
	}


	// ----------------------------------------------------------------
	//  DEPENDENT Updates
	// ----------------------------------------------------------------
    //public void UpdateSurfaceDists() {
    //    for (int side=0; side<NumSides; side ++) {
    //        UpdateSurfaceDist(side);
    //    }
    //}
    //private void UpdateSurfaceDist(int side) {
    //    minDistsIndexes[side] = -1; // Default this to -1: There is no closest, because they're all infinity.
    //    collSpeedsRel[side] = Mathf.NegativeInfinity; // nothing's moving towards me.
    //    for (int index=0; index<NumWhiskersPerSide; index++) {
    //        UpdateWhiskerRaycast(side, index); // update the distances and colliders.
    //        float dist = surfaceDists[side,index]; // use the dist we just updated.
    //        if (DistToSurface(side) > dist) { // Update the min distance, too.
    //            minDistsIndexes[side] = index;
    //        }
    //    }
    //    onSurfaces[side] = collsTouching[side].Count > 0; // Update onSurfaces!
    //}
    ///// wi: WhiskerIndex
    //private void UpdateWhiskerRaycast(int side, int wi) {
    //    // Find the closest collider.
    //    hits = GetRaycast(side, wi);
    //    h = new RaycastHit2D();
    //    Collider2D coll = null;
    //    for (int i=0; i<hits.Length; i++) { // Check every collision for ones we interact with...
    //        if (DoCollideWithColl(hits[i].collider)) {
    //            h = hits[i];
    //            coll = h.collider;
    //            break;
    //        }
    //    }

    //    // Update my knowledge!
    //    float dist = DistToColl(h, WhiskerPos(side, wi));
    //    surfaceDists[side,wi] = dist;
    //    collsAroundMe[side,wi] = coll;
    //    UpdateCollSpeedsRel(side, coll);
    //}
    
    
	public void UpdateSurfaces() {
		for (int side=0; side<NumSides; side ++) {
			// Remember the previous colliders, and clear out the new list!
			pcollsTouching[side] = new HashSet<Collider2D>(collsTouching[side]);
			collsTouching[side].Clear();
            
			UpdateSurface(side);
        }

        if (DirTouchingWall() != 0) {
            DirLastTouchedWall = DirTouchingWall();
        }
        
        // Now that EVERY side's been updated, check: Have we STOPPED or STARTED touching an old/new collider?
        for (int side=0; side<NumSides; side++) {
			foreach (Collider2D col in pcollsTouching[side]) {
				if (!collsTouching[side].Contains(col)) {
					myCharacter.OnWhiskersLeaveCollider(side, col);
				}
			}
		}
		for (int side=0; side<NumSides; side++) {
			foreach (Collider2D col in collsTouching[side]) {
				if (!pcollsTouching[side].Contains(col)) {
                    // Ignore collider? Skip it.
                    if (myCharacter.IgnoreColl(side, col)) { continue; }
					myCharacter.OnWhiskersTouchCollider(side, col);
				}
			}
        }
    }
	private void UpdateSurface(int side) {
		minDistsIndexes[side] = -1; // Default this to -1: There is no closest, because they're all infinity.
        collSpeedsRel[side] = 0;// TEST. 0 instead of neg-infinity will disable being able to move into a coll that won't be there next frame. //Mathf.NegativeInfinity; // nothing's moving towards me.
		for (int index=0; index<NumWhiskersPerSide; index++) {
			UpdateWhiskerRaycast(side, index); // update the distances and colliders.
			float dist = surfaceDists[side,index]; // use the dist we just updated.
			if (DistToSurface(side) > dist) { // Update the min distance, too.
				minDistsIndexes[side] = index;
			}
		}
		// Update onSurfaces!
		onSurfaces[side] = collsTouching[side].Count > 0;
	}
    /// wi: WhiskerIndex
	private void UpdateWhiskerRaycast(int side, int wi) {
        // Find the closest collider.
		hits = GetRaycast(side, wi);
        h = new RaycastHit2D();
        Collider2D coll = null;
        for (int i=0; i<hits.Length; i++) { // Check every collision for ones we interact with...
            if (DoCollideWithColl(hits[i].collider)) {
                h = hits[i];
                coll = h.collider;
                break;
            }
        }

        // Update my knowledge!
        float dist = DistToColl(h, WhiskerPos(side, wi));
        
        //// TESTTT!
        //if (coll != null) {
        //    Collidable collidable = coll.gameObject.GetComponent<Collidable>();
        //    if (collidable != null) {
        //        float collSpeed = GetCollSpeedRel(side, collidable);
        //        dist += -Mathf.Abs(collSpeed);
        //        dist = Mathf.Max(0, dist);
        //    }
        //}
        
		surfaceDists[side,wi] = dist;
		collsAroundMe[side,wi] = coll;
        UpdateCollSpeedsRel(side, coll);
        
        //if (side == 0 && dist<Mathf.Infinity) {
        //    print(Time.frameCount + " side, wi: " + side+","+wi + "  dist: " + dist + " coll: " + coll + "    IgnoreColl: " + myCharacter.IgnoreColl(side, coll));
        //}
        
        // If we HIT a collider...!
        if (coll != null) {
            // If we're (just about) touching this collider...!
            if (dist <= TouchDistThreshold) {
                // If we SHAN'T ignore this collider...!
                if (!myCharacter.IgnoreColl(side, coll)) {
        			if (!collsTouching[side].Contains(coll)) {
        				collsTouching[side].Add(coll);
        			}
        		}
            }
        }
	}
    
    
    private void UpdateCollSpeedsRel(int side, Collider2D coll2D) {
        if (coll2D != null) {
            Collidable collidable = coll2D.gameObject.GetComponent<Collidable>();
            if (collidable != null) {
                float collSpeed = GetCollSpeedRel(side, collidable);
                collSpeedsRel[side] = Mathf.Max(collSpeed, collSpeedsRel[side]);
            }
        }
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