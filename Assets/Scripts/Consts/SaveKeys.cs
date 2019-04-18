using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveKeys {
    // Global Gameplay
    public const string LastPlayedPlayerType = "LastPlayedPlayerType";
    public static string IsPlayerTypeUnlocked(PlayerTypes playerType) { return "IsPlayerTypeUnlocked_" + playerType.ToString(); }
    
    public const string LastPlayedWorldIndex = "LastPlayedWorldIndex";
    public static string LastPlayedRoomKey(int worldIndex) { return "LastPlayedRoomKey_w" + worldIndex; }
    
    
    // Room-Specifics
    private static string FullRoomKey(Room r) { return FullRoomKey(r.MyRoomData); }
    private static string FullRoomKey(RoomData rd) { return "w" + rd.WorldIndex + "_" + rd.RoomKey; } // e.g. returns "w2_JumpPit".
    
    public static string HasPlayerBeenInRoom(RoomData rd) { return "HasPlayerBeenInRoom_" + FullRoomKey(rd); }
    
    public static string DidEatGem(Room room, int objIndex) { return DidEatGem(room.MyRoomData, objIndex); }
    public static string DidEatGem(RoomData rd, int objIndex) { return "DidEatGem_" + FullRoomKey(rd) + "_" + objIndex; }
    public static string DidEatSnack(Room room, int objIndex) { return DidEatSnack(room.MyRoomData, objIndex); }
    public static string DidEatSnack(RoomData rd, int objIndex) { return "DidEatSnack_" + FullRoomKey(rd) + "_" + objIndex; }
    
    public static string IsGateUnlocked(Room room, int objIndex) { return IsGateUnlocked(room.MyRoomData, objIndex); }
    public static string IsGateUnlocked(RoomData rd, int objIndex) { return "IsGateUnlocked_" + FullRoomKey(rd) + "_" + objIndex; }

    public static string CharBarrelTypeInMe(RoomData rd, int objIndex) { return "CharBarrelTypeInMe_" + FullRoomKey(rd) + "_" + objIndex; }


    // Editor
    public const string MapEditor_CameraPosX = "MapEditor_CameraPosX";
	public const string MapEditor_CameraPosY = "MapEditor_CameraPosY";
	public const string MapEditor_MapScale = "MapEditor_MapScale";
    public const string MapEditor_DoShowClusters = "MapEditor_DoShowClusters";
    public const string MapEditor_DoShowDesignerFlags = "MapEditor_DoShowDesignerFlags";
    public const string MapEditor_DoShowInstructions = "MapEditor_DoShowInstructions";
	public const string MapEditor_DoShowRoomNames = "MapEditor_DoShowRoomNames";
	public const string MapEditor_DoShowRoomEdibles = "MapEditor_DoShowRoomEdibles";
	public const string MapEditor_DoShowRoomProps = "MapEditor_DoShowRoomProps";
	public const string MapEditor_DoMaskRoomContents = "MapEditor_DoMaskRoomContents";
}
