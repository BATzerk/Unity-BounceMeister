using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveKeys {

	public const string LastPlayedWorldIndex = "LastPlayedWorldIndex";
    public static string LastPlayedLevelKey(int worldIndex) { return "LastPlayedLevelKey_w" + worldIndex; }

    private static string FullLvlKey(Level l) { return "w" + l.WorldIndex + "_" + l.LevelKey; } // e.g. returns "w2_JumpPit".

    public static string DidEatGem(Level level, int gemIndex) { return "DidEatGem_" + FullLvlKey(level) + "_" + gemIndex; }


    public const string MapEditor_CameraPosX = "MapEditor_CameraPosX";
	public const string MapEditor_CameraPosY = "MapEditor_CameraPosY";
	public const string MapEditor_MapScale = "MapEditor_MapScale";
	public const string MapEditor_DoShowInstructions = "MapEditor_DoShowInstructions";
	public const string MapEditor_DoShowDesignerFlags = "MapEditor_DoShowDesignerFlags";
	public const string MapEditor_DoShowLevelNames = "MapEditor_DoShowLevelNames";
	public const string MapEditor_DoShowLevelTileStars = "MapEditor_DoShowLevelTileStars";
	public const string MapEditor_DoShowLevelProps = "MapEditor_DoShowLevelProps";
	public const string MapEditor_DoMaskLevelContents = "MapEditor_DoMaskLevelContents";
}
