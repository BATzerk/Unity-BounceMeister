using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class MapEditorSettings {
	static public bool DoShowInstructions;
	static public bool DoShowDesignerFlags;
	static public bool DoShowLevelNames;
	static public bool DoShowLevelTileStars;
	static public bool DoShowLevelProps;



	static public void LoadAll () {
		DoShowInstructions = SaveStorage.GetInt (SaveKeys.MapEditor_DoShowInstructions, 0) == 1;
		DoShowDesignerFlags = SaveStorage.GetInt (SaveKeys.MapEditor_DoShowDesignerFlags, 0) == 1;
		DoShowLevelNames = SaveStorage.GetInt (SaveKeys.MapEditor_DoShowLevelNames, 0) == 1;
		DoShowLevelTileStars = SaveStorage.GetInt (SaveKeys.MapEditor_DoShowLevelTileStars, 0) == 1;
		DoShowLevelProps = SaveStorage.GetInt (SaveKeys.MapEditor_DoShowLevelProps, 0) == 1;
	}
	static public void SaveAll () {
		SaveStorage.SetInt (SaveKeys.MapEditor_DoShowInstructions, DoShowInstructions?1:0);
		SaveStorage.SetInt (SaveKeys.MapEditor_DoShowDesignerFlags, DoShowDesignerFlags?1:0);
		SaveStorage.SetInt (SaveKeys.MapEditor_DoShowLevelNames, DoShowLevelNames?1:0);
		SaveStorage.SetInt (SaveKeys.MapEditor_DoShowLevelTileStars, DoShowLevelTileStars?1:0);
		SaveStorage.SetInt (SaveKeys.MapEditor_DoShowLevelProps, DoShowLevelProps?1:0);
	}




//	public bool Debug_doShowLevelTileDesignerFlags {
//		get { return debug_doShowLevelTileDesignerFlags; }
//		set {
//			if (debug_doShowLevelTileDesignerFlags != value) {
//				debug_doShowLevelTileDesignerFlags = value;
//				SaveStorage.SetInt(SaveKeys.DEBUG_DO_SHOW_LEVEL_TILE_DESIGNER_FLAGS, debug_doShowLevelTileDesignerFlags ? 1 : 0);
//			}
//		}
//	}
}
