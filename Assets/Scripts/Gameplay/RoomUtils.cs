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








