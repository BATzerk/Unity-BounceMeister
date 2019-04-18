using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RoomUtils {


	// ================================================================
	//	Room Datas
	// ================================================================
	public static bool IsRoomDataInArray (RoomData[] roomDatas, string roomKey) {
		for (int i=0; i<roomDatas.Length; i++) {
			if (roomDatas[i].RoomKey == roomKey) return true;
		}
		return false;
	}


	// ================================================================
	//	Getters
	// ================================================================
	//public static int GetNumRoomsConnectedToStart (Dictionary<string, RoomData> roomDatas) {
	//	int total = 0;
	//	foreach (RoomData rd in roomDatas.Values) { if (rd.isConnectedToStart) { total ++; } }
	//	return total;
	//}
//	public static int GetNumRegularStarsInRoomsConnectedToStart (Dictionary<string, RoomData> roomDatas) {
//		int total = 0;
//		foreach (RoomData rd in roomDatas.Values) {
//			if (rd.isConnectedToStart) {
//				foreach (StarData starData in rd.starDatas) { if (!starData.isSecretStar && !starData.isNotYours) { total += rd.starDatas.Count; } }
//			}
//		}
//		return total;
//	}
//	public static int GetNumSecretStarsInRoomsConnectedToStart (Dictionary<string, RoomData> roomDatas) {
//		int total = 0;
//		foreach (RoomData rd in roomDatas.Values) {
//			if (rd.isConnectedToStart) {
//				foreach (StarData starData in rd.starDatas) { if (starData.isSecretStar) { total += rd.starDatas.Count; } }
//			}
//		}
//		return total;
//	}
	/** Give me a room we're STARTING at, give me a room to start LOOKING towards, and I'll look towards that room and recursively tally up how many rooms in this world branch out from there.
	 If I reach the worldStart or worldEnd rooms, I'll count that as +99 rooms! (Hacky, because I'm not ACTUALLY counting the rooms from previous/next worlds. But I don't think I need to.) */
	public static int GetNumSubsequentRooms (WorldData worldDataRef, string roomSourceKey, string roomToKey, bool doIncludeSecretRooms=false) {
		Dictionary<string, RoomData> allRoomDatas = worldDataRef.RoomDatas;

		RoomData roomSourceData = allRoomDatas [roomSourceKey];
		RoomData roomToData = allRoomDatas [roomToKey];

		// First, don't look back at the room we're starting from.
		roomSourceData.WasUsedInSearchAlgorithm = true;

		// Now, recursively puck up a big list of all the RoomDatas that are connected from the roomTo!
		List<RoomData> roomDatasFromRoomTo = new List<RoomData> ();
		roomDatasFromRoomTo.Add (roomToData); // Add the roomTo to start! The algorithm won't add it itself.
		RecursivelyAddRoomDatasConnectedToRoomData (worldDataRef, ref roomDatasFromRoomTo, roomToData);

		// Reset used-in-algorithm values for all RoomDatas
		foreach (RoomData rd in allRoomDatas.Values) {
			rd.WasUsedInSearchAlgorithm = false;
		}

		// Are any of these rooms the START or END of the world?? Then return 99!!
		for (int i=0; i<roomDatasFromRoomTo.Count; i++) {
			if (   (roomDatasFromRoomTo[i].RoomKey == GameProperties.GetFirstRoomName (worldDataRef.WorldIndex))
				|| (roomDatasFromRoomTo[i].RoomKey == GameProperties.GetLastRoomName (worldDataRef.WorldIndex))) {
				return 99;
			}
		}
		// Now just return the length of the list. :)
		return roomDatasFromRoomTo.Count;
	}
	private static void RecursivelyAddRoomDatasConnectedToRoomData (WorldData worldDataRef, ref List<RoomData> roomDatas, RoomData startingRoomData) {
		// If this startingRoomData has ALREADY been used, OR it's a VIRGIN SECRET room, get outta here!
		if (startingRoomData.WasUsedInSearchAlgorithm) {// || (startingRoomData.isSecretRoom && !startingRoomData.hasPlayerBeenHere)
			return;
		}

		// Use me use me!
		startingRoomData.WasUsedInSearchAlgorithm = true;
		// Arright, get ALL the rooms that connect to the startingRoomData!
		//List<RoomData> neighborRoomDatas = worldDataRef.GetRoomDatasConnectedToRoomData (startingRoomData);
		//// Add the remaining ones that HAVEN'T yet been used in this search to the list AND do this function again for each of those unused neighboring rooms!
		//for (int i=0; i<neighborRoomDatas.Count; i++) {
		//	if (neighborRoomDatas[i].WasUsedInSearchAlgorithm) { continue; }// || (neighborRoomDatas[i].isSecretRoom && !neighborRoomDatas[i].hasPlayerBeenHere)
		//	roomDatas.Add (neighborRoomDatas[i]);
		//	RecursivelyAddRoomDatasConnectedToRoomData (worldDataRef, ref roomDatas, neighborRoomDatas[i]);
		//}
	}

	public static List<RoomData> GetRoomsConnectedToRoom (WorldData worldDataRef, RoomData sourceRoom, bool doIncludeSourceRoom=true) {
		Dictionary<string, RoomData> allRoomDatas = worldDataRef.RoomDatas;

		List<RoomData> roomsConnectedToSourceRooms = new List<RoomData> ();
		if (doIncludeSourceRoom) { roomsConnectedToSourceRooms.Add (sourceRoom); } // We can opt to include/not include the source room for this function.
		// Now, recursively puck up a big list of all the RoomDatas that are connected to the sourceRoom!
		RecursivelyAddRoomDatasConnectedToRoomData (worldDataRef, ref roomsConnectedToSourceRooms, sourceRoom);

		// Reset used-in-algorithm values for all RoomDatas
		foreach (RoomData rd in allRoomDatas.Values) { rd.WasUsedInSearchAlgorithm = false; }

		// Return!
		return roomsConnectedToSourceRooms;
	}

	/*
//		// Now populate a list of only the neighbors that HAVEN'T been used in the search algorithm.
	//		List<RoomData> unusedNeighborRoomDatas = new List<RoomData> ();
	//		for (int i=0; i<allNeighborRoomDatas.Count; i++) {
	//			if (!allNeighborRoomDatas[i].WasUsedInSearchAlgorithm) {
	//				unusedNeighborRoomDatas.Add (allNeighborRoomDatas[i]);
	//			}
	//		}
	//		// Remove all RoomDatas in the list that HAVE been used in this search!
	//		for (int i=neighborRoomDatas.Count-1; i>=0; --i) {
	//			if (neighborRoomDatas[i].WasUsedInSearchAlgorithm) {
	//				neighborRoomDatas.Remove (neighborRoomDatas[i]);
	//			}
	//		}
	*/
	/** This returns a rect that ONLY INCLUDES rooms connected to the start room! It discards any other rooms in this world. * /
	public static Rect CalculateWorldBoundsPlayableRooms (WorldData worldDataRef) {
		Dictionary<string, RoomData> allRoomDatas = worldDataRef.RoomDatas;

		List<RoomData> roomsComposingViewRect = new List<RoomData> (); // Either all the rooms connected to the start, but if we can't find the start, then we'll fall back to ALL the rooms in this world.

		// Find the STARTING room, bro!
		RoomData startingRoom;
		string startingRoomKey = GameProperties.GetFirstRoomName (worldDataRef.WorldIndex);
		if (allRoomDatas.ContainsKey (startingRoomKey)) { // This starting room exists :)
			startingRoom = allRoomDatas [startingRoomKey];

			// Now, recursively puck up a big list of all the RoomDatas that are connected to the start!
			roomsComposingViewRect.Add (startingRoom); // Add the roomTo to start! The algorithm won't add it itself.
			RecursivelyAddRoomDatasConnectedToRoomData (worldDataRef, ref roomsComposingViewRect, startingRoom);

			// Reset used-in-algorithm values for all RoomDatas
			foreach (RoomData rd in allRoomDatas.Values) { rd.WasUsedInSearchAlgorithm = false; }
		}
		else { // Uhh, this starting room does not exist. So we'll use ALL this worlds' rooms.
			roomsComposingViewRect = new List<RoomData> (allRoomDatas.Values);
		}

		// Now, make the rectangle!
		Rect returnRect = roomsComposingViewRect [0].BoundsGlobal; // Start it with the first room's rect.
		for (int i=0; i<roomsComposingViewRect.Count; i++) {
			returnRect = GameMathUtils.GetCompoundRectangle (returnRect, roomsComposingViewRect[i].BoundsGlobal);
		}
		return returnRect;
	}
	/** Just make a big-ass rectangle that includes ALL rooms in the entire world provided. * /
	static public Rect CalculateWorldBoundsAllRooms (WorldData worldDataRef) {
		// Calculate my boundsRectAllRooms so I can know when the camera is lookin' at me!
		Rect returnRect = new Rect (0,0, 0,0);
		foreach (RoomData rd in worldDataRef.roomDatas.Values) {
			Rect ldBounds = rd.BoundsGlobalMinSize;
			if (returnRect.width==0 && returnRect.height==0) { // FIRST room of the bunch? Cool, make worldsBoundRect start EXACTLY like this room's rect. (So we don't include the origin in every worldBoundsRect.)
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
	public static void SetRoomsNumRoomsFromStart (int WorldIndex, Dictionary<string, Room> rooms) {
		// Default all rooms to NOT even being connected to the start!
		foreach (Room l in rooms.Values) { l.numRoomsFromStart = -1; }
		// Firsht off, if this start room doesn't exist, don't continue doing anything. This whole world is total anarchy.
		Room startRoom = rooms [GameProperties.GetFirstRoomName (WorldIndex)];
		if (startRoom == null) {
			return;
		}

		// Reset used-in-algorithm values for all RoomDatas
		foreach (Room l in rooms.Values) {
			l.RoomDataRef.WasUsedInSearchAlgorithm = false;
		}

		// Okay. Now, starting with the startRoom, 


		// Put the toilet seat back down.
		foreach (Room l in rooms.Values) { l.RoomDataRef.WasUsedInSearchAlgorithm = false; }
	}
	private static void RecursivelySetNumRoomsFromStart (int numRoomsFromStart, Room sourceRoom) {
		// Don't let us use this room again.
		sourceRoom.RoomDataRef.WasUsedInSearchAlgorithm = true;
		Note: It would be fun to finish writing this, but I don't need to. Not the best use of my time.
	}
	*/


}








