﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager {
	// Properties
    public CharLineup CharLineup { get; private set; }
    private List<WorldData> worldDatas;
    //  public int mostRecentlySavedRoom_worldIndex; // an nbd shortcut to highlight the most recently created room in the MapEditor.
    //  public string mostRecentlySavedRoom_roomKey; // an nbd shortcut to highlight the most recently created room in the MapEditor.
    public RoomData currRoomData = null; // if this is defined when GameController opens, we'll open THAT room!
    // User Progress Properties
    public int CoinsCollected { get; private set; } // NOTE: Not fully implemented.
    public SnackCount SnackCountGame = new SnackCount(); // ALL snack counts totaled together!
    // Entering-Room Properties
    public string doorToID = null; // defined when use a RoomDoor. When we enter a room, this is the door we'll start at!
    public PlayerData playerGroundedRespawnData=null; // I'll respawn at this pos. Set when we leave a Ground that has IsPlayerRespawn.
    
	// ----------------------------------------------------------------
	//  Getters
	// ----------------------------------------------------------------
    public WorldData CurrWorldData { get { return currRoomData==null ? null : currRoomData.MyWorldData; } }
    public int NumWorldDatas { get { return worldDatas.Count; } }
    public RoomData GetRoomData(RoomAddress addr, bool doMakeOneIfItDoesntExist) {
        return GetRoomData(addr.world, addr.room, doMakeOneIfItDoesntExist);
    }
    public RoomData GetRoomData(int worldIndex, string roomKey, bool doMakeOneIfItDoesntExist) {
        return GetWorldData(worldIndex).GetRoomData(roomKey, doMakeOneIfItDoesntExist);
    }
	public WorldData GetWorldData (int worldIndex) {
		return worldDatas[worldIndex];
    }
    //public int NumSnacksCollected(int worldIndex) {
    //    return GetWorldData(worldIndex).NumSnacksCollected;
    //}
    //public int NumSnacksTotal(int worldIndex) {
    //    return GetWorldData(worldIndex).NumSnacksTotal;
    //}
    public bool IsPlayerTypeUnlocked(PlayerTypes playerType) {
        return SaveStorage.GetBool(SaveKeys.IsPlayerTypeUnlocked(playerType));
    }
    public RoomAddress LastPlayedRoomAddress() {
        int worldIndex = SaveStorage.GetInt(SaveKeys.LastPlayedWorldIndex, GameProperties.FirstWorld);
        return LastPlayedRoomAddress(worldIndex);
    }
    public RoomAddress LastPlayedRoomAddress(int worldIndex) {
        string roomKey = SaveStorage.GetString(SaveKeys.LastPlayedRoomKey(worldIndex));//, GameProperties.FirstClust(worldIndex));
        return new RoomAddress(worldIndex, -1, roomKey);
    }
    public int LastPlayedClustIndex(int worldIndex) {
        RoomAddress address = LastPlayedRoomAddress(worldIndex);
        RoomData rd = GetRoomData(address, false);
        if (rd==null || rd.MyCluster==null) { return 0; }
        return rd.MyCluster.ClustIndex;
    }
    public RoomData LastPlayedRoomData(int worldIndex) {
        RoomAddress address = LastPlayedRoomAddress(worldIndex);
        return GetRoomData(address, false);
    }
    public RoomData GetPlayerTrialStartRoom(PlayerTypes pt) {
        string roomKey = pt.ToString() + "TrialStart"; // TEMP HARDCODED finding.
        return GetRoomData(GameProperties.TEMP_TrialsWorldIndex, roomKey, false);
    }


    // ----------------------------------------------------------------
    //  Setters
    // ----------------------------------------------------------------
    public void ChangeCoinsCollected(int value) {
		SetCoinsCollected (CoinsCollected + value);
	}
	public void SetCoinsCollected(int value) {
		CoinsCollected = value;
        SaveStorage.SetInt(SaveKeys.NumCoinsCollected, CoinsCollected);
		// Dispatch event!
		GameManagers.Instance.EventManager.OnCoinsCollectedChanged();
	}
    public void UnlockPlayerType(PlayerTypes playerType) {
        SaveStorage.SetBool(SaveKeys.IsPlayerTypeUnlocked(playerType), true);
    }
//	public void SetWorldIndexOnLoadGameScene (int WorldIndex) {
//		worldIndexOnLoadGameScene = WorldIndex;
//		SaveStorage.SetInt (SaveKeys.LastWorldPlayedIndex, worldIndexOnLoadGameScene);
//	}



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public DataManager() {
		Reset();
//		Developer_CountTotalStarsToManuallyUpdateGamePropertiesValue();
	}

	private void Reset () {
		CoinsCollected = SaveStorage.GetInt(SaveKeys.NumCoinsCollected);
//		highestWorldEndEverReached = SaveStorage.GetInt (SaveKeys.HIGHEST_WORLD_END_EVER_REACHED);
        
        ReloadCharLineup();
		ReloadWorldDatas();
        RefreshSnackCountGame();
	}
    
    private void ReloadCharLineup() {
        CharLineup = new CharLineup();
        CharLineup.LoadFromSaveData();
    }
	public void ReloadWorldDatas () {
		worldDatas = new List<WorldData> ();
		for (int i=0; i<GameProperties.NUM_WORLDS; i++) {
			WorldData newWorldData = new WorldData(i);
			newWorldData.Initialize();
			worldDatas.Add(newWorldData);
		}
	}

    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void RefreshSnackCountGame() {
        // Zero my count.
        SnackCountGame.ZeroCounts();
        // Add SnackCounts from every cluster in every world!
        for (int w=0; w<worldDatas.Count; w++) {
            for (int c=0; c<worldDatas[w].clusters.Count; c++) {
                SnackCountGame.Add(worldDatas[w].clusters[c].SnackCount);
            }
        }
        if (!GameManagers.IsInitializing) {
            GameManagers.Instance.EventManager.OnSnackCountGameChanged();
        }
    }
    //public void RefreshTotalEdiblesEaten() {
    //    NumSnacksEaten = 0;
    //    for (int w=0; w<worldDatas.Count; w++) {
    //        for (int c=0; c<worldDatas[w].clusters.Count; c++) {
    //            NumSnacksEaten += worldDatas[w].clusters[c].NumSnacksEaten;
    //        }
    //    }
    //    if (!GameManagers.IsInitializing) {
    //        GameManagers.Instance.EventManager.OnNumSnacksEatenChanged();
    //    }
    //}
    //public void IncrementNumSnacksEaten() {
    //    NumSnacksEaten ++;
    //    GameManagers.Instance.EventManager.OnNumSnacksEatenChanged();
    //}


    // ----------------------------------------------------------------
    //  Deleting / Resetting
    // ----------------------------------------------------------------
    public void ClearAllSaveData() {
        // NOOK IT
        SaveStorage.DeleteAll ();
        Reset ();
        Debug.Log ("All SaveStorage CLEARED!");
    }

    public void ClearClusterSaveData(RoomClusterData clustData) {
        if (clustData == null) { Debug.LogWarning("Can't clear NULL Cluster save data!"); return; } // Safety check.
        foreach (RoomData rd in clustData.rooms) { ClearRoomSaveData(rd); }
        Debug.Log("Cleared CLUSTER save data!");
    }
    public void ClearRoomSaveData(Room room) { ClearRoomSaveData(room.MyRoomData); }
    public void ClearRoomSaveData(RoomData rd) {
        // Delete saved values!
        SaveStorage.DeleteKey(SaveKeys.HasPlayerBeenInRoom(rd));
        for (int i=0; i<9; i++) { // Sloppy and inefficient!! But NBD for our purposes.
            SaveStorage.DeleteKey(SaveKeys.DidEatGem(rd, i));
            SaveStorage.DeleteKey(SaveKeys.DidEatSnack(rd, i));
            SaveStorage.DeleteKey(SaveKeys.IsGateUnlocked(rd, i));
            SaveStorage.DeleteKey(SaveKeys.IsProgressGateOpen(rd, i));
            SaveStorage.DeleteKey(SaveKeys.IsVeilUnveiled(rd, i));
            SaveStorage.DeleteKey(SaveKeys.CharBarrelTypeInMe(rd, i));
        }
        // Recalculate SnackCountGame!
        if (rd.IsInCluster) { rd.MyCluster.RefreshSnackCount(); }
        RefreshSnackCountGame();
    }
    
    /// Resets static values that determine where Player will start when reloading a Room (e.g. RoomDoorID, prev-room-exit-pos, grounded-respawn-pos).
    public void ResetRoomEnterValues() {
        doorToID = null;
        playerGroundedRespawnData = null;
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
//			foreach (RoomData rd in wd.RoomDatas.Values) {
//				string roomKey = rd.RoomKey;
//				SaveStorage.DeleteKey (SaveKeys.SerializedRoomDataSnapshot (WorldIndex, roomKey));
//				for (int s=0; s<rd.starDatas.Count; s++) {
//					SaveStorage.DeleteKey (SaveKeys.IsStarCollected (slotIndex, WorldIndex, roomKey, s));
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
//			foreach (RoomData rd in wd.roomDatas.Values) {
//				if (!rd.isConnectedToStart) { continue; }
//				foreach (StarData sd in rd.starDatas) {
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









