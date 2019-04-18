using UnityEngine;
using System.Collections;

static public class GameProperties {
    // Editor
    public const bool DoPauseInEditMode = true; // if TRUE, we'll automatically pause gameplay while editing a room in Gameplay Scene.
    public static bool IsEditModeAvailable {
        // Currently, EditMode is only available via the Unity Editor.
        get { return Application.isEditor; }
    }
    
    // Gameplay
    public const float UnitSize = 1f; // Grid-snap units! In Unity units.
	public const int NUM_WORLDS = 6; // including World 0 for testing.

    public static string GetFirstRoomName(int worldIndex) {
        switch (worldIndex) {
            default: return "WorldStart";
        }
    }
    public static string GetLastRoomName(int worldIndex) {
        switch (worldIndex) {
            default: return "WorldEnd";
        }
    }


}


