using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveKeys {
    // Global Gameplay
    public const string LastPlayedPlayerType = "LastPlayedPlayerType";
    public static string IsPlayerTypeUnlocked(PlayerTypes playerType) { return "IsPlayerTypeUnlocked_" + playerType.ToString(); }
    
    public const string LastPlayedWorldIndex = "LastPlayedWorldIndex";
    public static string LastPlayedLevelKey(int worldIndex) { return "LastPlayedLevelKey_w" + worldIndex; }
    
    
    // Level-Specifics
    private static string FullLvlKey(Level l) { return FullLvlKey(l.LevelDataRef); }
    private static string FullLvlKey(LevelData ld) { return "w" + ld.WorldIndex + "_" + ld.LevelKey; } // e.g. returns "w2_JumpPit".
    
    public static string HasPlayerBeenInLevel(LevelData ld) { return "HasPlayerBeenInLevel_" + FullLvlKey(ld); }
    
    public static string DidEatGem(Level level, int objIndex) { return DidEatGem(level.LevelDataRef, objIndex); }
    public static string DidEatGem(LevelData ld, int objIndex) { return "DidEatGem_" + FullLvlKey(ld) + "_" + objIndex; }
    public static string DidEatSnack(Level level, int objIndex) { return DidEatSnack(level.LevelDataRef, objIndex); }
    public static string DidEatSnack(LevelData ld, int objIndex) { return "DidEatSnack_" + FullLvlKey(ld) + "_" + objIndex; }
    
    public static string IsGateUnlocked(Level level, int objIndex) { return IsGateUnlocked(level.LevelDataRef, objIndex); }
    public static string IsGateUnlocked(LevelData ld, int objIndex) { return "IsGateUnlocked_" + FullLvlKey(ld) + "_" + objIndex; }

    public static string CharBarrelTypeInMe(LevelData ld, int objIndex) { return "CharBarrelTypeInMe_" + FullLvlKey(ld) + "_" + objIndex; }


    // Editor
    public const string MapEditor_CameraPosX = "MapEditor_CameraPosX";
	public const string MapEditor_CameraPosY = "MapEditor_CameraPosY";
	public const string MapEditor_MapScale = "MapEditor_MapScale";
    public const string MapEditor_DoShowClusters = "MapEditor_DoShowClusters";
    public const string MapEditor_DoShowDesignerFlags = "MapEditor_DoShowDesignerFlags";
    public const string MapEditor_DoShowInstructions = "MapEditor_DoShowInstructions";
	public const string MapEditor_DoShowLevelNames = "MapEditor_DoShowLevelNames";
	public const string MapEditor_DoShowLevelEdibles = "MapEditor_DoShowLevelEdibles";
	public const string MapEditor_DoShowLevelProps = "MapEditor_DoShowLevelProps";
	public const string MapEditor_DoMaskLevelContents = "MapEditor_DoMaskLevelContents";
}
