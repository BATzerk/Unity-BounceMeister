using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditorNamespace {
public class MapEditorSettings {
	public bool DoMaskRoomContents;
	public bool DoShowInstructions;
    public bool DoShowClusters;
	public bool DoShowDesignerFlags;
    public bool DoShowRoomEdibles;
    public bool DoShowRoomNames;
	public bool DoShowRoomProps;


	public MapEditorSettings() {
		DoMaskRoomContents = SaveStorage.GetBool (SaveKeys.MapEditor_DoMaskRoomContents, true);
        DoShowClusters = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowClusters, true);
        DoShowDesignerFlags = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowDesignerFlags, false);
        DoShowInstructions = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowInstructions, true);
        DoShowRoomEdibles = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowRoomEdibles, true);
        DoShowRoomNames = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowRoomNames, true);
		DoShowRoomProps = SaveStorage.GetBool (SaveKeys.MapEditor_DoShowRoomProps, true);
	}
	public void SaveAll () {
		SaveStorage.SetBool (SaveKeys.MapEditor_DoMaskRoomContents, DoMaskRoomContents);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowClusters, DoShowClusters);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowDesignerFlags, DoShowDesignerFlags);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowInstructions, DoShowInstructions);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowRoomEdibles, DoShowRoomEdibles);
        SaveStorage.SetBool (SaveKeys.MapEditor_DoShowRoomNames, DoShowRoomNames);
		SaveStorage.SetBool (SaveKeys.MapEditor_DoShowRoomProps, DoShowRoomProps);
	}


}
}
