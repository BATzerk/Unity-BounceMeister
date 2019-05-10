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
	private static void RecursivelyAddRoomDatasConnectedToRoomData (WorldData worldData, ref List<RoomData> roomDatas, RoomData startingRoomData) {
		// If this startingRoomData has ALREADY been used, OR it's a VIRGIN SECRET room, get outta here!
		if (startingRoomData.WasUsedInSearchAlgorithm) {// || (startingRoomData.isSecretRoom && !startingRoomData.hasPlayerBeenHere)
			return;
		}

		// Use me use me!
		startingRoomData.WasUsedInSearchAlgorithm = true;
		// Arright, get ALL the rooms that connect to the startingRoomData!
		//List<RoomData> neighborRoomDatas = worldData.GetRoomDatasConnectedToRoomData (startingRoomData);
		//// Add the remaining ones that HAVEN'T yet been used in this search to the list AND do this function again for each of those unused neighboring rooms!
		//for (int i=0; i<neighborRoomDatas.Count; i++) {
		//	if (neighborRoomDatas[i].WasUsedInSearchAlgorithm) { continue; }// || (neighborRoomDatas[i].isSecretRoom && !neighborRoomDatas[i].hasPlayerBeenHere)
		//	roomDatas.Add (neighborRoomDatas[i]);
		//	RecursivelyAddRoomDatasConnectedToRoomData (worldData, ref roomDatas, neighborRoomDatas[i]);
		//}
	}

	public static List<RoomData> GetRoomsConnectedToRoom (WorldData worldData, RoomData sourceRoom, bool doIncludeSourceRoom=true) {
		Dictionary<string, RoomData> allRoomDatas = worldData.RoomDatas;

		List<RoomData> roomsConnectedToSourceRooms = new List<RoomData> ();
		if (doIncludeSourceRoom) { roomsConnectedToSourceRooms.Add (sourceRoom); } // We can opt to include/not include the source room for this function.
		// Now, recursively puck up a big list of all the RoomDatas that are connected to the sourceRoom!
		RecursivelyAddRoomDatasConnectedToRoomData (worldData, ref roomsConnectedToSourceRooms, sourceRoom);

		// Reset used-in-algorithm values for all RoomDatas
		foreach (RoomData rd in allRoomDatas.Values) { rd.WasUsedInSearchAlgorithm = false; }

		// Return!
		return roomsConnectedToSourceRooms;
	}




}








