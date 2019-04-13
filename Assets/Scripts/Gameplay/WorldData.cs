﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class WorldData {
	// Components
	public Dictionary<string, LevelData> levelDatas; // ALL level datas in this world! Loaded up when WE'RE loaded up.
    public List<LevelClusterData> clusters;
	// Properties
	public bool isWorldUnlocked;
	public int worldIndex; // starts at 0.
    public int NumSnacksCollected { get; private set; }
    public int NumSnacksTotal { get; private set; }
    private Rect boundsRectAllLevels; // For the finished game, it doesn't make sense to have TWO rects. But in development, a lot of worlds' levels aren't used. So we have to make the distinction.
	private Rect boundsRectPlayableLevels; // For determining LevelSelect view and WorldSelect view.

	// Getters
	/** Though we don't position anything special with this value, this IS used under the hood to keep street poses within a reasonable range to avoid float-point issues. */
	public Vector2 CenterPos { get { return boundsRectPlayableLevels.center; } } // TODO: Decide if we're using this or not. (I'd say cut it for now unless we know we want it.)


	// ================================================================
	//  Initialize
	// ================================================================
	public WorldData (int _worldIndex) {
		worldIndex = _worldIndex;
	}

	/** Initialize from scratch, only from file data and stuff. */
	public void Initialize () {
		isWorldUnlocked = true;//SaveStorage.GetInt (SaveKeys.IsWorldUnlocked (WorldIndex)) == 1;

		LoadAllLevelDatas();

		SetAllLevelDatasFundamentalProperties();
//		Debug.Log ("World " + WorldIndex + "  lvls: " + LevelUtils.GetNumLevelsConnectedToStart (levelDatas) + "   stars: " + LevelUtils.GetNumRegularStarsInLevelsConnectedToStart (levelDatas) + "   (" + LevelUtils.GetNumSecretStarsInLevelsConnectedToStart (levelDatas) + " secret stars)");
	}
//	/** Initialize from an existing World; for serialization. */
//	public void InitializeFromExistingWorld (Dictionary<string, Level> _levels, List<LevelLinkData> _levelLinkDatas) {
//		// Collect serialized LevelDatas for every level in the provided world.
//		Dictionary<string, LevelData> serializedLevelDatas = new Dictionary<string, LevelData> (_levels.Count);
//		foreach (Level l in _levels.Values) {
//			LevelData serializedLevelData = l.SerializeAsData();
//			serializedLevelDatas[l.LevelKey] = serializedLevelData;
//		}
//		// Assign my LevelDatas and LevelLinkDatas from the ones we've created.
//		levelDatas = serializedLevelDatas;
//		levelLinkDatas = _levelLinkDatas;
//	}

	/** Call this immediately after we've got our list of LevelDatas. This function is essential: it sets the fundamental properties of all my LevelDatas, as well as my own world bounds rects. */
	public void SetAllLevelDatasFundamentalProperties() {
        UpdateAllLevelsOpeningsAndNeighbors();
        RecalculateLevelClusters();
		UpdateWorldBoundsRects();
//		SetAllLevelDatasPosWorld (); // now that I know MY center position, let's tell all my LevelDatas their posWorld (based on their global position)!
    }

    private void UpdateAllLevelsOpeningsAndNeighbors() {
        foreach (LevelData ld in levelDatas.Values) { ld.CalculateOpenings(); }
        foreach (LevelData ld in levelDatas.Values) { ld.UpdateNeighbors(); }
    }
	private void UpdateWorldBoundsRects () {
		// Calculate my boundsRectAllLevels so I can know when the camera is lookin' at me!
		boundsRectAllLevels = new Rect (0,0, 0,0);
		boundsRectPlayableLevels = new Rect (0,0, 0,0);
		foreach (LevelData ld in levelDatas.Values) {
			AddLevelBoundsToWorldBoundsRects (ld);
		}
		// Hey, politely check if our playableLevels rect is still nothing (which will happen if we DON'T have a starting level of the stipulated name). If so, make playable-lvls rect same as all-lvls rect.
		if (boundsRectPlayableLevels == new Rect ()) {
			boundsRectPlayableLevels = new Rect(boundsRectAllLevels);
		}
//		// Now round my rects' values to even numbers!
//		MathUtils.RoundRectValuesToEvenInts (ref boundsRectAllLevels);
//		MathUtils.RoundRectValuesToEvenInts (ref boundsRectPlayableLevels);
	}
	private void AddLevelBoundsToWorldBoundsRects (LevelData ld) {
		Rect ldBounds = ld.BoundsGlobal;
		// Add ALL levels to the allLevels list.
		boundsRectAllLevels = MathUtils.GetCompoundRectangle (boundsRectAllLevels, ldBounds);
		// Only add levels IN CLUSTERS for the playableLevels list.
		if (ld.IsInCluster) {
			boundsRectPlayableLevels = MathUtils.GetCompoundRectangle (boundsRectPlayableLevels, ldBounds);
		}
	}
//	/** Once we know where the center of the playable world is, set the posWorld value for all my levels! */
//	private void SetAllLevelDatasPosWorld () {
//		foreach (LevelData ld in levelDatas.Values) {
//			ld.SetPosWorld (new Vector2 (ld.posGlobal.x-CenterPos.x, ld.posGlobal.y-CenterPos.y));
//		}
//	}
    private void RecalculateLevelClusters() {
        // Reset Levels' ClusterIndex.
        foreach (LevelData ld in levelDatas.Values) {
            ld.ClusterIndex = -1;
            ld.WasUsedInSearchAlgorithm = false;
        }
        
        // Remake Clusters!
        clusters = new List<LevelClusterData>();
        foreach (LevelData ld in levelDatas.Values) {
            if (ld.WasUsedInSearchAlgorithm) { continue; } // Already used this fella? Skip 'em.
            // If this is a ClusterStart level...!
            if (ld.isClustStart) {
                // Add a new Cluster, and populate it!
                LevelClusterData newClust = new LevelClusterData(clusters.Count);
                clusters.Add(newClust);
                RecursivelyAddLevelToCluster(ld, newClust);
                // Update the Cluster's values!
                newClust.UpdateCollectablesCounts();
            }
        }
        // Reset Levels' WasUsedInSearchAlgorithm.
        foreach (LevelData ld in levelDatas.Values) {
            ld.WasUsedInSearchAlgorithm = false;
        }
        
    }
    private void RecursivelyAddLevelToCluster(LevelData ld, LevelClusterData cluster) {
        if (ld.WasUsedInSearchAlgorithm) { return; } // This LevelData was used? Ignore it.
        // Update Level's values, and add to Cluster's list!
        ld.ClusterIndex = cluster.ClusterIndex;
        ld.WasUsedInSearchAlgorithm = true;
        cluster.levels.Add(ld);
        // Now try for all its neighbors!
        for (int i=0; i<ld.Neighbors.Count; i++) {
            if (ld.Neighbors[i].IsLevelTo) {
                RecursivelyAddLevelToCluster(ld.Neighbors[i].LevelTo, cluster);
            }
        }
    }
    

	// ================================================================
	//  Getters
	// ================================================================
	public bool IsWorldUnlocked { get { return isWorldUnlocked; } }
	public int WorldIndex { get { return worldIndex; } }
	public Dictionary<string, LevelData> LevelDatas { get { return levelDatas; } }
	public Rect BoundsRectAllLevels { get { return boundsRectAllLevels; } }
	public Rect BoundsRectPlayableLevels { get { return boundsRectPlayableLevels; } }


    public LevelData GetLevelData (string key, bool doMakeOneIfItDoesntExist=false) {
		if (levelDatas.ContainsKey(key)) {
			return levelDatas[key];
		}
		if (doMakeOneIfItDoesntExist) {
            return AddNewLevel(key);
		}
		else {
			return null;
		}
	}

	private Vector2 GetBrandNewLevelPos() {
		// Return where the MapEditor camera was last looking!!
		return new Vector2 (SaveStorage.GetFloat (SaveKeys.MapEditor_CameraPosX), SaveStorage.GetFloat (SaveKeys.MapEditor_CameraPosY));
	}

	/** Creates and returns a rect that's JUST made up of these levels. */
	public Rect GetBoundsOfLevels (string[] _levelKeys) {
		Rect returnRect = new Rect (0,0, 0,0);
		for (int i=0; i<_levelKeys.Length; i++) {
			LevelData ld = GetLevelData (_levelKeys [i]);
			if (ld == null) { Debug.LogError ("Oops! This level doesn't exist in this world! " + _levelKeys[i] + ", world " + worldIndex); continue; }
			returnRect = MathUtils.GetCompoundRectangle (returnRect, ld.BoundsGlobal);
		}
		return returnRect;
	}
    

    public LevelData GetLevelNeighbor(LevelData originLD, LevelOpening opening) {
        // Get the neighbor, ignoring ITS openings.
        LevelData neighbor = GetLevelAtSide(originLD, opening.posCenter, opening.side);
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
	/// Use this for debug level-jumping. Will return level at side, irrespective of Player's position.
	public LevelData Debug_GetSomeLevelAtSide(LevelData originLD, int side) {
		// Find where a point in this next level would be. Then return the LevelData with that point in it!
		Vector2Int dir = MathUtils.GetDir(side);
		Vector2 originSize = originLD.BoundsGlobal.size;
		Vector2 searchOrigin = originLD.BoundsGlobal.position;//.center;
		searchOrigin += new Vector2(dir.x*(originSize.x+1f), dir.y*(originSize.y+1f)); // +1 so the levels don't have to be directly touching; we'll allow even a small gap.
		// Instead of just looking at one point, put out TWO feelers, as levels can be irregularly aligned.
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
			LevelData ldHere = GetLevelWithPoint(point);
			if (ldHere != null) { return ldHere; }
		}
		return null;
	}
	/** FOR NOW, our level-traversal system is dead simple. Nothing's precalculated. We search for the next level the moment we exit one. */
	public LevelData GetLevelAtSide(LevelData originLD, Vector2 searchPos, int side) {
        // Lock searchPos on the EDGE of the origin level-- we only wanna use its x/y value parallel to the next level.
        searchPos = LockPosOnLevelEdge(originLD, searchPos, side);
		// Find where a point in this next level would be. Then return the LevelData with that point in it!
        Vector2Int dir = MathUtils.GetDir(side);
		Vector2 searchPoint = originLD.posGlobal + searchPos + dir*1f; // look ahead a few feet into the next level area.
		LevelData ldHere = GetLevelWithPoint(searchPoint);
		if (ldHere != null) { return ldHere; }
		return null;
	}
	public LevelData GetLevelWithPoint(Vector2 point) {
		foreach (LevelData ld in levelDatas.Values) {
			// HACK Temp conversion business
			Rect bounds = new Rect(ld.BoundsGlobal);
			bounds.position -= bounds.size*0.5f;
			if (bounds.Contains(point)) { return ld; }
		}
		return null; // Nah, nobody here.
    }
    /// Takes a LOCAL pos, and sets its x or y to the exact provided side of this level. E.g. Level's 500 tall, and pass in (0,0) and side Top: will return (0,250).
    public Vector2 LockPosOnLevelEdge(LevelData ld, Vector2 pos, int side) {
        switch (side) {
            case Sides.B: return new Vector2(pos.x, ld.BoundsLocal.yMin);
            case Sides.T: return new Vector2(pos.x, ld.BoundsLocal.yMax);
            case Sides.L: return new Vector2(ld.BoundsLocal.xMin, pos.y);
            case Sides.R: return new Vector2(ld.BoundsLocal.xMax, pos.y);
            default: Debug.LogError("Side not recongized: " + side); return pos; // Hmm.
        }
    }

	public string GetUnusedLevelKey(string prefix="NewLevel") {
		int suffixIndex = 0;
		int safetyCount = 0;
		while (safetyCount++ < 99) { // 'Cause I'm feeling cautious. :)
			string newKey = prefix + suffixIndex;
			if (!levelDatas.ContainsKey(newKey)) { return newKey; }
			suffixIndex ++;
		}
		Debug.LogError("Wowza. Somehow got caught in an infinite naming loop. Either you have 99 levels named NewLevel0-99, or bad code.");
		return "NewLevel";
    }


    // ================================================================
    //  Doers
    // ================================================================
    public void UpdateNumSnacks() {
        NumSnacksTotal = 0;
        NumSnacksCollected = 0;
        foreach (LevelData ld in levelDatas.Values) {
            ld.UpdateNumSnacks();
            NumSnacksTotal += ld.NumSnacksTotal;
            NumSnacksCollected += ld.NumSnacksCollected;
        }
    }
    //public void UpdateNumSnacksTotal() {
    //    NumSnacksTotal = 0;
    //    foreach (LevelData ld in levelDatas.Values) {
    //        foreach (PropData pd in ld.allPropDatas) {
    //            if (pd is SnackData) { NumSnacksTotal++; }
    //        }
    //    }
    //}

    // ================================================================
    //  Events
    // ================================================================
    public void OnPlayerEatSnack() {
        // Update counts!
        UpdateNumSnacks(); // CANDO #optimization: Only update the snacks in the level we ate it in.
        // Dispatch event!
        GameManagers.Instance.EventManager.OnSnacksCollectedChanged(WorldIndex);
    }


    // ================================================================
    //  LevelDatas
    // ================================================================
    /** Makes a LevelData for every level file in our world's levels folder!! */
    private void LoadAllLevelDatas () {
		levelDatas = new Dictionary<string, LevelData>();

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
				if (fileName == "_LevelLinks") { continue; } // Ignore the _LevelLinks.txt file.
				string levelKey = fileName;
				AddLevelFromFile (levelKey);
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

        //		TextAsset[] levelFiles = Resources.LoadAll<TextAsset> (worldPath);
        //		foreach (TextAsset t in levelFiles) {
        //			if (t.name == "_LevelLinks") { continue; } // Ignore the _LevelLinks.txt file.
        //			string levelKey = t.name;
        //			AddLevelFromFile (levelKey);
        //		}
    }
	private LevelData AddLevelFromFile(string levelKey) {
		LevelData newLevelData = new LevelData(this, levelKey);
		LevelSaverLoader.LoadLevelDataFromItsFile (newLevelData);
		levelDatas.Add(levelKey, newLevelData);
		return newLevelData;
	}
	public void ReloadLevelData(string levelKey) {
        // Safety check.
        if (!levelDatas.ContainsKey(levelKey)) {
            Debug.LogWarning("Can't reload LevelData; doesn't exist. World: " + worldIndex + ", LevelKey: " + levelKey);
            return;
        }
		LevelData levelData = GetLevelData(levelKey);
		// Reload all of this levelData's contents! (WITHOUT remaking it or anything-- it's important to retain all references to it!)
		LevelSaverLoader.LoadLevelDataFromItsFile(levelData);
        // Refresh fundamental world/level properties!
        SetAllLevelDatasFundamentalProperties();
	}


	private LevelData AddNewLevel(string levelKey) {
        if (GetLevelData(levelKey) != null) { // Safety check.
            Debug.LogError("Whoa, trying to make a Level with key: " + levelKey + ", but one already exists!");
            return null;
        }
		// Make/populate/add it!
		LevelData ld = new LevelData(this, levelKey);
        ld.SetPosGlobal(GetBrandNewLevelPos());
        LevelSaverLoader.AddEmptyLevelElements(ref ld);
        levelDatas.Add(levelKey, ld);
        // Refresh fundamental world/level properties!
        SetAllLevelDatasFundamentalProperties();
        // Save the file!
        LevelSaverLoader.SaveLevelFile(ld);
		return ld;
	}


	// ================================================================
	//	Level Files
	// ================================================================
	public void MoveLevelFileToWorldFolder(string levelKey, int worldIndexTo) {
		string destDirectory = FilePaths.WorldFileAddress(worldIndexTo);
		MoveLevelFileToFolder(levelKey, destDirectory);
	}
	public void MoveLevelFileToTrashFolder(string levelKey) {
		string destDirectory = FilePaths.WorldTrashFileAddress(worldIndex);
		MoveLevelFileToFolder(levelKey, destDirectory);
	}
	private void MoveLevelFileToFolder (string levelKey, string destDirectory) {
		string sourceNameFull = FilePaths.WorldFileAddress(worldIndex) + levelKey + ".txt";
        // Directory doesn't exist? Make it!
        if (!Directory.Exists(destDirectory)) {
            Directory.CreateDirectory(destDirectory);
            Debug.LogWarning("Created directory: \"" + destDirectory + "\"");
        }
        string destNameFull = destDirectory + levelKey + ".txt";
		try {
			File.Move(sourceNameFull, destNameFull);
		}
		catch (System.Exception e) { Debug.LogError ("Error moving level file to world folder: " + sourceNameFull + " to " + destNameFull + ". " + e.ToString ()); }
	}



}


/*
    private void SetLevelsIsConnectedToStart() {
        // Tell all of them they're NOT first.
        foreach (LevelData ld in levelDatas.Values) { ld.isConnectedToStart = false; }
        // Get the special ones that are.
        LevelData startLevel = GetLevelData (GameProperties.GetFirstLevelName (worldIndex));
        // Firsht off, if this start level doesn't exist, don't continue doing anything. This whole world is total anarchy.
        if (startLevel != null) {
            List<LevelData> levelsConnectedToStart = LevelUtils.GetLevelsConnectedToLevel (this, startLevel);
            // Tell these levels they're connected to the start dude!
            startLevel.isConnectedToStart = true;
            for (int i=0; i<levelsConnectedToStart.Count; i++) {
                levelsConnectedToStart [i].isConnectedToStart = true;
            }
        }
    }
    */


