using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilePaths {

    public static string RoomFile(int worldIndex, string roomKey) {
        return WorldFileAddress(worldIndex) + roomKey + ".txt";
    }
    public static string WorldFileAddress(int worldIndex) {
        return Application.streamingAssetsPath + "/Levels/World" + worldIndex + "/";
    }
    public static string WorldTrashFileAddress(int worldIndex) {
		return Application.streamingAssetsPath + "/Levels/WorldTrash" + worldIndex + "/";
	}
	//public static string RoomLinksFileAddress(int WorldIndex) {
	//	return WorldFileAddress (WorldIndex) + "_RoomLinks.txt";
	//}

}
