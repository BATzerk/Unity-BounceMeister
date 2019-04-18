using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RoomData {
//	// Constants
//	public static readonly RoomData undefined = new RoomData(0, "undefined");
	// Components!
	public CameraBoundsData cameraBoundsData;
	public List<PropData> allPropDatas;
	// Properties
    public bool HasPlayerBeenHere { get; private set; } // false until the player enters me for the first time!
    public bool isClustStart;// { get; private set; }
    public string roomKey; // everything we use to reference this room! Including the room's file name (minus the .txt suffix).
	public int designerFlag; // for the room designer! We can flag any room to be like "testing" or "good" or etc.
    public int ClusterIndex=-1;
    public int NumSnacksCollected { get; private set; }
    public int NumSnacksTotal { get; private set; }
	public bool WasUsedInSearchAlgorithm { get; set; }
	public Vector2 posGlobal; // my position, global to ALL worlds! These values get big (up to around 70,000)!
    public List<RoomNeighborData> Neighbors; // EXACTLY like Openings, except contains refs to other rooms (which *can be null* if there's no room at an opening)!
    public List<RoomOpening> Openings { get; private set; } // JUST the openings. These can be saved.
    // Getters
    public string RoomKey { get { return roomKey; } }
	public int DesignerFlag { get { return designerFlag; } }
	public int WorldIndex { get { return MyWorldData.WorldIndex; } }
	public Rect BoundsLocal { get { return new Rect(cameraBoundsData.myRect); } } // Currently, the camera bounds and room bounds are one in the same.
	public Rect BoundsGlobal { get { return new Rect(cameraBoundsData.myRect.center+posGlobal, cameraBoundsData.myRect.size); } }
	public Vector2 PosGlobal { get { return posGlobal; } }
//	public Rect BoundsLocal { get { return boundsLocal; } }
	public WorldData MyWorldData { get; private set; }
    public bool IsInCluster { get { return ClusterIndex != -1; } }
    ///// Returns the closest PlayerStart to the provided pos.
    //public Vector2 ClosestPlayerStartPos(Vector2 playerPos) {
    //    float bestDist = Mathf.Infinity;
    //    Vector2 bestPos = Vector2.zero;
    //    foreach (PropData pd in allPropDatas) {
    //        if (pd is PlayerStartData) {
    //            float dist = Vector2.Distance(playerPos, pd.pos);
    //            if (bestDist > dist) {
    //                bestDist = dist;
    //                bestPos = pd.pos;
    //            }
    //        }
    //    }
    //    return bestPos;
    //}
    /// Returns the first PlayerStart in our list.
    public Vector2 DefaultPlayerStartPos() {
        foreach (PropData pd in allPropDatas) {
            if (pd is PlayerStartData) {
                return pd.pos;
            }
        }
        return Vector2.zero; // Oops.
    }
    public Vector2 GetRoomDoorPos(string doorID) {
        foreach (PropData pd in allPropDatas) {
            if (pd is RoomDoorData) {
                if ((pd as RoomDoorData).myID == doorID) {
                    return pd.pos;
                }
            }
        }
        // Oops, no door with this ID.
        Debug.LogWarning("Oops! No RoomDoor with doorID. Room: " + roomKey + ", doorID: " + doorID);
        return DefaultPlayerStartPos();
    }
    public bool AreEdiblesLeft() {
        return NumSnacksCollected < NumSnacksTotal;
    }
    public void UpdateNumSnacks() {
        NumSnacksTotal = 0;
        NumSnacksCollected = 0;
        foreach (PropData pd in allPropDatas) {
            if (pd is SnackData) {
                if (SaveStorage.GetBool(SaveKeys.DidEatSnack(this, NumSnacksTotal))) { NumSnacksCollected++; }
                NumSnacksTotal ++;
            }
        }
    }

	// Setters
	public void SetPosGlobal (Vector2 _posGlobal) {
		// Round my posGlobal values to even numbers! For snapping rooms together more easily.
		posGlobal = new Vector2 (Mathf.Round(_posGlobal.x*0.5f)*2f, Mathf.Round (_posGlobal.y*0.5f)*2f);
	}
	public void SetDesignerFlag (int _designerFlag) {
		designerFlag = _designerFlag;
	}
//	public void SetPosWorld (Vector2 _posWorld) {
//		posWorld = _posWorld;
//	}


	// ================================================================
	//  Initialize
	// ================================================================
	public RoomData(WorldData _worldData, string _key) { //, Vector2 defaultAbsolutePos) {
		MyWorldData = _worldData;
		roomKey = _key;
        HasPlayerBeenHere = SaveStorage.GetBool(SaveKeys.HasPlayerBeenInRoom(this), false);
        
		// Initialize all my PropData lists.
		ClearAllPropDataLists ();
        Openings = new List<RoomOpening>();
	}
	public void ClearAllPropDataLists () {
		allPropDatas = new List<PropData>();
		cameraBoundsData = new CameraBoundsData();
	}
    
    public void OnPlayerEnterMe() {
        // First time in room?? Update hasPlayerBeenHere!!
        if (!HasPlayerBeenHere) {
            HasPlayerBeenHere = true;
            SaveStorage.SetBool(SaveKeys.HasPlayerBeenInRoom(this), HasPlayerBeenHere);
        }
    }
    
    
	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
    public void CalculateOpenings() {
        // First, make list of just my GroundDatas.
        List<GroundData> groundDatas = GetGroundDatas();
        // Calculate 'em!
        Openings = new List<RoomOpening>();
        for (int side = 0; side<4; side++) {
            AddRoomOpeningsAtSide(side,groundDatas);
        }
    }
    private List<GroundData> GetGroundDatas() { // CANDO: #optimization. ONLY add Grounds that are on the sides.
        List<GroundData> groundDatas = new List<GroundData>();
        for (int i=0; i<allPropDatas.Count; i++) {
            if (allPropDatas[i] is GroundData) {
                groundDatas.Add(allPropDatas[i] as GroundData);
            }
        }
        return groundDatas;
    }
    private void AddRoomOpeningsAtSide(int side, List<GroundData> groundDatas) {
        float searchUnit = 1; // how granular our searches are. The smaller this value, the more steps we take along sides of the room.
        Rect bl = BoundsLocal;
        // Determine where we start search, and what dir to go.
        float sideLength=0;
        Vector2 cornerPos=Vector2.zero; // default to whatever.
        Vector2 dir=Vector2.one; // default to whatever.
        switch (side) {
            case Sides.L: // BL to TL
                sideLength = bl.height;
                cornerPos = new Vector2(bl.xMin, bl.yMin);
                dir = Vector2.up;
                break;
            case Sides.R: // BR to TR
                sideLength = bl.height;
                cornerPos = new Vector2(bl.xMax, bl.yMin) - new Vector2(0.1f, 0); // add small bloat so we don't miss any Grounds.
                dir = Vector2.up;
                break;
            case Sides.B: // BL to BR
                sideLength = bl.width;
                cornerPos = new Vector2(bl.xMin, bl.yMin);
                dir = Vector2.right;
                break;
            case Sides.T: // TL to TR
                sideLength = bl.width;
                cornerPos = new Vector2(bl.xMin, bl.yMax) - new Vector2(0, 0.1f); // add small bloat so we don't miss any Grounds.
                dir = Vector2.right;
                break;
        }
        Rect searchRect = new Rect {
            size = new Vector2(searchUnit,searchUnit)
        };
        // Step slowly along this side!
        int numSteps = Mathf.CeilToInt(sideLength/searchUnit) + 1; // Note: Idk why +1.
        Vector2 openingStartPos = Vector2Extensions.NaN; // when this ISN'T NaN, we're making an opening!
        for (int i=0; i<numSteps; i++) {
            // Update the pos of the search-rect.
            searchRect.position = cornerPos + dir*i;
            bool isGround = IsGround(groundDatas, searchRect);
            // We're NOT yet making an opening...
            if (Vector2Extensions.IsNaN(openingStartPos)) {
                // There's NO ground...!
                if (!isGround) {
                    openingStartPos = searchRect.position;
                }
            }
            // We ARE making an opening...!
            else {
                // There IS Ground (OR it's the final search-step)...!
                if (isGround || i==numSteps-1) {
                    AddRoomOpening(side, openingStartPos, searchRect.position);
                    openingStartPos = Vector2Extensions.NaN; // clear this out! So we start looking for a new opening again.
                }
            }
        }
    }
    
    private bool IsGround(List<GroundData> groundDatas, Rect searchRect) {
        for (int i=0; i<groundDatas.Count; i++) {
            // Use top-left aligned ground rect.
            Rect groundRect = groundDatas[i].MyRectTLAligned();
            if (groundRect.Overlaps(searchRect)) { return true; }
        }
        return false;
    }

    private void AddRoomOpening(int side, Vector2 startPos, Vector2 endPos) {
        RoomOpening newObj = new RoomOpening(side, startPos,endPos);
        Openings.Add(newObj);
    }


    public void UpdateNeighbors() {
        Neighbors = new List<RoomNeighborData>();
        // Add a neighbor for EVERY opening! Even ones that DON'T lead anywhere. That way we know where we have null neighbors.
        for (int i=0; i<Openings.Count; i++) {
            RoomData otherLD = MyWorldData.GetRoomNeighbor(this, Openings[i]);
            Neighbors.Add(new RoomNeighborData(otherLD, Openings[i]));
        }
    }







}






