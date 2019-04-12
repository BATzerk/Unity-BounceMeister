﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager {
	// Properties
	private int coinsCollected; // the total value of all the coins we've collected!
    private List<WorldData> worldDatas;
//  public int mostRecentlySavedLevel_worldIndex; // an nbd shortcut to highlight the most recently created level in the MapEditor.
//  public string mostRecentlySavedLevel_levelKey; // an nbd shortcut to highlight the most recently created level in the MapEditor.
    public LevelData currLevelData = null; // TODO: Remove this. We don't need it (right?). if this is defined when GameController opens, we'll open THAT level!
    // Entering-Level Properties
    public string levelToDoorID = null; // defined when use a LevelDoor. When we enter a level, this is the door we'll start at!
    //public int playerSideEnterNextLevel=-1; // pairs with playerPosGlobalOnExitLevel.
    //public Vector2 playerPosGlobalOnExitLevel=Vector2Extensions.NaN; // The suuuuper simple way we know how to set the Player's pos on entering the next level.
    public Vector2 playerGroundedRespawnPos=Vector2Extensions.NaN; // I'll respawn at this pos. Set when we leave a Ground that has IsPlayerRespawn.

	// ----------------------------------------------------------------
	//  Getters
	// ----------------------------------------------------------------
    public WorldData CurrWorldData { get { return currLevelData==null ? null : currLevelData.WorldDataRef; } }
	//public int currentWorldIndex { get { return currLevelData==null ? 0 : currLevelData.WorldIndex; } }
	public int CoinsCollected { get { return coinsCollected; } }
	public int NumWorldDatas { get { return worldDatas.Count; } }
	public LevelData GetLevelData(int worldIndex, string levelKey, bool doMakeOneIfItDoesntExist) {
		return GetWorldData(worldIndex).GetLevelData(levelKey, doMakeOneIfItDoesntExist);
	}
	public WorldData GetWorldData (int worldIndex) {
		return worldDatas[worldIndex];
    }
    public int NumSnacksCollected(int worldIndex) {
        return GetWorldData(worldIndex).NumSnacksCollected;
    }
    public int NumSnacksTotal(int worldIndex) {
        return GetWorldData(worldIndex).NumSnacksTotal;
    }


    // ----------------------------------------------------------------
    //  Setters
    // ----------------------------------------------------------------
    public void ChangeCoinsCollected(int value) {
		SetCoinsCollected (coinsCollected + value);
	}
	public void SetCoinsCollected(int value) {
		coinsCollected = value;
		// Dispatch event!
		GameManagers.Instance.EventManager.OnCoinsCollectedChanged();
	}
//	public void SetWorldIndexOnLoadGameScene (int WorldIndex) {
//		worldIndexOnLoadGameScene = WorldIndex;
//		SaveStorage.SetInt (SaveKeys.LastWorldPlayedIndex, worldIndexOnLoadGameScene);
//	}



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public DataManager() {
		Reset ();
//		Developer_CountTotalStarsToManuallyUpdateGamePropertiesValue ();
	}

	private void Reset () {
		coinsCollected = 0;
//		debug_doShowLevelTileDesignerFlags = SaveStorage.GetInt (SaveKeys.DEBUG_DO_SHOW_LEVEL_TILE_DESIGNER_FLAGS, 0) == 1;
//		highestWorldEndEverReached = SaveStorage.GetInt (SaveKeys.HIGHEST_WORLD_END_EVER_REACHED);

		ReloadWorldDatas();
	}

	public void ReloadWorldDatas () {
		worldDatas = new List<WorldData> ();
		for (int i=0; i<GameProperties.NUM_WORLDS; i++) {
			WorldData newWorldData = new WorldData(i);
			newWorldData.Initialize ();
			worldDatas.Add (newWorldData);
		}
	}



    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void ClearAllSaveData() {
        // NOOK IT
        SaveStorage.DeleteAll ();
        Reset ();
        Debug.Log ("All SaveStorage CLEARED!");
    }
    /// Resets static values that determine where Player will start when reloading a Level (e.g. LevelDoorID, prev-level-exit-pos, grounded-respawn-pos).
    public void ResetLevelEnterValues() {
        levelToDoorID = null;
        playerGroundedRespawnPos = Vector2Extensions.NaN;
        //playerSideEnterNextLevel = -1;
        //playerPosGlobalOnExitLevel = Vector2Extensions.NaN;
    }


//	public void EraseSaveSlot (int slotIndex) {
//		// SIGH. There's no cleaner way (aka not hardcode-erasing every value) to do this WITHOUT switching to a non-SaveStorage system.
//		int originalCurrentSlotIndex = currentSaveSlotIndex;
//		//		SetCurrentSaveSlotIndex (slotIndex);
//		currentSaveSlotIndex = slotIndex; // Set this directly, sans any overhead: We only need currentSaveSlotIndex to be this value for a moment.
//		SaveStorage.DeleteKey (SaveKeys.GetLastWorldPlayedIndex (slotIndex));
//		SaveStorage.DeleteKey (SaveKeys.TotalStarsCollectedRegular (slotIndex));
//		SaveStorage.DeleteKey (SaveKeys.TotalStarsCollectedSecret (slotIndex));
//		SaveStorage.DeleteKey (SaveKeys.TotalStarsCollectedNotYours (slotIndex));
//		// It's UGLY, but use all my known WorldDatas to take care of this.
//		foreach (WorldData wd in worldDatas) {
//			int WorldIndex = wd.WorldIndex;
//			SaveStorage.DeleteKey (SaveKeys.DidPlayerReachWorldEnd (slotIndex, WorldIndex));
//			SaveStorage.DeleteKey (SaveKeys.HasUnveiledWorld (slotIndex, WorldIndex));
//			SaveStorage.DeleteKey (SaveKeys.IsWorldUnlocked (slotIndex, WorldIndex));
//			SaveStorage.DeleteKey (SaveKeys.SnapshotGameplayData (WorldIndex));
//			SaveStorage.DeleteKey (SaveKeys.SnapshotPlayerData (WorldIndex));
//			foreach (LevelData ld in wd.LevelDatas.Values) {
//				string levelKey = ld.LevelKey;
//				SaveStorage.DeleteKey (SaveKeys.SerializedLevelDataSnapshot (WorldIndex, levelKey));
//				for (int s=0; s<ld.starDatas.Count; s++) {
//					SaveStorage.DeleteKey (SaveKeys.IsStarCollected (slotIndex, WorldIndex, levelKey, s));
//				}
//			}
//		}
//		// Put back which slot we've got active.
//		currentSaveSlotIndex = -1; // Set this to -1 so we *definitely* reload the current slot's values.
//		SetCurrentSaveSlotIndex (originalCurrentSlotIndex);
//	}


//	/** It's overkill to calculate this value whenever we load up the game. The number of stars WON'T change... but if/when it does, just call this function and update the hardcoded values in GameProperties. */
//	private void Developer_CountTotalStarsToManuallyUpdateGamePropertiesValue () {
//		// Recalculate how many stars there are total!
//		int numTotalStarsRegular = 0;
//		int numTotalStarsSecret = 0;
//		foreach (WorldData wd in worldDatas) {
//			if (!wd.IsPlayableWorld) { continue; }
//			foreach (LevelData ld in wd.levelDatas.Values) {
//				if (!ld.isConnectedToStart) { continue; }
//				foreach (StarData sd in ld.starDatas) {
//					if (sd.isNotYours) { continue; }
//					if (sd.isSecretStar) { numTotalStarsSecret ++; }
//					else { numTotalStarsRegular ++; }
//				}
//			}
//		}
//		// Don't forget about the GiantStar!
//		numTotalStarsRegular ++;
//		Debug.Log ("TOTAL STARS REGULAR: " + numTotalStarsRegular);
//		Debug.Log ("TOTAL STARS SECRET: " + numTotalStarsSecret);
//	}

	//	public void OnPlayerReachWorldEnd (int _ssi, int _worldIndex) {
	//		SaveStorage.SetInt (SaveKeys.DidPlayerReachWorldEnd (_ssi, _worldIndex), 1);
	//		highestWorldEndEverReached = Mathf.Max (highestWorldEndEverReached, _worldIndex);
	//		SaveStorage.SetInt (SaveKeys.HIGHEST_WORLD_END_EVER_REACHED, highestWorldEndEverReached);
	//	}

}









