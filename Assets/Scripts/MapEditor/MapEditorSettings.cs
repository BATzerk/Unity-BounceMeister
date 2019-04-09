using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorSettings {
	public bool DoMaskLevelContents;
	public bool DoShowInstructions;
    public bool DoShowClusters;
	public bool DoShowDesignerFlags;
    public bool DoShowLevelEdibles;
    public bool DoShowLevelNames;
	public bool DoShowLevelProps;


	public MapEditorSettings() {
		DoMaskLevelContents = SaveStorage.GetBool (SaveKeys.MapEditor_DoMaskLevelContents, true);
        DoShowClusters = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowClusters, true);
        DoShowDesignerFlags = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowDesignerFlags, false);
        DoShowInstructions = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowInstructions, true);
        DoShowLevelEdibles = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowLevelEdibles, true);
        DoShowLevelNames = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowLevelNames, true);
		DoShowLevelProps = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowLevelProps, true);
	}
	public void SaveAll () {
		SaveStorage.SetBool (SaveKeys.MapEditor_DoMaskLevelContents, DoMaskLevelContents);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowClusters, DoShowClusters);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowDesignerFlags, DoShowDesignerFlags);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowInstructions, DoShowInstructions);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowLevelEdibles, DoShowLevelEdibles);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowLevelNames, DoShowLevelNames);
		SaveStorage.SetBool (SaveKeys.MapEditor_DoShowLevelProps, DoShowLevelProps);
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
