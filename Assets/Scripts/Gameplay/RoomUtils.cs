using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

static public class RoomUtils {


	// ================================================================
	//	Room Datas
	// ================================================================
	public static bool IsRoomDataInArray (RoomData[] roomDatas, string roomKey) {
		for (int i=0; i<roomDatas.Length; i++) {
			if (roomDatas[i].RoomKey == roomKey) return true;
		}
		return false;
	}

	/** Give me a room we're STARTING at, give me a room to start LOOKING towards, and I'll look towards that room and recursively tally up how many rooms in this world branch out from there.
	 If I reach the worldStart or worldEnd rooms, I'll count that as +99 rooms! (Hacky, because I'm not ACTUALLY counting the rooms from previous/next worlds. But I don't think I need to.) */
	public static int GetNumSubsequentRooms (WorldData worldData, string roomSourceKey, string roomToKey, bool doIncludeSecretRooms=false) {
    return 99;// DISABLED for now.
    /*
		Dictionary<string, RoomData> allRoomDatas = worldData.RoomDatas;

		RoomData roomSourceData = allRoomDatas [roomSourceKey];
		RoomData roomToData = allRoomDatas [roomToKey];

		// First, don't look back at the room we're starting from.
		roomSourceData.WasUsedInSearchAlgorithm = true;

		// Now, recursively puck up a big list of all the RoomDatas that are connected from the roomTo!
		List<RoomData> roomDatasFromRoomTo = new List<RoomData> ();
		roomDatasFromRoomTo.Add (roomToData); // Add the roomTo to start! The algorithm won't add it itself.
		RecursivelyAddRoomDatasConnectedToRoomData (worldData, ref roomDatasFromRoomTo, roomToData);

		// Reset used-in-algorithm values for all RoomDatas
		foreach (RoomData rd in allRoomDatas.Values) {
			rd.WasUsedInSearchAlgorithm = false;
		}

		// Are any of these rooms the START or END of the world?? Then return 99!!
		for (int i=0; i<roomDatasFromRoomTo.Count; i++) {
			if (   (roomDatasFromRoomTo[i].RoomKey == GameProperties.GetFirstRoomName (worldData.WorldIndex))
				|| (roomDatasFromRoomTo[i].RoomKey == GameProperties.GetLastRoomName (worldData.WorldIndex))) {
				return 99;
			}
		}
		// Now just return the length of the list. :)
		return roomDatasFromRoomTo.Count;
        */
	}
    public static List<RoomData> GetRoomsConnectedToRoom(RoomData sourceRD) {
        WorldData wd = sourceRD.MyWorldData;
        wd.ResetRoomsWasUsedInSearch();
        
        List<RoomData> list = new List<RoomData>();
        RecursivelyAddRoomToList(sourceRD, ref list);
        
        wd.ResetRoomsWasUsedInSearch();
        return list;
    }
    private static void RecursivelyAddRoomToList(RoomData rd, ref List<RoomData> list) {
        if (rd.WasUsedInSearchAlgorithm) { return; } // This RoomData was used? Ignore it.
        rd.WasUsedInSearchAlgorithm = true;
        list.Add(rd);
        // Now try for all its neighbors!
        for (int i=0; i<rd.Openings.Count; i++) {
            if (rd.Openings[i].IsRoomTo) {
                RecursivelyAddRoomToList(rd.Openings[i].RoomTo, ref list);
            }
        }
    }




}








