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
    
    public const int TEMP_TrialsWorldIndex = 7; // TEMP HACK they're all in W7.
    
    // Rooms!
    public const int NUM_WORLDS = 8; // including World 0 for testing.
    public const int FirstWorld = 1;
    public const int LastWorld = 4;
    public static bool IsFirstCluster(RoomAddress address) {
        return address.world==FirstWorld && address.clust==0;
    }
    public static int ClustNumSnacksReq(RoomAddress address) {
        switch (address.world) {
            // World 1
            case 1:
                switch(address.clust) {
                    case 0: return 0;
                    case 1: return 2;
                    case 2: return 6;
                    case 3: return 14;
                    case 4: return 16;
                    case 5: return 22;
                    case 6: return 40;
                    default: return 30;
                }
            // World 2
            case 2:
                switch(address.clust) {
                    case 0: return 14;
                    case 1: return 20;
                    case 2: return 28;
                    case 3: return 32;
                    case 4: return 40;
                    case 5: return 50;
                    case 6: return 60;
                    default: return 0;
                }
            // World 3
            case 3:
                switch(address.clust) {
                    case 0: return 30;
                    case 1: return 50;
                    case 2: return 70;
                    default: return 80;
                }
            default: return 0;
        }
    }


}


