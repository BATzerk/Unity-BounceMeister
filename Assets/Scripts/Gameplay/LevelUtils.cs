using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LevelUtils {


	// ================================================================
	//	Level Datas
	// ================================================================
	public static bool IsLevelDataInArray (LevelData[] levelDatas, string levelKey) {
		for (int i=0; i<levelDatas.Length; i++) {
			if (levelDatas[i].LevelKey == levelKey) return true;
		}
		return false;
	}


	// ================================================================
	//	Getters
	// ================================================================
	//public static int GetNumLevelsConnectedToStart (Dictionary<string, LevelData> levelDatas) {
	//	int total = 0;
	//	foreach (LevelData ld in levelDatas.Values) { if (ld.isConnectedToStart) { total ++; } }
	//	return total;
	//}
//	public static int GetNumRegularStarsInLevelsConnectedToStart (Dictionary<string, LevelData> levelDatas) {
//		int total = 0;
//		foreach (LevelData ld in levelDatas.Values) {
//			if (ld.isConnectedToStart) {
//				foreach (StarData starData in ld.starDatas) { if (!starData.isSecretStar && !starData.isNotYours) { total += ld.starDatas.Count; } }
//			}
//		}
//		return total;
//	}
//	public static int GetNumSecretStarsInLevelsConnectedToStart (Dictionary<string, LevelData> levelDatas) {
//		int total = 0;
//		foreach (LevelData ld in levelDatas.Values) {
//			if (ld.isConnectedToStart) {
//				foreach (StarData starData in ld.starDatas) { if (starData.isSecretStar) { total += ld.starDatas.Count; } }
//			}
//		}
//		return total;
//	}
	/** Give me a level we're STARTING at, give me a level to start LOOKING towards, and I'll look towards that level and recursively tally up how many levels in this world branch out from there.
	 If I reach the worldStart or worldEnd levels, I'll count that as +99 levels! (Hacky, because I'm not ACTUALLY counting the levels from previous/next worlds. But I don't think I need to.) */
	public static int GetNumSubsequentLevels (WorldData worldDataRef, string levelSourceKey, string levelToKey, bool doIncludeSecretLevels=false) {
		Dictionary<string, LevelData> allLevelDatas = worldDataRef.LevelDatas;

		LevelData levelSourceData = allLevelDatas [levelSourceKey];
		LevelData levelToData = allLevelDatas [levelToKey];

		// First, don't look back at the level we're starting from.
		levelSourceData.WasUsedInSearchAlgorithm = true;

		// Now, recursively puck up a big list of all the LevelDatas that are connected from the levelTo!
		List<LevelData> levelDatasFromLevelTo = new List<LevelData> ();
		levelDatasFromLevelTo.Add (levelToData); // Add the levelTo to start! The algorithm won't add it itself.
		RecursivelyAddLevelDatasConnectedToLevelData (worldDataRef, ref levelDatasFromLevelTo, levelToData);

		// Reset used-in-algorithm values for all LevelDatas
		foreach (LevelData ld in allLevelDatas.Values) {
			ld.WasUsedInSearchAlgorithm = false;
		}

		// Are any of these levels the START or END of the world?? Then return 99!!
		for (int i=0; i<levelDatasFromLevelTo.Count; i++) {
			if (   (levelDatasFromLevelTo[i].LevelKey == GameProperties.GetFirstLevelName (worldDataRef.WorldIndex))
				|| (levelDatasFromLevelTo[i].LevelKey == GameProperties.GetLastLevelName (worldDataRef.WorldIndex))) {
				return 99;
			}
		}
		// Now just return the length of the list. :)
		return levelDatasFromLevelTo.Count;
	}
	private static void RecursivelyAddLevelDatasConnectedToLevelData (WorldData worldDataRef, ref List<LevelData> levelDatas, LevelData startingLevelData) {
		// If this startingLevelData has ALREADY been used, OR it's a VIRGIN SECRET level, get outta here!
		if (startingLevelData.WasUsedInSearchAlgorithm) {// || (startingLevelData.isSecretLevel && !startingLevelData.hasPlayerBeenHere)
			return;
		}

		// Use me use me!
		startingLevelData.WasUsedInSearchAlgorithm = true;
		// Arright, get ALL the levels that connect to the startingLevelData!
		//List<LevelData> neighborLevelDatas = worldDataRef.GetLevelDatasConnectedToLevelData (startingLevelData);
		//// Add the remaining ones that HAVEN'T yet been used in this search to the list AND do this function again for each of those unused neighboring levels!
		//for (int i=0; i<neighborLevelDatas.Count; i++) {
		//	if (neighborLevelDatas[i].WasUsedInSearchAlgorithm) { continue; }// || (neighborLevelDatas[i].isSecretLevel && !neighborLevelDatas[i].hasPlayerBeenHere)
		//	levelDatas.Add (neighborLevelDatas[i]);
		//	RecursivelyAddLevelDatasConnectedToLevelData (worldDataRef, ref levelDatas, neighborLevelDatas[i]);
		//}
	}

	public static List<LevelData> GetLevelsConnectedToLevel (WorldData worldDataRef, LevelData sourceLevel, bool doIncludeSourceLevel=true) {
		Dictionary<string, LevelData> allLevelDatas = worldDataRef.LevelDatas;

		List<LevelData> levelsConnectedToSourceLevels = new List<LevelData> ();
		if (doIncludeSourceLevel) { levelsConnectedToSourceLevels.Add (sourceLevel); } // We can opt to include/not include the source level for this function.
		// Now, recursively puck up a big list of all the LevelDatas that are connected to the sourceLevel!
		RecursivelyAddLevelDatasConnectedToLevelData (worldDataRef, ref levelsConnectedToSourceLevels, sourceLevel);

		// Reset used-in-algorithm values for all LevelDatas
		foreach (LevelData ld in allLevelDatas.Values) { ld.WasUsedInSearchAlgorithm = false; }

		// Return!
		return levelsConnectedToSourceLevels;
	}

	/*
//		// Now populate a list of only the neighbors that HAVEN'T been used in the search algorithm.
	//		List<LevelData> unusedNeighborLevelDatas = new List<LevelData> ();
	//		for (int i=0; i<allNeighborLevelDatas.Count; i++) {
	//			if (!allNeighborLevelDatas[i].WasUsedInSearchAlgorithm) {
	//				unusedNeighborLevelDatas.Add (allNeighborLevelDatas[i]);
	//			}
	//		}
	//		// Remove all LevelDatas in the list that HAVE been used in this search!
	//		for (int i=neighborLevelDatas.Count-1; i>=0; --i) {
	//			if (neighborLevelDatas[i].WasUsedInSearchAlgorithm) {
	//				neighborLevelDatas.Remove (neighborLevelDatas[i]);
	//			}
	//		}
	*/
	/** This returns a rect that ONLY INCLUDES levels connected to the start level! It discards any other levels in this world. * /
	public static Rect CalculateWorldBoundsPlayableLevels (WorldData worldDataRef) {
		Dictionary<string, LevelData> allLevelDatas = worldDataRef.LevelDatas;

		List<LevelData> levelsComposingViewRect = new List<LevelData> (); // Either all the levels connected to the start, but if we can't find the start, then we'll fall back to ALL the levels in this world.

		// Find the STARTING level, bro!
		LevelData startingLevel;
		string startingLevelKey = GameProperties.GetFirstLevelName (worldDataRef.WorldIndex);
		if (allLevelDatas.ContainsKey (startingLevelKey)) { // This starting level exists :)
			startingLevel = allLevelDatas [startingLevelKey];

			// Now, recursively puck up a big list of all the LevelDatas that are connected to the start!
			levelsComposingViewRect.Add (startingLevel); // Add the levelTo to start! The algorithm won't add it itself.
			RecursivelyAddLevelDatasConnectedToLevelData (worldDataRef, ref levelsComposingViewRect, startingLevel);

			// Reset used-in-algorithm values for all LevelDatas
			foreach (LevelData ld in allLevelDatas.Values) { ld.WasUsedInSearchAlgorithm = false; }
		}
		else { // Uhh, this starting level does not exist. So we'll use ALL this worlds' levels.
			levelsComposingViewRect = new List<LevelData> (allLevelDatas.Values);
		}

		// Now, make the rectangle!
		Rect returnRect = levelsComposingViewRect [0].BoundsGlobal; // Start it with the first level's rect.
		for (int i=0; i<levelsComposingViewRect.Count; i++) {
			returnRect = GameMathUtils.GetCompoundRectangle (returnRect, levelsComposingViewRect[i].BoundsGlobal);
		}
		return returnRect;
	}
	/** Just make a big-ass rectangle that includes ALL levels in the entire world provided. * /
	static public Rect CalculateWorldBoundsAllLevels (WorldData worldDataRef) {
		// Calculate my boundsRectAllLevels so I can know when the camera is lookin' at me!
		Rect returnRect = new Rect (0,0, 0,0);
		foreach (LevelData ld in worldDataRef.levelDatas.Values) {
			Rect ldBounds = ld.BoundsGlobalMinSize;
			if (returnRect.width==0 && returnRect.height==0) { // FIRST level of the bunch? Cool, make worldsBoundRect start EXACTLY like this level's rect. (So we don't include the origin in every worldBoundsRect.)
				returnRect = new Rect(ldBounds);
			}
			else {
				returnRect = GameMathUtils.GetCompoundRectangle (returnRect, ldBounds);
			}
		}
		return returnRect;
	}
	*/

	/*
	public static void SetLevelsNumLevelsFromStart (int WorldIndex, Dictionary<string, Level> levels) {
		// Default all lvls to NOT even being connected to the start!
		foreach (Level l in levels.Values) { l.numLevelsFromStart = -1; }
		// Firsht off, if this start level doesn't exist, don't continue doing anything. This whole world is total anarchy.
		Level startLevel = levels [GameProperties.GetFirstLevelName (WorldIndex)];
		if (startLevel == null) {
			return;
		}

		// Reset used-in-algorithm values for all LevelDatas
		foreach (Level l in levels.Values) {
			l.LevelDataRef.WasUsedInSearchAlgorithm = false;
		}

		// Okay. Now, starting with the startLevel, 


		// Put the toilet seat back down.
		foreach (Level l in levels.Values) { l.LevelDataRef.WasUsedInSearchAlgorithm = false; }
	}
	private static void RecursivelySetNumLevelsFromStart (int numLevelsFromStart, Level sourceLevel) {
		// Don't let us use this level again.
		sourceLevel.LevelDataRef.WasUsedInSearchAlgorithm = true;
		Note: It would be fun to finish writing this, but I don't need to. Not the best use of my time.
	}
	*/


}








