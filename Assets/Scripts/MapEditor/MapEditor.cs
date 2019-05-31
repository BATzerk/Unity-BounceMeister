using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace MapEditorNamespace {
public class MapEditor : MonoBehaviour {
	// Constants
	private readonly Vector2 GridSize = new Vector2(2, 2);
	private const string instructionsTextString_enabled = "set world:  [1-8]\ntoggle names:  'n'\ntoggle contents:  'p'\nsearch:  [hold 'SHIFT' and type room name; ESC to cancel]\nhide instructions:  'i'";//move/zoom:  [mouse or arrow keys]\nplay room:  [double-click one]\n
	private const string instructionsTextString_disabled = "show instructions:  'i'";
	// Components
	[SerializeField] private RoomTileSelectionRect selectionRect=null;
	private List<GameObject> worldLayerGOs; // purely for hierarchy cleanness, we wanna put all the tiles into their respective world's GameObject.
	private List<List<RoomTile>> allRoomTiles; // RoomTiles for EVERY ROOM IN THE GAME!
	// Properties
	private bool isSearchingRoom; // When we start typing letters, yeah! Narrow down our options.
    private bool isKeyMultiSelection; // SHIFT key allows us to select/deselect multiple Tiles!
	private int currWorldIndex=-1;
	private string roomSearchString = "";
    public MapEditorSettings MySettings { get; private set; }
	public Vector2 MousePosScreenOnDown { get; private set; }
	public Vector2 MousePosWorld { get; private set; }
	// References
	[SerializeField] private Text currentWorldText=null;
	[SerializeField] private Text instructionsText=null;
	private List<RoomTile> tilesSelected = new List<RoomTile>();
    private MapEditorCamera editorCamera;
	public WorldData CurrWorldData { get { return GetWorldData(currWorldIndex); } }
	private WorldData GetWorldData (int worldIndex) {
		if (worldIndex<0 || dataManager.NumWorldDatas == 0) { return null; }
		return dataManager.GetWorldData (worldIndex);
	}
	
	
	// ================================================================
	//  Getters
	// ================================================================
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private InputController inputController { get { return InputController.Instance; } }
	private float fTS { get { return TimeController.FrameTimeScaleUnscaled; } } // frame time scale
	private List<RoomTile> CurrWorldRoomTiles { get { return allRoomTiles==null?null : allRoomTiles [currWorldIndex]; } }

	//public bool CanSelectARoomTile() {
	//	// NO tiles selected and mouse isn't down? Yeah, return true!
	//	if (tilesSelected.Count==0 && !Input.GetMouseButton(0)) { return true; }
	//	// Okay, if some might be selected, then return if we may we select multiple ones!
	//	if (isKeyMultiSelection) { return true; }
	//	// Hmm. No, there's no way we can select a RoomTile right now.
	//	return false;
	//}
	/** We may only move RoomTiles while running in the UNITY EDITOR. Don't allow moving tiles in a BUILD version. */
	private bool MayMoveRoomTiles () {
		bool mayMove = false;
		#if UNITY_EDITOR
		mayMove = true;
		#endif
		return mayMove;
	}
	public bool IsDraggingSelectedRoomTiles() {
		return MayMoveRoomTiles() && !isKeyMultiSelection && Input.GetMouseButton (0);
	}
	private RoomTile GetRoomTileByKey (string roomKey) {
		for (int i=0; i<CurrWorldRoomTiles.Count; i++) {
			if (CurrWorldRoomTiles[i].RoomKey == roomKey) {
				return CurrWorldRoomTiles[i];
			}
		}
		return null;
	}

    public float MapScale { get { return editorCamera.MapScale; } }
    private Vector2 SnapToGrid(Vector2 v) { return new Vector2(Mathf.Floor(v.x/GridSize.x)*GridSize.x, Mathf.Floor(v.y/GridSize.y)*GridSize.y); }
	public Vector2 MousePosScreen { get { return inputController.MousePosScreen; } }
    private Vector2 MousePosWorldDragging(Vector2 _mouseClickOffset) {
		return MousePosWorld + _mouseClickOffset;
	}
	public Vector2 MousePosWorldDraggingGrid(Vector2 _mouseClickOffset) { // Return the mouse position, scaled to the screen and snapped to the grid.
        bool doSnap = !Input.GetKey (KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl); // Hold down CONTROL to prevent snapping.
        if (doSnap) {
            Vector2 clickOffset = SnapToGrid(_mouseClickOffset + GridSize);
            return SnapToGrid(MousePosWorldDragging(clickOffset));
        }
        else {
            return MousePosWorldDragging(_mouseClickOffset);
        }
    }
	private RoomTile GetVisibleTileAtPoint(Vector2 point) {
		if (CurrWorldRoomTiles==null) { return null; } // Safety check for runtime compile.
		for (int i=CurrWorldRoomTiles.Count-1; i>=0; --i) { // loop thru backwards so we click NEWER tiles before older ones.
            RoomTile tile = CurrWorldRoomTiles[i];
            if (!tile.IsFullyVisible) { continue; } // Tile's not available?? Skip it!
			Rect boundsGlobal = tile.BoundsGlobal;
			if (boundsGlobal.Contains(point+boundsGlobal.size*0.5f)) { // Note: convert back to center-aligned. SIGH. I would love to just make room's rect corner-aligned. Avoid any ambiguity.
				return tile;
			}
		}
		return null;
	}

//	private void OnGUI () {
//		// Map-Editor instructions!
//		Rect textRect = new Rect (-Screen.width*0.5f, Screen.height*0.5f, Screen.width,Screen.height);
//		string textString = 
//		GUIStyle labelStyle = GUI.skin.GetStyle ("Label");
//		labelStyle.alignment = TextAnchor.UpperLeft;
//		labelStyle.fontSize = (int) (18 * GameVisualProperties.ScreenScale);
////		labelStyle.fontStyle = FontStyle.Bold;
//		GUI.Label (textRect, textString);
//	}
	
	
	
	
	// ================================================================
	//  Initialize / Reset
	// ================================================================
	private void Start() {
		// Find references
		editorCamera = FindObjectOfType<MapEditorCamera>();
        MySettings = new MapEditorSettings();
        this.gameObject.AddComponent<MapEditorGizmos>(); // add my Gizmos script.
		
		// Reload everything right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
		#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
		#endif

//		// Set the whole map's scale to be the same as how the game's scale works.
//		this.transform.localScale = new Vector3 (GameProperties.WORLD_SCALE, GameProperties.WORLD_SCALE, 1);

		// Make the worldLayerGOs and RoomTiles only once.
		MakeWorldLayerGOs();
        LoadAllRoomTiles();

        // Reset values
        Time.timeScale = 1f;
		Cursor.visible = true;

		// Set current world
		SetCurrWorld(SaveStorage.GetInt(SaveKeys.LastPlayedWorldIndex));
	}

	private void SetCurrWorld (int _worldIndex) {
		if (_worldIndex >= GameProperties.NUM_WORLDS) { return; } // Don't crash da game, bruddah.

		// Deselect any tiles that might be selected!
		DeselectAllRoomTiles();

		// If we're CHANGING the currentWorld...!!
		//if (currWorldIndex != _worldIndex) {
			// Tell all the tiles in the world we already were to hide their stuff!
			if (currWorldIndex != -1) {
				for (int i=0; i<CurrWorldRoomTiles.Count; i++) {
					CurrWorldRoomTiles[i].Hide();
				}
			}
			currWorldIndex = _worldIndex;
            SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, currWorldIndex);
        //}
        
        // Tell all the tiles in the NEW world to show their stuff!
        for (int i=0; i<CurrWorldRoomTiles.Count; i++) {
            CurrWorldRoomTiles[i].Show();
        }
        
        // Dispatch hevent!
        GameManagers.Instance.EventManager.OnMapEditorSetCurrWorld(currWorldIndex);
	}
    

	private void MakeWorldLayerGOs () {
		worldLayerGOs = new List <GameObject> ();
		for (int worldIndex=0; worldIndex<GameProperties.NUM_WORLDS; worldIndex++) {
			// Make a world GameObject to tidy up the hierarchy.
			GameObject worldLayerGO = new GameObject ();
			worldLayerGO.transform.SetParent (this.transform);
			worldLayerGO.transform.localPosition = Vector3.zero;
			worldLayerGO.transform.localScale = Vector3.one;
			worldLayerGO.name = "World" + worldIndex;
			worldLayerGOs.Add (worldLayerGO);
		}
	}
	private void LoadAllRoomTiles() {
		// Destroy 'em first!
		DestroyAllRoomTiles();
		// Make 'em hot & fresh!
		allRoomTiles = new List<List<RoomTile>>();
		GameObject prefab = ResourcesHandler.Instance.MapEditor_RoomTile;
		// For every world...
		for (int worldIndex=0; worldIndex<dataManager.NumWorldDatas; worldIndex++) {
			allRoomTiles.Add (new List<RoomTile>());
			// For every room in this world...
			WorldData wd = dataManager.GetWorldData (worldIndex);
            Transform parent = worldLayerGOs[worldIndex].transform;
            foreach (RoomData roomData in wd.RoomDatas.Values) {
                RoomTile newRoomTile = Instantiate (prefab).GetComponent<RoomTile>();
				newRoomTile.Initialize(this, roomData, parent);
				allRoomTiles[worldIndex].Add(newRoomTile);
			}
		}
        //currWorldIndex = -1; // reset this! 'cause there's no world selected right now.
	}
    
    private void ReloadAllTiles() {
        dataManager.ReloadWorldDatas();
        LoadAllRoomTiles();
        SetCurrWorld(currWorldIndex);
    }
	private void ReloadAllWorldDatasAndScene() {
        dataManager.ReloadWorldDatas();
        SceneHelper.ReloadScene();
    }




    // ================================================================
    //  Doers: Room Tiles
    // ================================================================
	private void SelectTile(RoomTile tile) {
        if (tile.IsSelected) { return; } // Safety check.
		/*
		// First, check if we're trying to set ANOTHER, different roomTile as roomTileDragging.
		if (roomTileDragging != null && thisRoomTile != null && roomTileDragging != thisRoomTile) {
			Debug.LogWarning("Hey, whoa! We're trying to set roomTileDragging to a RoomTile, but it's not already null!");
			return;
		}
		// Was dragging a RoomTile? Update its visuals!
		if (roomTileDragging != null) {
			roomTileDragging.UpdateBorderLine();
		}
		// Set it!
		roomTileDragging = thisRoomTile;
		// Update its visuals!
		if (roomTileDragging != null) {
			roomTileDragging.UpdateBorderLine();
		}
		// We DID specifiy a roomTile?
		if (roomTileDragging != null) {
			SetMouseClickOffset(roomTileDragging.transform.localPosition);
//			SetMouseClickOffset(roomTileDragging.Pos);//new PVector(roomTileDragging.pos.x+roomTileDragging.w*0.5, roomTileDragging.pos.y+roomTileDragging.h*0.5));
		}
		*/
		// Add it to my list!
		tilesSelected.Add(tile);
		// Tell it what's up!
		tile.OnSelected(MousePosWorld);
	}
	private void DeselectTile(RoomTile tile) {
        if (!tile.IsSelected) { return; } // Safety check.
		tile.OnDeselected ();
		tilesSelected.Remove (tile);
	}
    private void SelectTiles(List<RoomTile> tiles) { foreach (RoomTile tile in tiles) { SelectTile(tile); } }
    private void DeselectTiles(List<RoomTile> tiles) { foreach (RoomTile tile in tiles) { DeselectTile(tile); } }
	private void DeselectAllRoomTiles() {
		for (int i=tilesSelected.Count-1; i>=0; --i) {
			DeselectTile (tilesSelected [i]);
		}
		tilesSelected = new List<RoomTile> ();
	}
    private void SelectTiles(List<string> roomKeys) {
        foreach (string roomKey in roomKeys) {
            RoomTile tile = GetRoomTileByKey(roomKey);
            SelectTile(tile);
        }
    }
	private void DeselectTilesSelected() {
		// Tell all the tiles they've been deselected and clear out the list.
		foreach (RoomTile roomTile in tilesSelected) {
			roomTile.OnDeselected ();
		}
		// Clear out the list, of course.
		tilesSelected.Clear ();
		// Brute-force remake all the wallLines for EVERY RoomTile. It's dumb, but easy.
//		CurrWorldData.SetAllRoomDatasBounds ();

//		foreach (RoomTile roomTile in CurrentWorldRoomTiles) {
//			roomTile.RemakeWallLines ();
//		}
	}
	private void SelectAllRoomTiles() {
		DeselectTilesSelected(); // Just in case release any if we're already holding some.
		foreach (RoomTile roomTile in CurrWorldRoomTiles) {
			SelectTile(roomTile);
		}
	}
    
    private void DuplicateSelectedRooms() {
        List<string> newRoomKeys = new List<string>();
        foreach (RoomTile rt in tilesSelected) {
            newRoomKeys.Add(DuplicateRoom(rt.MyRoomData));
        }
        // Reload the tiles.
        //dataManager.ReloadWorldDatas();
        LoadAllRoomTiles();
        SetCurrWorld(currWorldIndex);
        // Select duplicated tiles.
        SelectTiles(newRoomKeys);
    }
    private string DuplicateRoom(RoomData originalData) {
        // Add a new room file, yo!
        string newRoomKey = originalData.MyWorldData.GetUnusedRoomKey(originalData.RoomKey);
        RoomSaverLoader.SaveRoomFileAs(originalData, originalData.WorldIndex, newRoomKey);
        return newRoomKey;
    }

	private void MoveRoomTilesSelectedRoomFilesToTrashFolder() {
		// No tiles selected? Womp, don't do anything LOL
		if (tilesSelected.Count == 0) { return; }
		// Move the files!!
		for (int i=tilesSelected.Count-1; i>=0; --i) {
//			DeselectRoomTile (roomTilesSelected[i]);
			CurrWorldData.MoveRoomFileToTrashFolder (tilesSelected[i].RoomKey);
		}
		
		// Reload everything right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
		#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
		#endif

		// Reload this map, yo.
		ReloadAllTiles();
	}

	private void MoveRoomTilesSelectedToWorld (int worldIndexTo) {
		// No tiles selected? Womp, don't do anything LOL
		if (tilesSelected.Count == 0) { return; }
		int worldIndexFrom = tilesSelected[0].WorldIndex; // We can assume all roomTilesSelected are in the same world.
		//WorldData worldDataTo = GetWorldData (worldIndexTo);
		//WorldData worldDataFrom = GetWorldData (worldIndexFrom);
//		string worldToFolderName = GameProperties.GetWorldName (worldIndexTo); // name of the folda we're moving the files TO.
		// If we're trying to move these tiles to the world they're ALREADY in, do nothin'!
		if (worldIndexFrom == worldIndexTo) { return; }
		// Compile a list of the DATAS of all the rooms we're moving.
		RoomData[] roomDatasMoving = new RoomData [tilesSelected.Count];
		for (int i=0; i<roomDatasMoving.Length; i++) {
			roomDatasMoving[i] = tilesSelected[i].MyRoomData;
		}
		// MOVE ROOMS!
		for (int i=0; i<roomDatasMoving.Length; i++) {
			string roomKey = roomDatasMoving[i].RoomKey;
			// Otherwise, move that glitterbomb!
//			DeselectRoomTile (roomTilesSelected[i]);
			CurrWorldData.MoveRoomFileToWorldFolder (roomKey, worldIndexTo);
		}
		
		// Reload this map, yo.
		ReloadAllTiles();
	}

    private void AddAndStartNewRoom() {
        // Make a RoomData with a unique name, put it where the Camera is, and open the room!
        string roomKey = CurrWorldData.GetUnusedRoomKey();
        RoomData newLD = CurrWorldData.GetRoomData(roomKey, true);
        Vector2 pos = SnapToGrid(editorCamera.Pos);
        newLD.SetPosGlobal(pos);
        SceneHelper.OpenGameplayScene(newLD);
    }
    
    //private void SnapTilesSelectedToGrid() {
    //    foreach (RoomTile tile in tilesSelected) {
    //        Vector2 pos = tile.MyRoomData.PosGlobal;
    //        pos = SnapToGrid(pos);
    //        tile.MyRoomData.SetPosGlobal(pos);
    //    }
    //}
    private void ToggleTilesSelectedIsSecret() {
        // Toggle and save 'em all.
        foreach (RoomTile tile in tilesSelected) {
            RoomData rd = tile.MyRoomData;
            rd.SetIsSecret(!rd.IsSecret);
            RoomSaverLoader.UpdateRoomPropertiesInRoomFile(rd);
            tile.RefreshAllVisuals();
        }
    }
    private void ToggleTilesSelectedIsClustStart() {
        // Toggle and save 'em all.
        foreach (RoomTile tile in tilesSelected) {
            RoomData rd = tile.MyRoomData;
            rd.isClustStart = !rd.isClustStart;
            RoomSaverLoader.UpdateRoomPropertiesInRoomFile(rd);
        }
        // Reload the world, as our Clusters may have changed from this.
        ReloadAllTiles();
    }



    // ================================================================
    //  Events
    // ================================================================
    public void OnSetMapScale() {
        // Update my room tiles' text scales!
        if (allRoomTiles != null) {
            foreach (RoomTile roomTile in CurrWorldRoomTiles) {
                roomTile.OnMapScaleChanged();
            }
        }
    }
    private void OnClickRoomTile(RoomTile tile) {
        //// Conditions are right for selecting the tile!
        //if (CanSelectARoomTile()) {
            // Determine which tiles are affected.
            List<RoomTile> tiles = GetTilesInClick(tile);
            bool doSelect = !tile.IsSelected;
            
            // MULTI-SELECT key is down??...
            if (isKeyMultiSelection) {
                if (doSelect) { SelectTiles(tiles); }
                else { DeselectTiles(tiles); }
            }
            // MULTI-SELECT key ISN'T down...!
            else {
                // This tile's NOT already selected. Deselect all tiles first.
                if (!tile.IsSelected) {
				    DeselectTilesSelected();
                }
                // Select the affected tiles!
                SelectTiles(tiles);
			}
		//}
	}
    
    private List<RoomTile> GetTilesInClick(RoomTile sourceTile) {
        // SHIFT = Consider ALL CONNECTED tiles!
        if (InputController.IsKey_shift) { return GetConnectedTiles(sourceTile); }
        // Otherwise, just return the ONE Tile.
        return new List<RoomTile> { sourceTile };
    }
    /// Returns all RoomTiles that're connected to this one (including this one).
    private List<RoomTile> GetConnectedTiles(RoomTile firstTile) {
        RoomData firstRD = firstTile.MyRoomData;
        List<RoomData> roomDatas = RoomUtils.GetRoomsConnectedToRoom(firstRD);
        List<RoomTile> tiles = new List<RoomTile>();
        foreach (RoomData rd in roomDatas) {
            tiles.Add(GetRoomTileByKey(rd.RoomKey));
        }
        return tiles;
    }

	private void RefreshAllTileVisuals() {
		for (int i=0; i<CurrWorldRoomTiles.Count; i++) { CurrWorldRoomTiles[i].RefreshAllVisuals(); }
//		for (int i=0; i<roomLinkViews.Count; i++) { roomLinkViews[i].UpdateComponentVisibilities(); }
	}
    private void TogSettings_RoomContentMasks() {
        MySettings.DoMaskRoomContents = !MySettings.DoMaskRoomContents;
        OnChangeSettings();
    }
    private void TogSettings_DoShowClusters() {
        MySettings.DoShowClusters = !MySettings.DoShowClusters;
        OnChangeSettings();
    }
    private void TogSettings_DoShowDesignerFlags() {
        MySettings.DoShowDesignerFlags = !MySettings.DoShowDesignerFlags;
        OnChangeSettings();
    }
    private void TogSettings_DoShowRoomNames() {
        MySettings.DoShowRoomNames = !MySettings.DoShowRoomNames;
        OnChangeSettings();
    }
    private void TogSettings_DoShowEdibles() {
        MySettings.DoShowRoomEdibles = !MySettings.DoShowRoomEdibles;
        OnChangeSettings();
    }
    private void TogSettings_DoShowProps() {
        MySettings.DoShowRoomProps = !MySettings.DoShowRoomProps;
        OnChangeSettings();
    }
    private void TogSettings_DoShowInstructions() {
        MySettings.DoShowInstructions = !MySettings.DoShowInstructions;
        OnChangeSettings();
    }
    private void OnChangeSettings() {
        RefreshAllTileVisuals();
        MySettings.SaveAll();
    }

    public void OnRoomTileSelectionRectDeactivated () {
		if (CurrWorldRoomTiles==null) { return; } // Safety check for runtime compile.
		// Select all the extra ones selected by the selection rect!
		for (int i=0; i<CurrWorldRoomTiles.Count; i++) {
			if (CurrWorldRoomTiles[i].IsInSelectionRect) {
				if (!tilesSelected.Contains(CurrWorldRoomTiles[i])) {
					SelectTile (CurrWorldRoomTiles[i]);
				}
			}
		}
	}

    private void ClearRoomSearch() {
        isSearchingRoom = false;
        roomSearchString = "";
        UpdateRoomTilesFromSearchString();
    }
    private void UpdateRoomTilesFromSearchString () {
		// Update their visibilities!
		for (int i=0; i<CurrWorldRoomTiles.Count; i++) {
			CurrWorldRoomTiles[i].UpdateVisibilityFromSearchCriteria (roomSearchString);
		}
	}
    
    private void UpdateIsKeyMultiSelection() {
        isKeyMultiSelection = InputController.IsKey_control;
        //// Tell all the (curr world) tiles!
        //for (int i=0; i<CurrWorldRoomTiles.Count; i++) {
        //    CurrWorldRoomTiles[i].UpdateIsDragReadyMouseOverMe();
        //}
    }
	
	
	
	
	// ================================================================
	//  Update
	// ================================================================
	private void Update() {
		if (inputController==null) { return; } // Safety check for runtime compile.

		UpdateMousePosWorld();
		RegisterKeyInputs();
		RegisterMouseInputs();
		UpdateSelectionRectSelection();
		UpdateUI();
	}

	private void UpdateMousePosWorld() {
//		inputManager.UpdateMousePosRelative (editorCamera.transform.localPosition, mapScale);
		MousePosWorld = inputController.MousePosScreen;// / ScreenHandler.ScreenScale;// - new Vector3(ScreenHandler.OriginalScreenSize.x,ScreenHandler.OriginalScreenSize.y,0)*0.5f;
		MousePosWorld /= editorCamera.MapScale;
		MousePosWorld += editorCamera.Pos;
	}

	private void UpdateUI () {
		// We can afford to update these every frame.
		// currentWorldText
		currentWorldText.text = currWorldIndex.ToString ();
		// instructionsText
		if (MySettings.DoShowInstructions) {
			instructionsText.color = new Color (1,1,1, 0.36f);
			instructionsText.text = instructionsTextString_enabled;
		}
		else {
			instructionsText.color = new Color (1,1,1, 0.3f);
			instructionsText.text = instructionsTextString_disabled;
		}
	}

	private void UpdateSelectionRectSelection() {
		if (CurrWorldRoomTiles==null) { return; } // Safety check for runtime compile.
		// Update which tiles are within the rect!
		for (int i=0; i<CurrWorldRoomTiles.Count; i++) {
			CurrWorldRoomTiles[i].IsInSelectionRect =
                   CurrWorldRoomTiles[i].BodyCollider.IsEnabled
                && selectionRect.IsActive
                && selectionRect.SelectionRect.Contains(CurrWorldRoomTiles[i].MyRoomData.PosGlobal);
		}
//		if (roomTileSelectionRect.IsActive) {
//			roomTileSelectionRect.roomTilesSelected
//		}
	}
	
	
	// ================================================================
	//  Input Events
	// ================================================================
	private void RegisterMouseInputs() {
		// Clickz
		if (inputController.IsDoubleClick) { OnMouseDoubleClicked (); }
		if (InputController.IsMouseButtonDown()) { OnMouseDown(); }
		else if (InputController.IsMouseButtonUp()) { OnMouseUp(); }
	}
	private void RegisterKeyInputs() {
        // CONTROL UP or DOWN...
        if (InputController.IsKeyDown_control || InputController.IsKeyUp_control) {
            UpdateIsKeyMultiSelection();
        }

        // CONTROL + ...
        if (InputController.IsKey_control) {
            // CONTROL + A = Select ALL RoomTiles!
            if (Input.GetKeyDown (KeyCode.A)) {
			    SelectAllRoomTiles();
            }
            // CONTROL + D = Duplicate ONE selected room.
            else if (Input.GetKeyDown(KeyCode.D)) {
                DuplicateSelectedRooms();
            }
            // CONTROL + N = Add and open a new room!
            else if (Input.GetKeyDown(KeyCode.N)) {
                AddAndStartNewRoom();
            }
            // CONTROL + U = Toggle tilesSelected isClustStart!
            else if (Input.GetKeyDown(KeyCode.U)) {
                ToggleTilesSelectedIsClustStart();
            }
            // CONTROL + S = Toggle tilesSelected IsSecret!
            else if (Input.GetKeyDown(KeyCode.S)) {
                ToggleTilesSelectedIsSecret();
            }
        }

		// ALT + ____
		else if (InputController.IsKey_alt) {
			// ALT + [number] = Move all RoomTiles selected to that world!!
			if (Input.GetKeyDown (KeyCode.Alpha0)) { MoveRoomTilesSelectedToWorld (0); }
			else if (Input.GetKeyDown (KeyCode.Alpha1)) { MoveRoomTilesSelectedToWorld (1); }
			else if (Input.GetKeyDown (KeyCode.Alpha2)) { MoveRoomTilesSelectedToWorld (2); }
			else if (Input.GetKeyDown (KeyCode.Alpha3)) { MoveRoomTilesSelectedToWorld (3); }
			else if (Input.GetKeyDown (KeyCode.Alpha4)) { MoveRoomTilesSelectedToWorld (4); }
			else if (Input.GetKeyDown (KeyCode.Alpha5)) { MoveRoomTilesSelectedToWorld (5); }
			else if (Input.GetKeyDown (KeyCode.Alpha6)) { MoveRoomTilesSelectedToWorld (6); }
			else if (Input.GetKeyDown (KeyCode.Alpha7)) { MoveRoomTilesSelectedToWorld (7); }
			else if (Input.GetKeyDown (KeyCode.Alpha8)) { MoveRoomTilesSelectedToWorld (8); }
			else if (Input.GetKeyDown (KeyCode.Alpha9)) { MoveRoomTilesSelectedToWorld (9); }
			else if (Input.GetKeyDown (KeyCode.Minus)) { MoveRoomTilesSelectedToWorld (10); }
		}
		
        // NO alt/control...!
        else {
            // DELETE/BACKSPACE = move selected files to this world's trash folder
            if (tilesSelected.Count>0 && !isSearchingRoom && (Input.GetKeyDown (KeyCode.Delete) || Input.GetKeyDown (KeyCode.Backspace))) {
                MoveRoomTilesSelectedRoomFilesToTrashFolder ();
            }
            
            // ENTER = Reload datas and reload scene!
            else if (Input.GetKeyDown(KeyCode.Return)) {
                ReloadAllWorldDatasAndScene();
            }
            
            // BACKSPACE
            else if (Input.GetKeyDown (KeyCode.Backspace)) {//c == "\b"[0]) {
                if (roomSearchString.Length != 0) {
                    roomSearchString = roomSearchString.Substring(0, roomSearchString.Length - 1);
                    UpdateRoomTilesFromSearchString ();
                }
            }
            // ESCAPE = Cancel searching
            if (Input.GetKeyDown(KeyCode.Escape)) {
                ClearRoomSearch();
            }
            // SHIFT + Some typeable character = Search string!
            else if (InputController.IsKey_shift && Input.inputString.Length > 0) {
                char c = Input.inputString[0];
                // Typeable character
                if (char.IsLetterOrDigit(c) || char.IsPunctuation(c)) {
                    roomSearchString += c;
                    UpdateRoomTilesFromSearchString ();
                }
            }
        }
        
        // No ALT/CONTROL/SHIFT...!
        if (!InputController.IsKey_alt && !InputController.IsKey_control && !InputController.IsKey_shift) {
            // R = Print incomplete room links!
            if (Input.GetKeyDown(KeyCode.R)) { Debug_PrintIncompleteRoomLinks(); }
		    // Visibility togglin'
            else if (Input.GetKeyDown(KeyCode.U)) { TogSettings_DoShowClusters(); } // U = toggle RoomCluster visuals
		    else if (Input.GetKeyDown(KeyCode.F)) { TogSettings_DoShowDesignerFlags(); } // F = toggle DesignerFlags
            else if (Input.GetKeyDown(KeyCode.E)) { TogSettings_DoShowEdibles(); } // E = toggle Edibles
            else if (Input.GetKeyDown(KeyCode.N)) { TogSettings_DoShowRoomNames(); } // N = toggle Room names
            else if (Input.GetKeyDown(KeyCode.I)) { TogSettings_DoShowInstructions(); } // I = toggle instructions
            else if (Input.GetKeyDown(KeyCode.P)) { TogSettings_DoShowProps(); } // P = toggle Props
            else if (Input.GetKeyDown(KeyCode.M)) { TogSettings_RoomContentMasks(); } // M = toggle RoomTile contents being masked
		
		    // LOAD DIFFERENT WORLDS
		    else if (Input.GetKeyDown(KeyCode.Alpha0)) { SetCurrWorld(0); }
		    else if (Input.GetKeyDown(KeyCode.Alpha1)) { SetCurrWorld(1); }
		    else if (Input.GetKeyDown(KeyCode.Alpha2)) { SetCurrWorld(2); }
		    else if (Input.GetKeyDown(KeyCode.Alpha3)) { SetCurrWorld(3); }
		    else if (Input.GetKeyDown(KeyCode.Alpha4)) { SetCurrWorld(4); }
		    else if (Input.GetKeyDown(KeyCode.Alpha5)) { SetCurrWorld(5); }
		    else if (Input.GetKeyDown(KeyCode.Alpha6)) { SetCurrWorld(6); }
		    else if (Input.GetKeyDown(KeyCode.Alpha7)) { SetCurrWorld(7); }
		    else if (Input.GetKeyDown(KeyCode.Alpha8)) { SetCurrWorld(8); }
		    else if (Input.GetKeyDown(KeyCode.Alpha9)) { SetCurrWorld(9); }
		    else if (Input.GetKeyDown(KeyCode.Minus)) { SetCurrWorld (10); }
        }
	}
	private void OnMouseDown() {
		int mouseButton = InputController.GetMouseButtonDown();
		// LEFT click?
		if (mouseButton == 0) {
			// Tell ALL selected tiles to update their click offset!
			foreach (RoomTile roomTile in tilesSelected) {
				roomTile.SetMouseClickOffset (MousePosWorld);
			}
            RoomTile tileOver = GetVisibleTileAtPoint(MousePosWorld);
            // YES over a tile??...
            if (tileOver != null) {
                OnClickRoomTile(tileOver);
            }
            else { // Not over ANY tile?? Deselect Tiles and activate selectionRect!
                DeselectTilesSelected();
                selectionRect.Activate();
			}
		}
        // RIGHT click?
        else if (mouseButton == 1) {
            Debug.Log ("mousePosWorld: " + MousePosWorld);
        }
		// Update on-mouse-down vectors!
		MousePosScreenOnDown = MousePosScreen;
	}
	private void OnMouseUp() {
		int mouseButton = InputController.GetMouseButtonUp ();

		// Dragging a TILE(S), released the LEFT mouse button, AND not holding down the multi-selection key??
		if (tilesSelected.Count>0 && mouseButton==0 && !isKeyMultiSelection) {
			// Save all dragging tiles' roomData to file (and clear out any snapshot datas)!
			foreach (RoomTile tile in tilesSelected) {
				RoomSaverLoader.UpdateRoomPropertiesInRoomFile(tile.MyRoomData);
			}
			// Update the world-bounds, room neighbors, etc.!
			CurrWorldData.SetAllRoomDatasFundamentalProperties();
            // Update ALL Tiles' visuals.
			foreach (RoomTile tile in CurrWorldRoomTiles) {
				tile.RefreshColors();
			}

//			// Mouse up = release all roomTilesSelected!
//			DeselectTilesSelected();
		}
	}
	private void OnMouseDoubleClicked() {
		// Am I over any room?? Load it!!
        RoomTile tileOver = GetVisibleTileAtPoint(MousePosWorld);
        if (tileOver != null && tileOver.IsMouseOverMe) {//hacky with IsMouseOverMe. Technically means that mouse is over its COLLIDer.
			SceneHelper.OpenGameplayScene(tileOver.WorldIndex, tileOver.RoomKey);
		}
	}


	
	
	// ================================================================
	//  Destroy
	// ================================================================
	private void DestroyAllRoomTiles() {
		if (allRoomTiles != null) {
			for (int worldIndex=0; worldIndex<allRoomTiles.Count; worldIndex++) {
				for (int i=0; i<allRoomTiles[worldIndex].Count; i++) {
					Destroy (allRoomTiles[worldIndex][i].gameObject);
				}
			}
			allRoomTiles.Clear();
            tilesSelected.Clear(); // also clear out tilesSelected list!
		}
	}



    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Debug
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
        if (UnityEditor.EditorApplication.isPlaying) {
            SceneHelper.ReloadScene();
        }
    }
#endif
    private void Debug_PrintIncompleteRoomLinks() {
        //for (int i=0; i<dataManager.NumWorldDatas; i
        CurrWorldData.Debug_PrintIncompleteRoomLinks();
    }

}
}






