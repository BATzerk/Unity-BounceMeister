using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager {
	// Properties
	private int coinsCollected; // the total value of all the coins we've collected!
    private List<WorldData> worldDatas;
//  public int mostRecentlySavedRoom_worldIndex; // an nbd shortcut to highlight the most recently created room in the MapEditor.
//  public string mostRecentlySavedRoom_roomKey; // an nbd shortcut to highlight the most recently created room in the MapEditor.
    public RoomData currRoomData = null; // TODO: Remove this. We don't need it (right?). if this is defined when GameController opens, we'll open THAT room!
    // Entering-Room Properties
    public string roomToDoorID = null; // defined when use a RoomDoor. When we enter a room, this is the door we'll start at!
    //public int playerSideEnterNextRoom=-1; // pairs with playerPosGlobalOnExitRoom.
    //public Vector2 playerPosGlobalOnExitRoom=Vector2Extensions.NaN; // The suuuuper simple way we know how to set the Player's pos on entering the next room.
    public Vector2 playerGroundedRespawnPos=Vector2Extensions.NaN; // I'll respawn at this pos. Set when we leave a Ground that has IsPlayerRespawn.

	// ----------------------------------------------------------------
	//  Getters
	// ----------------------------------------------------------------
    public WorldData CurrWorldData { get { return currRoomData==null ? null : currRoomData.WorldDataRef; } }
	//public int currentWorldIndex { get { return currRoomData==null ? 0 : currRoomData.WorldIndex; } }
	public int CoinsCollected { get { return coinsCollected; } }
	public int NumWorldDatas { get { return worldDatas.Count; } }
	public RoomData GetRoomData(int worldIndex, string roomKey, bool doMakeOneIfItDoesntExist) {
		return GetWorldData(worldIndex).GetRoomData(roomKey, doMakeOneIfItDoesntExist);
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
    public bool IsPlayerTypeUnlocked(PlayerTypes playerType) {
        return SaveStorage.GetBool(SaveKeys.IsPlayerTypeUnlocked(playerType));
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
		Reset ();
//		Developer_CountTotalStarsToManuallyUpdateGamePropertiesValue ();
	}

	private void Reset () {
		coinsCollected = 0;
//		debug_doShowRoomTileDesignerFlags = SaveStorage.GetInt (SaveKeys.DEBUG_DO_SHOW_LEVEL_TILE_DESIGNER_FLAGS, 0) == 1;
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

    public void ClearRoomSaveData(Room room) { ClearRoomSaveData(room.RoomDataRef); }
    public void ClearRoomSaveData(RoomData rd) {
        SaveStorage.DeleteKey(SaveKeys.HasPlayerBeenInRoom(rd));
        for (int i=0; i<9; i++) { // Sloppy and inefficient!! But NBD for our purposes.
            SaveStorage.DeleteKey(SaveKeys.DidEatGem(rd, i));
            SaveStorage.DeleteKey(SaveKeys.DidEatSnack(rd, i));
            SaveStorage.DeleteKey(SaveKeys.IsGateUnlocked(rd, i));
            SaveStorage.DeleteKey(SaveKeys.CharBarrelTypeInMe(rd, i));
        }
    }
    
    /// Resets static values that determine where Player will start when reloading a Room (e.g. RoomDoorID, prev-room-exit-pos, grounded-respawn-pos).
    public void ResetRoomEnterValues() {
        roomToDoorID = null;
        playerGroundedRespawnPos = Vector2Extensions.NaN;
        //playerSideEnterNextRoom = -1;
        //playerPosGlobalOnExitRoom = Vector2Extensions.NaN;
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









