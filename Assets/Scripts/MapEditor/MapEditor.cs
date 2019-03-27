using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapEditor : MonoBehaviour {
	// Constants
	private readonly float gridSizeX = 1;//GameVisualProperties.OriginalScreenSize.x * 0.25f;
	private readonly float gridSizeY = 1;//GameVisualProperties.OriginalScreenSize.y * 0.25f;
	private const string instructionsTextString_enabled = "change world:  [1-8]\ntoggle names:  'n'\ntoggle contents:  'p'\nsearch:  [hold 'SHIFT' and type lvl name; ESC to cancel]\nhide instructions:  'i'";//move/zoom:  [mouse or arrow keys]\nplay level:  [double-click one]\n
	private const string instructionsTextString_disabled = "show instructions:  'i'";
	// Components
	[SerializeField] private LevelTileSelectionRect selectionRect=null;
	private List<GameObject> worldLayerGOs; // purely for hierarchy cleanness, we wanna put all the tiles into their respective world's GameObject.
	private List<List<LevelTile>> allLevelTiles; // LevelTiles for EVERY LEVEL IN THE GAME!
	// Properties
	private bool isSearchingLevel; // When we start typing letters, yeah! Narrow down our options.
	private int currWorldIndex=-1;
	private string levelSearchString = "";
    public MapEditorSettings MySettings { get; private set; }
	public Vector2 MousePosScreenOnDown { get; private set; }
	public Vector2 MousePosWorld { get; private set; }
	// References
	[SerializeField] private Text currentWorldText=null;
	[SerializeField] private Text demoText;
	[SerializeField] private Text instructionsText=null;
	private List<LevelTile> tilesSelected = new List<LevelTile>();
    private MapEditorCamera editorCamera;
	public WorldData CurrentWorldData { get { return GetWorldData(currWorldIndex); } }
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
	private List<LevelTile> CurrWorldLevelTiles { get { return allLevelTiles==null?null : allLevelTiles [currWorldIndex]; } }

	public bool CanSelectALevelTile() {
		// Otherwise, NO tiles selected and our mouse isn't down? Yeah, return true!
		if (tilesSelected.Count==0 && !Input.GetMouseButton(0)) { return true; }
		// Okay, so some might be selected? Can we select multiple ones??
		if (CanSelectMultipleTiles()) { return true; }
		// Hmm. No, there's no way we can select a LevelTile right now.
		return false;
	}
	/** We may only move LevelTiles while running in the UNITY EDITOR. Don't allow moving tiles in a BUILD version. */
	private bool MayMoveLevelTiles () {
		bool mayMove = false;
		#if UNITY_EDITOR
		mayMove = true;
		#endif
		return mayMove;
	}
	private bool CanSelectMultipleTiles() {
		// Is the right key down?
		return Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift);
	}
	public bool IsDraggingSelectedLevelTiles() {
		return MayMoveLevelTiles() && !CanSelectMultipleTiles () && Input.GetMouseButton (0);
	}
	private LevelTile GetLevelTileByKey (string levelKey) {
		for (int i=0; i<CurrWorldLevelTiles.Count; i++) {
			if (CurrWorldLevelTiles[i].LevelKey == levelKey) {
				return CurrWorldLevelTiles[i];
			}
		}
		return null;
	}
	private Vector2 GetConnectionPosRelativeToLevelTile(LevelTile levelTile, Vector2 globalPos) {
		return new Vector2(globalPos.x-levelTile.MyLevelData.PosGlobal.x, globalPos.y-levelTile.MyLevelData.PosGlobal.y);
    }

    public float MapScale { get { return editorCamera.MapScale; } }
    private Vector2 SnapToGrid(Vector2 v) { return new Vector2(Mathf.Floor(v.x/gridSizeX)*gridSizeX, Mathf.Floor(v.y/gridSizeY)*gridSizeY); }
	public Vector2 MousePosScreen { get { return inputController.MousePosScreen; } }
    private Vector2 MousePosWorldDragging(Vector2 _mouseClickOffset) {
		return MousePosWorld + _mouseClickOffset;
	}
	public Vector2 MousePosWorldDraggingGrid(Vector2 _mouseClickOffset) { // Return the mouse position, scaled to the screen and snapped to the grid.
        bool doSnap = !Input.GetKey (KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl); // Hold down CONTROL to prevent snapping.
        if (doSnap) {
            return MousePosWorldDragging(SnapToGrid(_mouseClickOffset));
        }
        else {
            return MousePosWorldDragging(_mouseClickOffset);
        }
    }
	private LevelTile GetLevelTileAtPoint(Vector2 point) {
		if (CurrWorldLevelTiles==null) { return null; } // Safety check for runtime compile.
		for (int i=CurrWorldLevelTiles.Count-1; i>=0; --i) { // loop thru backwards so we click NEWER tiles before older ones.
            LevelTile tile = CurrWorldLevelTiles[i];
			Rect boundsGlobal = tile.BoundsGlobal;
			if (boundsGlobal.Contains(point+boundsGlobal.size*0.5f)) { // Note: convert back to center-aligned. SIGH. I would love to just make level's rect corner-aligned. Avoid any ambiguity.
				return tile;
			}
		}
		return null;
	}
	private bool IsMouseOverAnything() {
		/*
		// LevelTiles?
		for (int i=0; i<levelTiles.Count; i++) {
			if (levelTiles[i].IsDragReadyMouseOverMe) {
				return true;
			}
		}
		return false;
		*/
		return GetLevelTileAtPoint (MousePosWorld) != null;
	}


	private void OnDrawGizmos () {
		// World Bounds
		if (CurrentWorldData != null) {
			Gizmos.color = new Color (0.1f, 0.1f, 0.1f);
			Rect boundsRectAllLevels = CurrentWorldData.BoundsRectAllLevels;
			Gizmos.DrawWireCube (new Vector3 (boundsRectAllLevels.center.x, boundsRectAllLevels.center.y, 0), new Vector3 (boundsRectAllLevels.size.x, boundsRectAllLevels.size.y, 10));
		}
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
		
		// Reload everything right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
		#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
		#endif

//		// Set the whole map's scale to be the same as how the game's scale works.
//		this.transform.localScale = new Vector3 (GameProperties.WORLD_SCALE, GameProperties.WORLD_SCALE, 1);

		// Make the worldLayerGOs and LevelTiles only once.
		MakeWorldLayerGOs();
        LoadAllLevelTiles();

        // Reset values
        Time.timeScale = 1f;
		Cursor.visible = true;

		// Set current world
		SetCurrWorld(SaveStorage.GetInt(SaveKeys.LastPlayedWorldIndex));
	}

	private void SetCurrWorld (int _worldIndex) {
		if (_worldIndex >= GameProperties.NUM_WORLDS) { return; } // Don't crash da game, bruddah.

		// Deselect any tiles that might be selected!
		DeselectAllLevelTiles ();

		// If we're CHANGING the currentWorld...!!
		if (currWorldIndex != _worldIndex) {
			// Tell all the tiles in the world we already were to hide their stuff!
			if (currWorldIndex != -1) {
				for (int i=0; i<CurrWorldLevelTiles.Count; i++) {
					CurrWorldLevelTiles[i].HideContents ();
				}
			}
			currWorldIndex = _worldIndex;
            SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, currWorldIndex);
        }
		
		// Tell all the tiles in the NEW world to show their stuff!
		for (int i=0; i<CurrWorldLevelTiles.Count; i++) {
			CurrWorldLevelTiles[i].ShowContents ();
		}
		
		// Set background colla
		editorCamera.OnSetCurrentWorld(currWorldIndex);
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
	private void LoadAllLevelTiles() {
		// Destroy 'em first!
		DestroyAllLevelTiles ();
		// Make 'em hot & fresh!
		allLevelTiles = new List<List<LevelTile>>();
		GameObject prefab = ResourcesHandler.Instance.MapEditor_LevelTile;
		// For every world...
		for (int worldIndex=0; worldIndex<dataManager.NumWorldDatas; worldIndex++) {
			allLevelTiles.Add (new List<LevelTile>());
			// For every level in this world...
			WorldData wd = dataManager.GetWorldData (worldIndex);
            Transform parent = worldLayerGOs[worldIndex].transform;
            foreach (LevelData levelData in wd.LevelDatas.Values) {
                LevelTile newLevelTile = Instantiate (prefab).GetComponent<LevelTile>();
				newLevelTile.Initialize(this, levelData, parent);
				allLevelTiles[worldIndex].Add(newLevelTile);
			}
		}
	}

	private void ReloadAllWorldDatasAndScene() {
        dataManager.ReloadWorldDatas();
        SceneHelper.ReloadScene();
    }




    // ================================================================
    //  Doers: Level Tiles
    // ================================================================
	private void SelectLevelTile(LevelTile thisLevelTile) {
		/*
		// First, check if we're trying to set ANOTHER, different levelTile as levelTileDragging.
		if (levelTileDragging != null && thisLevelTile != null && levelTileDragging != thisLevelTile) {
			Debug.LogWarning("Hey, whoa! We're trying to set levelTileDragging to a LevelTile, but it's not already null!");
			return;
		}
		// Was dragging a LevelTile? Update its visuals!
		if (levelTileDragging != null) {
			levelTileDragging.UpdateBorderLine();
		}
		// Set it!
		levelTileDragging = thisLevelTile;
		// Update its visuals!
		if (levelTileDragging != null) {
			levelTileDragging.UpdateBorderLine();
		}
		// We DID specifiy a levelTile?
		if (levelTileDragging != null) {
			SetMouseClickOffset(levelTileDragging.transform.localPosition);
//			SetMouseClickOffset(levelTileDragging.Pos);//new PVector(levelTileDragging.pos.x+levelTileDragging.w*0.5, levelTileDragging.pos.y+levelTileDragging.h*0.5));
		}
		*/
		// Add it to my list!
		tilesSelected.Add (thisLevelTile);
		// Tell it what's up!
		thisLevelTile.OnSelected (MousePosWorld);
	}
	private void DeselectLevelTile(LevelTile thisLevelTile) {
		thisLevelTile.OnDeselected ();
		tilesSelected.Remove (thisLevelTile);
	}
	private void DeselectAllLevelTiles () {
		for (int i=tilesSelected.Count-1; i>=0; --i) {
			DeselectLevelTile (tilesSelected [i]);
		}
		tilesSelected = new List<LevelTile> ();
	}
	private void ReleaseLevelTilesSelected() {
		// No tiles selected? No gazpacho!
		if (tilesSelected.Count == 0) { return; }
		// Tell all the tiles they've been deselected and clear out the list.
		foreach (LevelTile levelTile in tilesSelected) {
			levelTile.OnDeselected ();
		}
		// Clear out the list, of course.
		tilesSelected.Clear ();
		// Brute-force remake all the wallLines for EVERY LevelTile. It's dumb, but easy.
//		CurrentWorldData.SetAllLevelDatasBounds ();

//		foreach (LevelTile levelTile in CurrentWorldLevelTiles) {
//			levelTile.RemakeWallLines ();
//		}
	}
	private void SelectAllLevelTiles() {
		ReleaseLevelTilesSelected (); // Just in case release any if we're already holding some.
		foreach (LevelTile levelTile in CurrWorldLevelTiles) {
			SelectLevelTile(levelTile);
		}
	}
    private void DuplicateFirstSelectedLevel() {
        // No tiles selected? No gazpacho!
        if (tilesSelected.Count == 0) { return; }
        DuplicateLevel(tilesSelected[0].MyLevelData);
    }
    private void DuplicateLevel(LevelData originalData) {
        // Add a new level file, yo!
        string newLevelKey = originalData.levelKey + " copy";
        LevelSaverLoader.SaveLevelFileAs(originalData, originalData.worldIndex, newLevelKey);
        // Reload everything.
        ReloadAllWorldDatasAndScene();
    }

	// TODO: This! Convert to StreamingAssets.
	private void MoveLevelTilesSelectedLevelFilesToTrashFolder() {
		// No tiles selected? Womp, don't do anything LOL
		if (tilesSelected.Count == 0) { return; }
		// Move the files!!
		for (int i=tilesSelected.Count-1; i>=0; --i) {
//			DeselectLevelTile (levelTilesSelected[i]);
			CurrentWorldData.MoveLevelFileToTrashFolder (tilesSelected[i].LevelKey);
		}
		
		// Reload everything right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
		#if UNITY_EDITOR
//		UnityEditor.AssetDatabase.ImportAsset (saveLocation + fileName);
		UnityEditor.AssetDatabase.Refresh ();
		#endif

		// Reload this map, yo.
		ReloadAllWorldDatasAndScene();
	}

	private void MoveLevelTilesSelectedToWorld (int worldIndexTo) {
		// No tiles selected? Womp, don't do anything LOL
		if (tilesSelected.Count == 0) { return; }
		int worldIndexFrom = tilesSelected[0].WorldIndex; // We can assume all levelTilesSelected are in the same world.
		//WorldData worldDataTo = GetWorldData (worldIndexTo);
		//WorldData worldDataFrom = GetWorldData (worldIndexFrom);
//		string worldToFolderName = GameProperties.GetWorldName (worldIndexTo); // name of the folda we're moving the files TO.
		// If we're trying to move these tiles to the world they're ALREADY in, do nothin'!
		if (worldIndexFrom == worldIndexTo) { return; }
		// Compile a list of the DATAS of all the levels we're moving.
		LevelData[] levelDatasMoving = new LevelData [tilesSelected.Count];
		for (int i=0; i<levelDatasMoving.Length; i++) {
			levelDatasMoving[i] = tilesSelected[i].MyLevelData;
		}
		// MOVE LEVELS!
		for (int i=0; i<levelDatasMoving.Length; i++) {
			string levelKey = levelDatasMoving[i].LevelKey;
			// Otherwise, move that glitterbomb!
//			DeselectLevelTile (levelTilesSelected[i]);
			CurrentWorldData.MoveLevelFileToWorldFolder (levelKey, worldIndexTo);
		}
		
		// Reload this map, yo.
		ReloadAllWorldDatasAndScene();
	}

	
	
	// ================================================================
	//  Events
	// ================================================================
    public void OnSetMapScale() {
        // Update my level tiles' text scales!
        if (allLevelTiles != null) {
            foreach (LevelTile levelTile in CurrWorldLevelTiles) {
                levelTile.OnMapScaleChanged();
            }
        }
    }
    public void OnClickLevelTile(LevelTile levelTile) {
		// Conditions are right for selecting the tile!
//		if (!Input.GetKey(KeyCode.LeftAlt) && newLinkFirstLevelTile==null) {
		if (!Input.GetKey(KeyCode.LeftAlt)) {
			// If the COMMAND/CONTROL key ISN'T down, first release all levelTilesSelected!
			if (!CanSelectMultipleTiles()) {
				ReleaseLevelTilesSelected();
			}
			// If this guy is IN the list, remove him; if he's NOT in the list, add him!
			if (tilesSelected.Contains(levelTile)) { DeselectLevelTile(levelTile); }
			else { SelectLevelTile(levelTile); }
		}
	}

	private void UpdateComponentVisibilities() {
		for (int i=0; i<CurrWorldLevelTiles.Count; i++) { CurrWorldLevelTiles[i].UpdateComponentVisibilities(); }
//		for (int i=0; i<levelLinkViews.Count; i++) { levelLinkViews[i].UpdateComponentVisibilities(); }
	}
    private void ToggleLevelContentsMasked() {
        MySettings.DoMaskLevelContents = !MySettings.DoMaskLevelContents;
        OnChangeSettings();
    }
    private void ToggleLevelTileDesignerFlagsVisibility() {
        MySettings.DoShowDesignerFlags = !MySettings.DoShowDesignerFlags;
        OnChangeSettings();
    }
    private void ToggleLevelTileNamesVisibility() {
        MySettings.DoShowLevelNames = !MySettings.DoShowLevelNames;
        OnChangeSettings();
    }
    private void ToggleLevelTileStarsVisibility() {
        MySettings.DoShowLevelTileStars = !MySettings.DoShowLevelTileStars;
        OnChangeSettings();
    }
    private void ToggleLevelPropsVisibility() {
        MySettings.DoShowLevelProps = !MySettings.DoShowLevelProps;
        OnChangeSettings();
    }
    private void ToggleInstructionsVisibility() {
        MySettings.DoShowInstructions = !MySettings.DoShowInstructions;
        OnChangeSettings();
    }
    private void OnChangeSettings() {
        UpdateComponentVisibilities();
        MySettings.SaveAll();
    }

    public void OnLevelTileSelectionRectDeactivated () {
		if (CurrWorldLevelTiles==null) { return; } // Safety check for runtime compile.
		// Select all the extra ones selected by the selection rect!
		for (int i=0; i<CurrWorldLevelTiles.Count; i++) {
			if (CurrWorldLevelTiles[i].IsWithinLevelTileSelectionRect) {
				if (!tilesSelected.Contains(CurrWorldLevelTiles[i])) {
					SelectLevelTile (CurrWorldLevelTiles[i]);
				}
			}
		}
	}

    private void ClearLevelSearch() {
        isSearchingLevel = false;
        levelSearchString = "";
        UpdateLevelTilesFromSearchString();
    }
    private void UpdateLevelTilesFromSearchString () {
		// Update their visibilities!
		for (int i=0; i<CurrWorldLevelTiles.Count; i++) {
			CurrWorldLevelTiles[i].UpdateVisibilityFromSearchCriteria (levelSearchString);
		}
	}

	
	
	
	
	// ================================================================
	//  Update
	// ================================================================
	private void Update() {
		if (inputController==null) { return; } // Safety check for runtime compile.

		UpdateMousePosWorld();
		RegisterKeyInputs();
		RegisterMouseInputs();
		UpdateLevelTileSelectionRectSelection();
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

	private void UpdateLevelTileSelectionRectSelection() {
		if (CurrWorldLevelTiles==null) { return; } // Safety check for runtime compile.
		// Update which tiles are within the rect!
		for (int i=0; i<CurrWorldLevelTiles.Count; i++) {
			CurrWorldLevelTiles[i].IsWithinLevelTileSelectionRect = selectionRect.IsActive && selectionRect.SelectionRect.Contains(CurrWorldLevelTiles[i].MyLevelData.PosGlobal);
		}
//		if (levelTileSelectionRect.IsActive) {
//			levelTileSelectionRect.levelTilesSelected
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
        bool isKeyDown_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool isKeyDown_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		// DELETE/BACKSPACE = move selected files to this world's trash folder
		if (tilesSelected.Count>0 && !isSearchingLevel && (Input.GetKeyDown (KeyCode.Delete) || Input.GetKeyDown (KeyCode.Backspace))) {
			MoveLevelTilesSelectedLevelFilesToTrashFolder ();
		}
        
        // ENTER = Reload datas and reload scene!
        else if (Input.GetKeyDown(KeyCode.Return)) {
            ReloadAllWorldDatasAndScene();
        }
		
		// Backspace
		else if (Input.GetKeyDown (KeyCode.Backspace)) {//c == "\b"[0]) {
			if (levelSearchString.Length != 0) {
				levelSearchString = levelSearchString.Substring(0, levelSearchString.Length - 1);
				UpdateLevelTilesFromSearchString ();
			}
		}
		// SHIFT + Some typeable character = Search string!
		else if (isKeyDown_shift && Input.inputString.Length > 0) {
			char c = Input.inputString[0];
			// Typeable character
			if (char.IsLetterOrDigit(c) || char.IsPunctuation(c)) {
				levelSearchString += c;
				UpdateLevelTilesFromSearchString ();
			}
		}

        // CONTROL + ...
        if (isKeyDown_control) {// TODO: Test these
            // CONTROL + A = Select ALL LevelTiles!
            if (Input.GetKeyDown (KeyCode.A)) {
			    SelectAllLevelTiles ();
            }
            // CONTROL + D = Duplicate ONE selected level.
            else if (Input.GetKeyDown(KeyCode.D)) {
                DuplicateFirstSelectedLevel();
            }
            // CONTROL + J = Open LevelJump!
            else if (Input.GetKeyDown (KeyCode.J)) {
                SceneHelper.OpenScene(SceneNames.LevelJump);
            }
		}

		// CONTROL/ALT + ____
		else if (Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightAlt) || Input.GetKey (KeyCode.RightControl)) {
			// CONTROL + [number] = Move all LevelTiles selected to that world!!
			if (Input.GetKeyDown (KeyCode.Alpha0)) { MoveLevelTilesSelectedToWorld (0); }
			else if (Input.GetKeyDown (KeyCode.Alpha1)) { MoveLevelTilesSelectedToWorld (1); }
			else if (Input.GetKeyDown (KeyCode.Alpha2)) { MoveLevelTilesSelectedToWorld (2); }
			else if (Input.GetKeyDown (KeyCode.Alpha3)) { MoveLevelTilesSelectedToWorld (3); }
			else if (Input.GetKeyDown (KeyCode.Alpha4)) { MoveLevelTilesSelectedToWorld (4); }
			else if (Input.GetKeyDown (KeyCode.Alpha5)) { MoveLevelTilesSelectedToWorld (5); }
			else if (Input.GetKeyDown (KeyCode.Alpha6)) { MoveLevelTilesSelectedToWorld (6); }
			else if (Input.GetKeyDown (KeyCode.Alpha7)) { MoveLevelTilesSelectedToWorld (7); }
			else if (Input.GetKeyDown (KeyCode.Alpha8)) { MoveLevelTilesSelectedToWorld (8); }
			else if (Input.GetKeyDown (KeyCode.Alpha9)) { MoveLevelTilesSelectedToWorld (9); }
			else if (Input.GetKeyDown (KeyCode.Minus)) { MoveLevelTilesSelectedToWorld (10); }
		}
		
		// Visibility togglin'
		else if (Input.GetKeyDown(KeyCode.F)) { ToggleLevelTileDesignerFlagsVisibility(); } // F = toggle flags
		else if (Input.GetKeyDown(KeyCode.N)) { ToggleLevelTileNamesVisibility(); } // N = toggle names
		else if (Input.GetKeyDown(KeyCode.P)) { ToggleLevelPropsVisibility(); } // P = toggle props
		else if (Input.GetKeyDown(KeyCode.M)) { ToggleLevelContentsMasked(); } // M = toggle levelTile contents being masked
		else if (Input.GetKeyDown(KeyCode.T)) { ToggleLevelTileStarsVisibility(); } // T = toggle stars
		else if (Input.GetKeyDown(KeyCode.I)) { ToggleInstructionsVisibility(); } // I = toggle instructions
		
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

		// ESCAPE = Cancel searching
		else if (Input.GetKeyDown (KeyCode.Escape)) {
			ClearLevelSearch ();
		}

	}
	
	
	private void OnMouseDown() {
		int mouseButton = InputController.GetMouseButtonDown();
		// LEFT click?
		if (mouseButton == 0) {
			Debug.Log ("mousePosWorld: " + MousePosWorld);
			// Double-click?!
//			Debug.Log(Time.frameCount + " timeSinceMouseButtonDown: " + timeSinceMouseButtonDown + "     " + Vector2.Distance (MousePosScreen, mousePosScreenOnDown));
//			if (timeSinceMouseButtonDown < DOUBLE_CLICK_TIME && Vector2.Distance (MousePosScreen, mousePosScreenOnDown) < 4) {
			// Tell ALL selected tiles to update their click offset!
			foreach (LevelTile levelTile in tilesSelected) {
				levelTile.SetMouseClickOffset (MousePosWorld);
			}
			// Are we NOT over anything? Activate the selectionRect AND release any selected tiles!
			if (!IsMouseOverAnything ()) {
				selectionRect.Activate ();
				ReleaseLevelTilesSelected();
			}
		}
		// Update on-mouse-down vectors!
		MousePosScreenOnDown = MousePosScreen;
	}
	private void OnMouseUp() {
		int mouseButton = InputController.GetMouseButtonUp ();

		// Dragging a LEVEL TILE(S), released the LEFT mouse button, AND not holding down the multi-selection key??
		if (tilesSelected.Count>0 && mouseButton==0 && !CanSelectMultipleTiles()) {
			// Save all dragging tiles' levelData to file (and clear out any snapshot datas)!
			foreach (LevelTile tile in tilesSelected) {
				LevelSaverLoader.UpdateLevelPropertiesInLevelFile(tile.MyLevelData);
			}
			// Update the worlds' bounds!
			CurrentWorldData.SetAllLevelDatasFundamentalProperties ();
//			// Mouse up = release all levelTilesSelected!
//			ReleaseLevelTilesSelected();
		}
	}
	private void OnMouseDoubleClicked() {
		// Am I over any level?? Load it!!
		for (int i=CurrWorldLevelTiles.Count-1; i>=0; --i) {
			LevelTile tile = CurrWorldLevelTiles[i];
			if (tile.IsDragReadyMouseOverMe) {
				OpenGameplayScene(tile.WorldIndex, tile.LevelKey);
				break;
			}
		}
	}

	private void OpenGameplayScene(int worldIndex, string levelKey) {
//		GameplaySnapshotController.SetWorldAndLevelToLoad (worldIndex, levelKey);
		dataManager.currentLevelData = dataManager.GetLevelData(worldIndex, levelKey, true);
		SceneHelper.OpenScene(SceneNames.Gameplay);
	}


	
	
	// ================================================================
	//  Destroy
	// ================================================================
	private void DestroyAllLevelTiles() {
		if (allLevelTiles != null) {
			for (int worldIndex=0; worldIndex<allLevelTiles.Count; worldIndex++) {
				for (int i=0; i<allLevelTiles[worldIndex].Count; i++) {
					Destroy (allLevelTiles[worldIndex][i].gameObject);
				}
			}
			allLevelTiles.Clear();
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

}






