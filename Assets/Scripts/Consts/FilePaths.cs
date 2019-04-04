using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilePaths {

    public static string LevelFileAddress(int worldIndex, string levelKey) {
        return WorldFileAddress(worldIndex) + levelKey + ".txt";
    }
    public static string WorldFileAddress(int worldIndex) {
        return Application.streamingAssetsPath + "/Levels/World" + worldIndex + "/";
    }
    public static string WorldTrashFileAddress(int worldIndex) {
		return Application.streamingAssetsPath + "/Levels/WorldTrash" + worldIndex + "/";
	}
	//public static string LevelLinksFileAddress(int WorldIndex) {
	//	return WorldFileAddress (WorldIndex) + "_LevelLinks.txt";
	//}

}
