using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class WorldData {
	// Components
	public Dictionary<string, LevelData> levelDatas; // ALL level datas in this world! Loaded up when WE'RE loaded up.
	public List<LevelLinkData> levelLinkDatas;
	// Properties
	public bool isWorldUnlocked;
	public int worldIndex; // starts at 0.
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

		LoadAllLevelDatas ();
		LoadLevelLinkDatas ();

		SetAllLevelDatasFundamentalProperties ();
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
	public List<LevelLinkData> LevelLinkDatas { get { return levelLinkDatas; } }
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
//	public LevelLinkData GetLevelLinkDataConnectingLevels(string levelKeyA,string levelKeyB) {
//		for (int i=0; i<levelLinkDatas.Count; i++) {
//			if (levelLinkDatas[i].DoesLinkLevels(levelKeyA,levelKeyB)) {
//				return levelLinkDatas[i];
//			}
//		}
//		return null; // Whoops! There aren't LevelLinkDatas that connect THESE two levels.
//	}
	public List<LevelLinkData> GetLevelLinksConnectingLevel (string levelKey) {
		List<LevelLinkData> returnList = new List<LevelLinkData> ();
		for (int i=0; i<levelLinkDatas.Count; i++) {
			if (levelLinkDatas[i].DoesLinkLevel (levelKey)) {
				returnList.Add (levelLinkDatas[i]);
			}
		}
		return returnList;
	}
	/** Look through every link and see if the provided key is used in ANY link. */
	public bool DoesLevelLinkToAnotherLevel(string levelKey) {
		for (int i=0; i<levelLinkDatas.Count; i++) {
			if (levelLinkDatas[i].DoesLinkLevel(levelKey)) { // It's used in one! Return true.
				return true;
			}
		}
		return false; // It's not used in any. Return false.
	}

	/** E.g. If this level links to other levels ABOVE and BELOW it, this'll return [true, false, true, false]. */
	public bool[] SidesLevelLinksToOtherLevels (string levelKey) {
		// Default all falses.
		bool[] isLinkAtSides = new bool[4];
		for (int i=0; i<isLinkAtSides.Length; i++) isLinkAtSides[i] = false;
		// Look through all levelLinkDatas and check how this dude compares to others!
		for (int i=0; i<levelLinkDatas.Count; i++) {
			LevelLinkData linkData = levelLinkDatas[i]; // For easier readability.
			if (linkData.DoesLinkLevel(levelKey)) { // This link contains this level!
				int relativeSide = GetSideLevelIsOn (levelKey, linkData.OtherKey(levelKey));
				isLinkAtSides[relativeSide] = true;
			}
		}
		return isLinkAtSides;
	}

	/** 0 top, 1 right, 2 bottom, 3 left. E.g. If the second level is to the RIGHT of the first, this'll return 1. */
	private int GetSideLevelIsOn (string levelKeyA, string levelKeyB) {
		Rect levelABounds = GetLevelData (levelKeyA).BoundsGlobal;
		Rect levelBBounds = GetLevelData (levelKeyB).BoundsGlobal;
		return MathUtils.GetSideRectIsOn (levelABounds, levelBBounds);
	}

	public List<LevelData> GetLevelDatasConnectedToLevelData (LevelData sourceLevelData) {
		List<LevelData> returnLevelDatas = new List<LevelData> ();
		// Look through all levelLinkDatas and find the levels this dude connects to!
		for (int i=0; i<levelLinkDatas.Count; i++) {
			LevelLinkData linkData = levelLinkDatas[i]; // For easier readability.
			if (linkData.DoesLinkLevel(sourceLevelData.LevelKey)) { // This link contains this level! Boom, child!
				// Add the OTHER level of this link!
				string otherLevelDataKey = linkData.OtherKey(sourceLevelData.LevelKey);
				LevelData otherLevelData = GetLevelData (otherLevelDataKey);
				if (otherLevelData != null) {
					returnLevelDatas.Add (otherLevelData);
				}
				else {
					Debug.LogError ("Whoa! We're trying to access a nonexistent LevelData: " + otherLevelDataKey);
				}
			}
		}
		return returnLevelDatas;
	}



	private string MakeLevelLinkFileLine(LevelLinkData levelLinkData) {
		string levelAKey = levelLinkData.Key (true);
		string levelBKey = levelLinkData.Key (false);
		string returnString = levelAKey+","+levelBKey;//  +  ","  +  connectingPosA.x+","+connectingPosA.y + "," + connectingPosB.x+","+connectingPosB.y;
		return returnString;
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

	/** FOR NOW, our level-traversal system is dead simple. Nothing's precalculated. We search for the next level the moment we exit one. */
	public LevelData GetLevelAtSide(LevelData originLD, int side) {
		// Find where a point in this next level would be. Then return the LevelData with that point in it!
		Vector2Int dir = MathUtils.GetDir(side);
		Vector2 originSize = originLD.BoundsGlobal.size;
		Vector2 searchOrigin = originLD.BoundsGlobal.position;//.center;
		searchOrigin += new Vector2(dir.x*(originSize.x+1f), dir.y*(originSize.y+1f)); // +1 so the levels don't have to be directly touching; we'll allow even a small gap.
		// Instead of just looking at one point, put out TWO feelers, as levels can be irregularly aligned.
		Vector2[] searchPoints = new Vector2[3];
		if (side==Sides.L || side==Sides.R) {
			searchPoints[0] = searchOrigin + new Vector2(0, originSize.y*0.4f);
			searchPoints[1] = searchOrigin;
			searchPoints[2] = searchOrigin - new Vector2(0, originSize.y*0.4f);
		}
		else {
			searchPoints[0] = searchOrigin + new Vector2(originSize.x*0.4f, 0);
			searchPoints[1] = searchOrigin;
			searchPoints[2] = searchOrigin - new Vector2(originSize.x*0.4f, 0);
		}
		foreach (Vector2 point in searchPoints) {
			LevelData ldHere = GetLevelWithPoint(point);
			if (ldHere != null) { return ldHere; }
		}
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
	//  LevelLinkDatas
	// ================================================================
	private void LoadLevelLinkDatas() {
		// Destroy all LevelLinkDatas if they already exist
		RemoveAllLevelLinkDatas ();
		// Make empty bucket for now
		levelLinkDatas = new List<LevelLinkData>();

//		// Load the file via our Resources.Load function! (So it can work as a build, not just in the editor.)
//		string fileName = GetLevelLinksFileName ();// TODO: This too!
//		if ((Resources.Load (fileName) as TextAsset) == null) {  // Just a lil' check.
//			Debug.LogWarning("Hey! There's no LevelLinks file for this world: " + worldIndex);
//			return;
//		}
//		string textFile = (Resources.Load(fileName) as TextAsset).ToString();

		string filePath = FilePaths.LevelLinksFileAddress(worldIndex);
		if (!File.Exists(filePath)) {
			Debug.LogWarning("Hey! There's no LevelLinks file for this world: " + worldIndex);
			return;
		}

		StreamReader file = File.OpenText(filePath);
		string wholeFile = file.ReadToEnd();
		file.Close();
		string[] levelLinkFile = TextUtils.GetStringArrayFromStringWithLineBreaks(wholeFile, System.StringSplitOptions.None);

		for (int i=0; i<levelLinkFile.Length; i++) {
			string lineString = levelLinkFile[i];
			if (lineString.Length <= 0) { continue; } // Empty line? Skip it!
			string[] data = lineString.Split(',');
			if (data.Length < 6) {
				Debug.LogError ("Invalid LevelLinks line (needs at least 6 params): " + lineString);
				continue;
			}
			string levelKeyA = data[0];
			string levelKeyB = data[1];
//			Vector2 connectingPosA = new Vector2(TextUtils.ParseFloat(data[2]),TextUtils.ParseFloat(data[3]));
//			Vector2 connectingPosB = new Vector2(TextUtils.ParseFloat(data[4]),TextUtils.ParseFloat(data[5]));
//			bool isSecretLink = data.Length > 6 && bool.Parse(data[6]);
			AddLevelLinkData(levelKeyA,levelKeyB, false);//, connectingPosA,connectingPosB, false);
		}
	}
	private int GetNumLevelLinkDatasConnectingLevels(string levelKeyA,string levelKeyB) {
		int total = 0;
		for (int i=0; i<levelLinkDatas.Count; i++) {
			if (levelLinkDatas[i].DoesLinkLevels(levelKeyA,levelKeyB)) {
				total ++;
			}
		}
		return total;
	}

	private void RemoveAllLevelLinkDatas() {
		if (levelLinkDatas == null) { return; }
		for (int i=levelLinkDatas.Count-1; i>=0; --i) {
			RemoveLevelLinkData(levelLinkDatas[i], false);
		}
	}
	/** For ChallengeMode. Any LevelLinkData that connects a level that's NOT one of my LevelDatas will be removed. */
	private void RemoveLevelLinkDatasNotConnectedToKnownLevels () {
		for (int i=levelLinkDatas.Count-1; i>=0; --i) {
			// If either of these levels in the link isn't in my list of levels, remove the link!
			if (GetLevelData (levelLinkDatas[i].LevelAKey)==null || GetLevelData (levelLinkDatas[i].LevelBKey)==null) {
				RemoveLevelLinkData (levelLinkDatas[i], false);
			}
		}
	}

	public LevelLinkData AddLevelLinkData (LevelLinkData levelLinkData, bool doUpdateFile) {
		return AddLevelLinkData (levelLinkData.LevelAKey,levelLinkData.LevelBKey, doUpdateFile);//, levelLinkData.ConnectingPosA,levelLinkData.ConnectingPosB, doUpdateFile);
	}
	public LevelLinkData AddLevelLinkData (string levelKeyA,string levelKeyB, bool doUpdateFile){//, Vector2 connectingPosA,Vector2 connectingPosB, bool doUpdateFile) {
//		int levelLinkID = GetNumLevelLinkDatasConnectingLevels(levelKeyA,levelKeyB); // To make the new LevelLinkData, we need to know how many others already exist that connect these two levels.
		LevelLinkData newLevelLinkData = new LevelLinkData(levelKeyA,levelKeyB);
		levelLinkDatas.Add(newLevelLinkData);
		// Resave file!
		if (doUpdateFile) { ResaveLevelLinksFile(); }
		// Return the created data in case we need it.
		return newLevelLinkData;
	}
	public void RemoveLevelLinkData (LevelLinkData levelLinkDataToRemove, bool doUpdateFile) {
		// Remove from BUCKET
		levelLinkDatas.Remove(levelLinkDataToRemove);
		//		// Tell the LevelLink it's been destroyed (so it can take care of its references)!
		//		levelLinkDataToRemove.Destroy ();
		// Resave file!
		if (doUpdateFile) { ResaveLevelLinksFile(); }
	}


	public void ResaveLevelLinksFile() {
		// Remake the whole file afresh!
		string levelLinksSaveFileName = FilePaths.LevelLinksFileAddress (worldIndex);
		StreamWriter sr = File.CreateText(levelLinksSaveFileName);
		for (int i=0; i<levelLinkDatas.Count; i++) {
			sr.WriteLine(MakeLevelLinkFileLine(levelLinkDatas[i]));
		}
		sr.Close();

		// Reload the text file right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
		#if UNITY_EDITOR
		UnityEditor.AssetDatabase.ImportAsset (levelLinksSaveFileName);
		#endif
	}




	// ================================================================
	//	Level Files
	// ================================================================
	public void MoveLevelFileToWorldFolder (string levelKey, int worldIndexTo) {
		string destFileName = FilePaths.WorldFileAddress (worldIndexTo) + levelKey + ".txt";
		MoveLevelFileToFolder (levelKey, destFileName);
	}
	public void MoveLevelFileToTrashFolder (string levelKey) {
		string destFileName = FilePaths.WorldTrashFileAddress (worldIndex) + levelKey + ".txt";
		MoveLevelFileToFolder (levelKey, destFileName);
	}
	private void MoveLevelFileToFolder (string levelKey, string destFileName) {
		string sourceFileName = FilePaths.WorldFileAddress (worldIndex) + levelKey + ".txt";
		try {
			File.Move (sourceFileName, destFileName);
		}
		catch (System.Exception e) { Debug.LogError ("Error moving level file to world folder: " + sourceFileName + " to " + destFileName + ". " + e.ToString ()); }
	}









}





