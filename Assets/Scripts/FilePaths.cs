using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilePaths {

	public static string WorldFileAddress(int worldIndex) {
		return Application.streamingAssetsPath + "/Levels/World" + worldIndex + "/";
	}
	public static string WorldTrashFileAddress(int worldIndex) {
		return Application.streamingAssetsPath + "/Levels/WorldTrash" + worldIndex + "/";
	}

}
