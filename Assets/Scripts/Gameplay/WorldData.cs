using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class WorldData {
	// Components
	public Dictionary<string, LevelData> levelDatas; // ALL level datas in this world! Loaded up when WE'RE loaded up.
	// Properties
	public bool isWorldUnlocked;
	public int worldIndex; // starts at 0.
    public int NumSnacksCollected { get; private set; }
    public int NumSnacksTotal { get; private set; }
    private Rect boundsRectAllLevels; // For the release version of Linelight, it doesn't make sense to have TWO rects. But in development, a lot of worlds' levels aren't used. So we have to make the distinction.
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
		isWorldUnlocked = true;//SaveStorage.GetInt (SaveKeys.IsWorldUnlocked (worldIndex)) == 1;

		LoadAllLevelDatas();

		SetAllLevelDatasFundamentalProperties();
//		Debug.Log ("World " + worldIndex + "  lvls: " + LevelUtils.GetNumLevelsConnectedToStart (levelDatas) + "   stars: " + LevelUtils.GetNumRegularStarsInLevelsConnectedToStart (levelDatas) + "   (" + LevelUtils.GetNumSecretStarsInLevelsConnectedToStart (levelDatas) + " secret stars)");
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
	public void SetAllLevelDatasFundamentalProperties () {
		SetLevelsIsConnectedToStart ();
		UpdateWorldBoundsRects ();
//		SetAllLevelDatasPosWorld (); // now that I know MY center position, let's tell all my LevelDatas their posWorld (based on their global position)!
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
		// Only add levels CONNECTED TO START for the playableLevels list.
		if (ld.isConnectedToStart) {
			boundsRectPlayableLevels = MathUtils.GetCompoundRectangle (boundsRectPlayableLevels, ldBounds);
		}
	}
//	/** Once we know where the center of the playable world is, set the posWorld value for all my levels! */
//	private void SetAllLevelDatasPosWorld () {
//		foreach (LevelData ld in levelDatas.Values) {
//			ld.SetPosWorld (new Vector2 (ld.posGlobal.x-CenterPos.x, ld.posGlobal.y-CenterPos.y));
//		}
//	}

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
			return levelDatas [key];
		}
		if (doMakeOneIfItDoesntExist) {
			return AddLevelData(key);
		}
		else {
			return null;
		}
	}
	/** Look through every link and see if the provided key is used in ANY link. */
	public bool DoesLevelLinkToAnotherLevel(string levelKey) {
		//for (int i=0; i<levelLinkDatas.Count; i++) {
		//	if (levelLinkDatas[i].DoesLinkLevel(levelKey)) { // It's used in one! Return true.
		//		return true;
		//	}
		//}
		return false; // It's not used in any. Return false.
	}

	/** E.g. If this level links to other levels ABOVE and BELOW it, this'll return [true, false, true, false]. */
	public bool[] SidesLevelLinksToOtherLevels (string levelKey) {
		// Default all falses.
		bool[] isLinkAtSides = new bool[4];
		for (int i=0; i<isLinkAtSides.Length; i++) isLinkAtSides[i] = false;
		//// Look through all levelLinkDatas and check how this dude compares to others!
		//for (int i=0; i<levelLinkDatas.Count; i++) {
		//	LevelLinkData linkData = levelLinkDatas[i]; // For easier readability.
		//	if (linkData.DoesLinkLevel(levelKey)) { // This link contains this level!
		//		int relativeSide = GetSideLevelIsOn (levelKey, linkData.OtherKey(levelKey));
		//		isLinkAtSides[relativeSide] = true;
		//	}
		//}
		return isLinkAtSides;
	}

	/** 0 top, 1 right, 2 bottom, 3 left. E.g. If the second level is to the RIGHT of the first, this'll return 1. */
	private int GetSideLevelIsOn (string levelKeyA, string levelKeyB) {
		Rect levelABounds = GetLevelData (levelKeyA).BoundsGlobal;
		Rect levelBBounds = GetLevelData (levelKeyB).BoundsGlobal;
		return MathUtils.GetSideRectIsOn (levelABounds, levelBBounds);
	}

	public Vector2 GetBrandNewLevelPos () {
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
	public LevelData GetLevelAtSide(LevelData originLD, Vector2 playerPosLocal, int side) {
        // Put playerPosLocal on the EDGE of the origin level-- we only wanna use its x/y value parallel to the next level.
        playerPosLocal = LockPosOnLevelEdge(originLD, playerPosLocal, side);
		// Find where a point in this next level would be. Then return the LevelData with that point in it!
        Vector2Int dir = MathUtils.GetDir(side);
		Vector2 searchPoint = originLD.posGlobal + playerPosLocal + dir*2f; // look ahead a few feet into the next level area.
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
    public void UpdateNumSnacksCollected() {
        NumSnacksCollected = 0;
        foreach (LevelData ld in levelDatas.Values) {
            int snackIndex=0; // incremented every time we find a Snack in this Level's Prop list.
            foreach (PropData pd in ld.allPropDatas) {
                if (pd is SnackData) {
                    if (SaveStorage.GetBool(SaveKeys.DidEatSnack(ld,snackIndex))) { NumSnacksCollected++; }
                    snackIndex++;
                }
            }
        }
    }
    public void UpdateNumSnacksTotal() {
        NumSnacksTotal = 0;
        foreach (LevelData ld in levelDatas.Values) {
            foreach (PropData pd in ld.allPropDatas) {
                if (pd is SnackData) { NumSnacksTotal++; }
            }
        }
    }

    // ================================================================
    //  Events
    // ================================================================
    public void OnPlayerEatSnack() {
        // Update counts!
        UpdateNumSnacksCollected();
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
				AddLevelData (levelKey);
			}
		}
		else {
			Debug.LogError("World folder not found! " + worldIndex);
        }

        UpdateNumSnacksCollected();
        UpdateNumSnacksTotal();

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
        //			AddLevelData (levelKey);
        //		}
    }
	private LevelData AddLevelData (string levelKey) {
		LevelData newLevelData = new LevelData (worldIndex, levelKey);
		LevelSaverLoader.LoadLevelDataFromItsFile (newLevelData);
		levelDatas.Add (levelKey, newLevelData);
		return newLevelData;
	}
	public void ReloadLevelData(string levelKey) {
		// FIRSTLY, check if this brah exists. If now, make 'im first!
		if (!levelDatas.ContainsKey (levelKey)) {
			AddLevelData (levelKey);
		}
		// If it DOES already exist, then reload this guy!
		else {
			LevelData levelData = GetLevelData(levelKey);
			// Reload all of this levelData's contents! (WITHOUT remaking it or anything-- it's important to retain all references to it!)
			LevelSaverLoader.LoadLevelDataFromItsFile(levelData);
		}
		// Update our world bounds! NOTE!! I don't know why we call this. If we call this, it would mean we also probably must call it along with everything else in SetAllLevelDatasFundamentalProperties.
		UpdateWorldBoundsRects ();
	}


	public LevelData CreateLevelDataIfKeyDoesntExist(string levelKey) {
		LevelData levelData = GetLevelData (levelKey);
		// If there ISN'T a level with this key...!!
		if (levelData == null) {
			// MAKE one!!
			levelData = AddLevelData (levelKey);
			// Set its pos to somewhere convenient!
			levelData.SetPosGlobal (GetBrandNewLevelPos (), false);
			// Update our boundsRects to include the new guy!
			AddLevelBoundsToWorldBoundsRects (levelData);
//			// Update who the most recently saved level is! Note: HACKY. We didn't actually *save* anything yet.
//			GameManagers.Instance.DataManager.mostRecentlySavedLevel_worldIndex = worldIndex;
//			GameManagers.Instance.DataManager.mostRecentlySavedLevel_levelKey = levelKey;
		}
		return levelData;
	}

	private void SetLevelsIsConnectedToStart () {
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





