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
    public const int FirstWorld = 1;
    public const int LastWorld = 4;
    
    
    // Rooms!
    public static int ClustNumSnacksReq(RoomAddress address) {
        switch (address.world) {
            // World 1
            case 1:
                switch(address.clust) {
                    case 0: return 0;
                    case 1: return 3;
                    case 2: return 10;
                    case 3: return 20;
                    default: return 0;
                }
            // World 2
            case 2:
                switch(address.clust) {
                    case 0: return 20;
                    case 1: return 30;
                    case 2: return 40;
                    default: return 0;
                }
            default: return 0;
        }
    }
    
    public static bool IsFirstCluster(RoomAddress address) {
        return address.world==1 && address.clust==0;
    }
    
    // TODO: Remove these! Replace with cluster knowledge!
    public static string GetFirstRoomName(int worldIndex) {
        switch (worldIndex) {
            case 1: return "IntroGame1";
            default: return "Clust0Start";//WorldStart";
        }
    }
    public static string GetLastRoomName(int worldIndex) {
        switch (worldIndex) {
            default: return "WorldEnd";
        }
    }


}


