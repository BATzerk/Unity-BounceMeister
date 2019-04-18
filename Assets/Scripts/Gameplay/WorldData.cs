using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class WorldData {
	// Components
	public Dictionary<string, RoomData> roomDatas; // ALL room datas in this world! Loaded up when WE'RE loaded up.
    public List<RoomClusterData> clusters;
	// Properties
	public bool isWorldUnlocked;
	public int worldIndex; // starts at 0.
    public int NumSnacksCollected { get; private set; }
    public int NumSnacksTotal { get; private set; }
    private Rect boundsRectAllRooms; // For the finished game, it doesn't make sense to have TWO rects. But in development, a lot of worlds' rooms aren't used. So we have to make the distinction.
	private Rect boundsRectPlayableRooms; // For determining RoomSelect view and WorldSelect view.

	// Getters
	/** Though we don't position anything special with this value, this IS used under the hood to keep street poses within a reasonable range to avoid float-point issues. */
	public Vector2 CenterPos { get { return boundsRectPlayableRooms.center; } } // TODO: Decide if we're using this or not. (I'd say cut it for now unless we know we want it.)


	// ================================================================
	//  Initialize
	// ================================================================
	public WorldData (int _worldIndex) {
		worldIndex = _worldIndex;
	}

	/** Initialize from scratch, only from file data and stuff. */
	public void Initialize () {
		isWorldUnlocked = true;//SaveStorage.GetInt (SaveKeys.IsWorldUnlocked (WorldIndex)) == 1;

		LoadAllRoomDatas();

		SetAllRoomDatasFundamentalProperties();
//		Debug.Log ("World " + WorldIndex + "  rooms: " + RoomUtils.GetNumRoomsConnectedToStart (roomDatas) + "   stars: " + RoomUtils.GetNumRegularStarsInRoomsConnectedToStart (roomDatas) + "   (" + RoomUtils.GetNumSecretStarsInRoomsConnectedToStart (roomDatas) + " secret stars)");
	}
//	/** Initialize from an existing World; for serialization. */
//	public void InitializeFromExistingWorld (Dictionary<string, Room> _rooms, List<RoomLinkData> _roomLinkDatas) {
//		// Collect serialized RoomDatas for every room in the provided world.
//		Dictionary<string, RoomData> serializedRoomDatas = new Dictionary<string, RoomData> (_rooms.Count);
//		foreach (Room l in _rooms.Values) {
//			RoomData serializedRoomData = l.SerializeAsData();
//			serializedRoomDatas[l.RoomKey] = serializedRoomData;
//		}
//		// Assign my RoomDatas and RoomLinkDatas from the ones we've created.
//		roomDatas = serializedRoomDatas;
//		roomLinkDatas = _roomLinkDatas;
//	}

	/** Call this immediately after we've got our list of RoomDatas. This function is essential: it sets the fundamental properties of all my RoomDatas, as well as my own world bounds rects. */
	public void SetAllRoomDatasFundamentalProperties() {
        UpdateAllRoomsOpeningsAndNeighbors();
        RecalculateRoomClusters();
		UpdateWorldBoundsRects();
//		SetAllRoomDatasPosWorld (); // now that I know MY center position, let's tell all my RoomDatas their posWorld (based on their global position)!
    }

    private void UpdateAllRoomsOpeningsAndNeighbors() {
        foreach (RoomData rd in roomDatas.Values) { rd.CalculateOpenings(); }
        foreach (RoomData rd in roomDatas.Values) { rd.UpdateNeighbors(); }
    }
	private void UpdateWorldBoundsRects () {
		// Calculate my boundsRectAllRooms so I can know when the camera is lookin' at me!
		boundsRectAllRooms = new Rect (0,0, 0,0);
		boundsRectPlayableRooms = new Rect (0,0, 0,0);
		foreach (RoomData rd in roomDatas.Values) {
			AddRoomBoundsToWorldBoundsRects (rd);
		}
		// Hey, politely check if our playableRooms rect is still nothing (which will happen if we DON'T have a starting room of the stipulated name). If so, make playable-rooms rect same as all-rooms rect.
		if (boundsRectPlayableRooms == new Rect ()) {
			boundsRectPlayableRooms = new Rect(boundsRectAllRooms);
		}
//		// Now round my rects' values to even numbers!
//		MathUtils.RoundRectValuesToEvenInts (ref boundsRectAllRooms);
//		MathUtils.RoundRectValuesToEvenInts (ref boundsRectPlayableRooms);
	}
	private void AddRoomBoundsToWorldBoundsRects (RoomData rd) {
		Rect ldBounds = rd.BoundsGlobal;
		// Add ALL rooms to the allRooms list.
		boundsRectAllRooms = MathUtils.GetCompoundRectangle (boundsRectAllRooms, ldBounds);
		// Only add rooms IN CLUSTERS for the playableRooms list.
		if (rd.IsInCluster) {
			boundsRectPlayableRooms = MathUtils.GetCompoundRectangle (boundsRectPlayableRooms, ldBounds);
		}
	}
//	/** Once we know where the center of the playable world is, set the posWorld value for all my rooms! */
//	private void SetAllRoomDatasPosWorld () {
//		foreach (RoomData rd in roomDatas.Values) {
//			rd.SetPosWorld (new Vector2 (rd.posGlobal.x-CenterPos.x, rd.posGlobal.y-CenterPos.y));
//		}
//	}
    private void RecalculateRoomClusters() {
        // Reset Rooms' ClusterIndex.
        foreach (RoomData rd in roomDatas.Values) {
            rd.ClusterIndex = -1;
            rd.WasUsedInSearchAlgorithm = false;
        }
        
        // Remake Clusters!
        clusters = new List<RoomClusterData>();
        foreach (RoomData rd in roomDatas.Values) {
            if (rd.WasUsedInSearchAlgorithm) { continue; } // Already used this fella? Skip 'em.
            // If this is a ClusterStart room...!
            if (rd.isClustStart) {
                // Add a new Cluster, and populate it!
                RoomClusterData newClust = new RoomClusterData(clusters.Count);
                clusters.Add(newClust);
                RecursivelyAddRoomToCluster(rd, newClust);
                // Update the Cluster's values!
                newClust.UpdateCollectablesCounts();
            }
        }
        // Reset Rooms' WasUsedInSearchAlgorithm.
        foreach (RoomData rd in roomDatas.Values) {
            rd.WasUsedInSearchAlgorithm = false;
        }
        
    }
    private void RecursivelyAddRoomToCluster(RoomData rd, RoomClusterData cluster) {
        if (rd.WasUsedInSearchAlgorithm) { return; } // This RoomData was used? Ignore it.
        // Update Room's values, and add to Cluster's list!
        rd.ClusterIndex = cluster.ClusterIndex;
        rd.WasUsedInSearchAlgorithm = true;
        cluster.rooms.Add(rd);
        // Now try for all its neighbors!
        for (int i=0; i<rd.Neighbors.Count; i++) {
            if (rd.Neighbors[i].IsRoomTo) {
                RecursivelyAddRoomToCluster(rd.Neighbors[i].RoomTo, cluster);
            }
        }
    }
    

	// ================================================================
	//  Getters
	// ================================================================
	public bool IsWorldUnlocked { get { return isWorldUnlocked; } }
	public int WorldIndex { get { return worldIndex; } }
	public Dictionary<string, RoomData> RoomDatas { get { return roomDatas; } }
	public Rect BoundsRectAllRooms { get { return boundsRectAllRooms; } }
	public Rect BoundsRectPlayableRooms { get { return boundsRectPlayableRooms; } }


    public RoomData GetRoomData (string key, bool doMakeOneIfItDoesntExist=false) {
		if (roomDatas.ContainsKey(key)) {
			return roomDatas[key];
		}
		if (doMakeOneIfItDoesntExist) {
            return AddNewRoom(key);
		}
		else {
			return null;
		}
	}

	private Vector2 GetBrandNewRoomPos() {
		// Return where the MapEditor camera was last looking!!
		return new Vector2 (SaveStorage.GetFloat (SaveKeys.MapEditor_CameraPosX), SaveStorage.GetFloat (SaveKeys.MapEditor_CameraPosY));
	}

	/** Creates and returns a rect that's JUST made up of these rooms. */
	public Rect GetBoundsOfRooms (string[] _roomKeys) {
		Rect returnRect = new Rect (0,0, 0,0);
		for (int i=0; i<_roomKeys.Length; i++) {
			RoomData rd = GetRoomData (_roomKeys [i]);
			if (rd == null) { Debug.LogError ("Oops! This room doesn't exist in this world! " + _roomKeys[i] + ", world " + worldIndex); continue; }
			returnRect = MathUtils.GetCompoundRectangle (returnRect, rd.BoundsGlobal);
		}
		return returnRect;
	}
    

    public RoomData GetRoomNeighbor(RoomData originLD, RoomOpening opening) {
        // Get the neighbor, ignoring ITS openings.
        RoomData neighbor = GetRoomAtSide(originLD, opening.posCenter, opening.side);
        if (neighbor == null) { return null; } // NOTHing there? Return null!
        // Ok, if this neighbor HAS a corresponding opening, return it!!
        Rect opRect = opening.GetCollRectGlobal(originLD.PosGlobal);
        for (int i=0; i<neighbor.Openings.Count; i++) {
            Rect otherOpRect = neighbor.Openings[i].GetCollRectGlobal(neighbor.PosGlobal);
            if (opRect.Overlaps(otherOpRect)) {
                return neighbor;
            }
        }
        // Ah, NO corresponding opening. Return null.
        return null;
    }
    // TODO: Replace this with known neighbors now!!
	/// Use this for debug room-jumping. Will return room at side, irrespective of Player's position.
	public RoomData Debug_GetSomeRoomAtSide(RoomData originLD, int side) {
		// Find where a point in this next room would be. Then return the RoomData with that point in it!
		Vector2Int dir = MathUtils.GetDir(side);
		Vector2 originSize = originLD.BoundsGlobal.size;
		Vector2 searchOrigin = originLD.BoundsGlobal.position;//.center;
		searchOrigin += new Vector2(dir.x*(originSize.x+1f), dir.y*(originSize.y+1f)); // +1 so the rooms don't have to be directly touching; we'll allow even a small gap.
		// Instead of just looking at one point, put out TWO feelers, as rooms can be irregularly aligned.
		Vector2[] searchPoints = new Vector2[3];
		if (side==Sides.L || side==Sides.R) {
			searchPoints[1] = searchOrigin;
			searchPoints[0] = searchOrigin + new Vector2(0, originSize.y*0.4f);
			searchPoints[2] = searchOrigin - new Vector2(0, originSize.y*0.4f);
		}
		else {
			searchPoints[1] = searchOrigin;
			searchPoints[0] = searchOrigin + new Vector2(originSize.x*0.4f, 0);
			searchPoints[2] = searchOrigin - new Vector2(originSize.x*0.4f, 0);
		}
		foreach (Vector2 point in searchPoints) {
			RoomData ldHere = GetRoomWithPoint(point);
			if (ldHere != null) { return ldHere; }
		}
		return null;
	}
	/** FOR NOW, our room-traversal system is dead simple. Nothing's precalculated. We search for the next room the moment we exit one. */
	public RoomData GetRoomAtSide(RoomData originLD, Vector2 searchPos, int side) {
        // Lock searchPos on the EDGE of the origin room-- we only wanna use its x/y value parallel to the next room.
        searchPos = LockPosOnRoomEdge(originLD, searchPos, side);
		// Find where a point in this next room would be. Then return the RoomData with that point in it!
        Vector2Int dir = MathUtils.GetDir(side);
		Vector2 searchPoint = originLD.posGlobal + searchPos + dir*1f; // look ahead a few feet into the next room area.
		RoomData ldHere = GetRoomWithPoint(searchPoint);
		if (ldHere != null) { return ldHere; }
		return null;
	}
	public RoomData GetRoomWithPoint(Vector2 point) {
		foreach (RoomData rd in roomDatas.Values) {
			// HACK Temp conversion business
			Rect bounds = new Rect(rd.BoundsGlobal);
			bounds.position -= bounds.size*0.5f;
			if (bounds.Contains(point)) { return rd; }
		}
		return null; // Nah, nobody here.
    }
    /// Takes a LOCAL pos, and sets its x or y to the exact provided side of this room. E.g. Room's 500 tall, and pass in (0,0) and side Top: will return (0,250).
    public Vector2 LockPosOnRoomEdge(RoomData rd, Vector2 pos, int side) {
        switch (side) {
            case Sides.B: return new Vector2(pos.x, rd.BoundsLocal.yMin);
            case Sides.T: return new Vector2(pos.x, rd.BoundsLocal.yMax);
            case Sides.L: return new Vector2(rd.BoundsLocal.xMin, pos.y);
            case Sides.R: return new Vector2(rd.BoundsLocal.xMax, pos.y);
            default: Debug.LogError("Side not recongized: " + side); return pos; // Hmm.
        }
    }

	public string GetUnusedRoomKey(string prefix="NewRoom") {
		int suffixIndex = 0;
		int safetyCount = 0;
		while (safetyCount++ < 99) { // 'Cause I'm feeling cautious. :)
			string newKey = prefix + suffixIndex;
			if (!roomDatas.ContainsKey(newKey)) { return newKey; }
			suffixIndex ++;
		}
		Debug.LogError("Wowza. Somehow got caught in an infinite naming loop. Either you have 99 rooms named NewRoom0-99, or bad code.");
		return "NewRoom";
    }


    // ================================================================
    //  Doers
    // ================================================================
    public void UpdateNumSnacks() {
        NumSnacksTotal = 0;
        NumSnacksCollected = 0;
        foreach (RoomData rd in roomDatas.Values) {
            rd.UpdateNumSnacks();
            NumSnacksTotal += rd.NumSnacksTotal;
            NumSnacksCollected += rd.NumSnacksCollected;
        }
    }
    //public void UpdateNumSnacksTotal() {
    //    NumSnacksTotal = 0;
    //    foreach (RoomData rd in roomDatas.Values) {
    //        foreach (PropData pd in rd.allPropDatas) {
    //            if (pd is SnackData) { NumSnacksTotal++; }
    //        }
    //    }
    //}

    // ================================================================
    //  Events
    // ================================================================
    public void OnPlayerEatSnack() {
        // Update counts!
        UpdateNumSnacks(); // CANDO #optimization: Only update the snacks in the room we ate it in.
        // Dispatch event!
        GameManagers.Instance.EventManager.OnSnacksCollectedChanged(WorldIndex);
    }


    // ================================================================
    //  RoomDatas
    // ================================================================
    /** Makes a RoomData for every room file in our world's rooms folder!! */
    private void LoadAllRoomDatas () {
		roomDatas = new Dictionary<string, RoomData>();

		string worldPath = FilePaths.WorldFileAddress (worldIndex);
		DirectoryInfo info = new DirectoryInfo(worldPath);
		if (info.Exists) {
			FileInfo[] fileInfos = info.GetFiles();
			foreach (FileInfo file in fileInfos) {
				if (file.Name.EndsWith(".meta")) { continue; } // Ignore .meta files (duh).
				if (file.Name == ".DS_Store") { // Sigh, Macs.
					continue;
				}
				string fileName = file.Name.Substring(0, file.Name.Length-4); // Remove the ".txt".
				if (fileName == "_RoomLinks") { continue; } // Ignore the _RoomLinks.txt file.
				string roomKey = fileName;
				AddRoomFromFile (roomKey);
			}
		}
		else {
			Debug.LogError("World folder not found! " + worldIndex);
        }

        UpdateNumSnacks();

        //		if (File.Exists(filePath)) {
        //			StreamReader file = File.OpenText(filePath);
        //			string wholeFile = file.ReadToEnd();
        //			file.Close();
        //			return TextUtils.GetStringArrayFromStringWithLineBreaks(wholeFile, StringSplitOptions.None);
        //		}

        //		TextAsset[] roomFiles = Resources.LoadAll<TextAsset> (worldPath);
        //		foreach (TextAsset t in roomFiles) {
        //			if (t.name == "_RoomLinks") { continue; } // Ignore the _RoomLinks.txt file.
        //			string roomKey = t.name;
        //			AddRoomFromFile (roomKey);
        //		}
    }
	private RoomData AddRoomFromFile(string roomKey) {
		RoomData newRoomData = new RoomData(this, roomKey);
		RoomSaverLoader.LoadRoomDataFromItsFile (newRoomData);
		roomDatas.Add(roomKey, newRoomData);
		return newRoomData;
	}
	public void ReloadRoomData(string roomKey) {
        // Safety check.
        if (!roomDatas.ContainsKey(roomKey)) {
            Debug.LogWarning("Can't reload RoomData; doesn't exist. World: " + worldIndex + ", RoomKey: " + roomKey);
            return;
        }
		RoomData roomData = GetRoomData(roomKey);
		// Reload all of this roomData's contents! (WITHOUT remaking it or anything-- it's important to retain all references to it!)
		RoomSaverLoader.LoadRoomDataFromItsFile(roomData);
        // Refresh fundamental world/room properties!
        SetAllRoomDatasFundamentalProperties();
	}


	private RoomData AddNewRoom(string roomKey) {
        if (GetRoomData(roomKey) != null) { // Safety check.
            Debug.LogError("Whoa, trying to make a Room with key: " + roomKey + ", but one already exists!");
            return null;
        }
		// Make/populate/add it!
		RoomData rd = new RoomData(this, roomKey);
        rd.SetPosGlobal(GetBrandNewRoomPos());
        RoomSaverLoader.AddEmptyRoomElements(ref rd);
        roomDatas.Add(roomKey, rd);
        // Refresh fundamental world/room properties!
        SetAllRoomDatasFundamentalProperties();
        // Save the file!
        RoomSaverLoader.SaveRoomFile(rd);
		return rd;
	}


	// ================================================================
	//	Room Files
	// ================================================================
	public void MoveRoomFileToWorldFolder(string roomKey, int worldIndexTo) {
		string destDirectory = FilePaths.WorldFileAddress(worldIndexTo);
		MoveRoomFileToFolder(roomKey, destDirectory);
	}
	public void MoveRoomFileToTrashFolder(string roomKey) {
		string destDirectory = FilePaths.WorldTrashFileAddress(worldIndex);
		MoveRoomFileToFolder(roomKey, destDirectory);
	}
	private void MoveRoomFileToFolder (string roomKey, string destDirectory) {
		string sourceNameFull = FilePaths.WorldFileAddress(worldIndex) + roomKey + ".txt";
        // Directory doesn't exist? Make it!
        if (!Directory.Exists(destDirectory)) {
            Directory.CreateDirectory(destDirectory);
            Debug.LogWarning("Created directory: \"" + destDirectory + "\"");
        }
        string destNameFull = destDirectory + roomKey + ".txt";
		try {
			File.Move(sourceNameFull, destNameFull);
		}
		catch (System.Exception e) { Debug.LogError ("Error moving room file to world folder: " + sourceNameFull + " to " + destNameFull + ". " + e.ToString ()); }
	}



}


/*
    private void SetRoomsIsConnectedToStart() {
        // Tell all of them they're NOT first.
        foreach (RoomData rd in roomDatas.Values) { rd.isConnectedToStart = false; }
        // Get the special ones that are.
        RoomData startRoom = GetRoomData (GameProperties.GetFirstRoomName (worldIndex));
        // Firsht off, if this start room doesn't exist, don't continue doing anything. This whole world is total anarchy.
        if (startRoom != null) {
            List<RoomData> roomsConnectedToStart = RoomUtils.GetRoomsConnectedToRoom (this, startRoom);
            // Tell these rooms they're connected to the start dude!
            startRoom.isConnectedToStart = true;
            for (int i=0; i<roomsConnectedToStart.Count; i++) {
                roomsConnectedToStart [i].isConnectedToStart = true;
            }
        }
    }
    */


