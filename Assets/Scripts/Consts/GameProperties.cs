using UnityEngine;
using System.Collections;

public class GameProperties : MonoBehaviour {
    public static bool IsEditModeAvailable {
        // Currently, EditMode is only available via the Unity Editor.
        get { return Application.isEditor; }
    }


    public const float UnitSize = 1f; // Grid-snap units! In Unity units.

	public const int NUM_WORLDS = 4;

	public static string GetFirstLevelName(int worldIndex) {
		switch(worldIndex) {
		default: return "Level0";
		}
	}
	public static string GetLastLevelName(int worldIndex) {
		switch(worldIndex) {
		default: return "WorldEnd";
		}
	}



}


