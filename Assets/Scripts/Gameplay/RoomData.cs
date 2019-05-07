using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RoomData {
	// Components!
    public  List<PropData> allPropDatas { get; private set; }
    public  CameraBoundsData cameraBoundsData;
    private List<GroundData> groundDatas;
    public  List<RoomDoorData> roomDoorDatas { get; private set; }
    public  List<SnackData> snackDatas { get; private set; }
	// Properties
    public bool HasPlayerBeenHere { get; private set; } // false until the player enters me for the first time!
    public bool isClustStart;// { get; private set; }
    public bool IsSecret { get; private set; } // TODO: Make editable in MapEditor. and show color in RoomTile.// if TRUE, I won't show up in the MiniMap until Player's been here.
    public bool WasUsedInSearchAlgorithm { get; set; }
    public int DesignerFlag { get; private set; } // for the room designer! We can flag any room to be like "testing" or "good" or etc.
    public RoomAddress MyAddress { get; private set; }
    public SnackCount SnackCount = new SnackCount();
    public Vector2 PosGlobal { get; private set; } // my position, global to ALL worlds! These values get big (up to around 70,000)!
    public List<RoomOpening> Openings { get; private set; }
    public HashSet<RoomData> NeighborRooms { get; private set; } // All RoomDatas I have Openings to.
    
    // Getters
    public Rect BoundsLocal { get { return new Rect(cameraBoundsData.myRect); } } // TODO: Try not copying the rect. // Currently, the camera bounds and room bounds are one in the same.
 //   public Rect BoundsLocal { get { return new Rect(cameraBoundsData.myRect.center, cameraBoundsData.myRect.size); } } // Currently, the camera bounds and room bounds are one in the same.
	public Rect BoundsGlobal { get { return new Rect(cameraBoundsData.myRect.center+PosGlobal, cameraBoundsData.myRect.size); } }
    //public Rect BoundsGlobal { get { return new Rect(BoundsLocal.position+posGlobal, BoundsLocal.size); } }
	public WorldData MyWorldData { get; private set; }
    public int WorldIndex { get { return MyAddress.world; } }
    //public int ClustIndex { get { return MyAddress.clust; } }TODO: Clarify this!
    public string RoomKey { get { return MyAddress.room; } }
    public bool IsInCluster { get { return MyCluster != null; } }
    //public bool IsInCluster { get { return ClustIndex != -1; } }
    //public RoomClusterData MyCluster { get { return ClustIndex<0 ? null : MyWorldData.clusters[ClustIndex]; } }
    public RoomClusterData MyCluster { get; private set; }
    /// Returns the first PlayerStart in our list.
    public Vector2 DefaultPlayerStartPos() {
        foreach (PropData pd in allPropDatas) {
            if (pd is PlayerStartData) {
                return pd.pos;
            }
        }
        return Vector2.zero; // Oops.
    }
    public RoomDoorData GetRoomDoor(string doorID) {
        foreach (RoomDoorData rdd in roomDoorDatas) {
            if (rdd.myID == doorID) { return rdd; }
        }
        return null;
    }
    public Vector2 GetRoomDoorPos(string doorID, bool doPrintWarning=true) {
        RoomDoorData rdd = GetRoomDoor(doorID);
        if (rdd != null) { return rdd.pos; }
        // Oops, no door with this ID.
        if (doPrintWarning) {
            Debug.LogWarning("Oops, GetRoomDoorPos! No RoomDoor with doorID. Room: W" + WorldIndex + " " + RoomKey + ", doorID: " + doorID);
        }
        return DefaultPlayerStartPos();
    }
    //public bool AreEdiblesLeft() {
    //    return NumSnacksEaten < NumSnacksTotal;
    //}
    public void RefreshSnackCount() {
        SnackCount.Refresh(this);
    }

	// Setters
    //public void SetClustIndex(int _clustIndex) {
    //    MyAddress = new RoomAddress(MyAddress.world, _clustIndex, MyAddress.room);
    //}
    public void SetMyCluster(RoomClusterData cluster) {
        MyCluster = cluster;
    }
	public void SetPosGlobal (Vector2 _posGlobal) {
		// Round my posGlobal values to even numbers! For snapping rooms together more easily.
		PosGlobal = _posGlobal;//new Vector2 (Mathf.Round(_posGlobal.x*0.5f)*2f, Mathf.Round (_posGlobal.y*0.5f)*2f);
	}
    public void SetDesignerFlag (int _designerFlag) {
        DesignerFlag = _designerFlag;
    }
    public void SetIsSecret(bool val) {
        IsSecret = val;
    }


	// ================================================================
	//  Initialize
	// ================================================================
	public RoomData(WorldData _worldData, string _key) {
		MyWorldData = _worldData;
		MyAddress = new RoomAddress(MyWorldData.worldIndex, -1, _key);
        IsSecret = false;
        HasPlayerBeenHere = SaveStorage.GetBool(SaveKeys.HasPlayerBeenInRoom(this), false);
        
		// Initialize all my PropData lists.
		ClearAllPropDataLists();
        Openings = new List<RoomOpening>();
	}
	public void ClearAllPropDataLists() {
		allPropDatas = new List<PropData>();
        cameraBoundsData = new CameraBoundsData();
        groundDatas = new List<GroundData>();
        roomDoorDatas = new List<RoomDoorData>();
        snackDatas = new List<SnackData>();
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
    public void AddPropData(PropData propData) {
        allPropDatas.Add(propData);
        //switch (propData.GetType()) {
            
        //}
        // TODO: Can we use a switch statement?
        if (propData is CameraBoundsData) { cameraBoundsData = propData as CameraBoundsData; }
        else if (propData is GroundData) { groundDatas.Add(propData as GroundData); }
        else if (propData is RoomDoorData) { roomDoorDatas.Add(propData as RoomDoorData); }
        else if (propData is SnackData) { snackDatas.Add(propData as SnackData); }
    }
    public void CalculateOpenings() {
        // Calculate 'em!
        Openings = new List<RoomOpening>();
        for (int side=0; side<4; side++) {
            AddRoomOpeningsAtSide(side);
        }
    }
    private void AddRoomOpeningsAtSide(int side) {
        // CANDO: #optimization. ONLY add Grounds that are on the sides.
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
            bool isGround = IsGround(searchRect);
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
    
    private bool IsGround(Rect searchRect) {
        for (int i=0; i<groundDatas.Count; i++) {
            // Use top-left aligned ground rect.
            Rect groundRect = groundDatas[i].MyRectTLAligned();
            if (groundRect.Overlaps(searchRect)) { return true; }
        }
        return false;
    }

    private void AddRoomOpening(int side, Vector2 startPos, Vector2 endPos) {
        RoomOpening newObj = new RoomOpening(this, side, startPos,endPos);
        Openings.Add(newObj);
    }

    public void UpdateNeighbors() {
        NeighborRooms = new HashSet<RoomData>();
        for (int i=0; i<Openings.Count; i++) {
            RoomData otherRoom = MyWorldData.GetRoomNeighbor(this, Openings[i]);
            // If there IS a neighbor...!
            if (otherRoom != null) {
                // Add the neighbor.
                if (!NeighborRooms.Contains(otherRoom)) { NeighborRooms.Add(otherRoom); }
                // Set the opening's neighbor.
                Openings[i].SetRoomTo(otherRoom);
            }
        }
    }


}





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