using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapEditor : MonoBehaviour {
	// Constants
	private const float ARROW_KEYS_PAN_SPEED = 20f; // for panning the map with the arrow keys. Scales based on mapScale.
	private const float DRAG_PANNING_SPEED = 5f; // higher is faster.
	private const float MOUSE_SCROLL_ZOOM_RATIO = 0.02f; // mouseScrollDelta * this = the % zoom change
	private readonly float gridSizeX = 1;//GameVisualProperties.OriginalScreenSize.x * 0.25f;
	private readonly float gridSizeY = 1;//GameVisualProperties.OriginalScreenSize.y * 0.25f;
	private const float mapScaleNeutral = 0.003f; // when the map is at its neutral zoom level. (If this were 1, then one LevelTile would occupy the whole screen.)
	private const float ZOOM_SPEED_KEYBOARD = 0.04f; // higher is faster.
	private const string instructionsTextString_enabled = "move/zoom:  [mouse or arrow keys]\nchange world:  [1-8]\nplay level:  [double-click one]\ntoggle names:  'n'\ntoggle streets:  's'\nsearch:  [hold 'SHIFT' and type lvl name; ESC to cancel]\nhide instructions:  'i'";
	private const string instructionsTextString_disabled = "show instructions:  'i'";
	// Components
	[SerializeField] private LevelTileSelectionRect selectionRect=null;
	private List<GameObject> worldLayerGOs; // purely for hierarchy cleanness, we wanna put all the tiles into their respective world's GameObject.
	private List<List<LevelTile>> allLevelTiles; // LevelTiles for EVERY LEVEL IN THE GAME!
//	private List<LevelLinkView> levelLinkViews;
	// Properties
	private bool isDragPanning; // true when we right-mouse-click and drag to fast-pan around the map.
	private bool isGrabPanning; // true when we middle-mouse-click to pan around the map. This is the limited scroll.
//	private bool isRemakingAllLevelLinkViews; // so we know when to update LevelTiles when a link is added. (Don't update 'em if we're loading up all the links afresh.)
	private bool isSearchingLevel; // When we start typing letters, yeah! Narrow down our options.
//	private bool willDeleteLevelLinkViewSelected;
	private float mapScale;
	private int currentWorldIndex=-1;
	private string levelSearchString = "";
	private Vector3 cameraPosOnMouseDown;
	private Vector2 mousePosScreenOnDown;
	private Vector2 mousePosWorld;
	// References
	[SerializeField] private Text currentWorldText=null;
	[SerializeField] private Text demoText;
	[SerializeField] private Text instructionsText=null;
	private GameObject levelTilePrefab;
	private List<LevelTile> tilesSelected;
//	private LevelTile connectionCircleLevelTileOverA; // when dragging a LevelLinkView, what LevelTile its connection circle is over.
//	private LevelTile connectionCircleLevelTileOverB; // when dragging a LevelLinkView, what LevelTile its connection circle is over.
	private MapEditorCamera editorCamera;
	private WorldData CurrentWorldData { get { return GetWorldData (currentWorldIndex); } }
	private WorldData GetWorldData (int worldIndex) {
		if (worldIndex<0 || dataManager.NumWorldDatas == 0) { return null; }
		return dataManager.GetWorldData (worldIndex);
	}
//	private LevelLinkView levelLinkViewSelected;
//	private LevelTile newLinkFirstLevelTile; // when I make a new link, this is the FIRST dude in the link!
	// Properties
//	private Vector2 newLinkFirstConnectionPos; // when I make a new link, this is the LOCATION of the first connection in the link!
	
	
	// ================================================================
	//  Getters
	// ================================================================
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private InputController inputController { get { return InputController.Instance; } }
	private float fTS { get { return TimeController.FrameTimeScaleUnscaled; } } // frame time scale
	private List<LevelTile> CurrentWorldLevelTiles { get { return allLevelTiles==null?null : allLevelTiles [currentWorldIndex]; } }

	public float MapScale { get { return mapScale; } }
	public bool CanSelectALevelTile() {
//		// levelLinkViewSelected ISN'T null?! Return false automatically.
//		if (levelLinkViewSelected != null) { return false; }
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
//	public bool WillDeleteLevelLinkViewSelected { get { return willDeleteLevelLinkViewSelected; } }
//	public LevelLinkView LevelLinkViewSelected { get { return levelLinkViewSelected; } }
//	public Vector3 MousePosWorld() { // Return the mouse position, scaled to the screen.
//		Vector3 mousePos = Input.mousePosition / GameVisualProperties.ScreenScale - new Vector3(GameVisualProperties.OriginalScreenSize.x,GameVisualProperties.OriginalScreenSize.y,0)*0.5f;
//		mousePos /= GameProperties.WORLD_SCALE * 2; // NOTE: Idk why *2 works. :P
//		mousePos /= mapScale/mapScaleNeutral;
//		mousePos += editorCamera.transform.localPosition;
//		return mousePos;
//	}
	private LevelTile GetLevelTileByKey (string levelKey) {
		for (int i=0; i<CurrentWorldLevelTiles.Count; i++) {
			if (CurrentWorldLevelTiles[i].LevelKey == levelKey) {
				return CurrentWorldLevelTiles[i];
			}
		}
		return null;
	}
	private Vector2 GetConnectionPosRelativeToLevelTile(LevelTile levelTile, Vector2 globalPos) {
		return new Vector2(globalPos.x-levelTile.LevelDataRef.PosGlobal.x, globalPos.y-levelTile.LevelDataRef.PosGlobal.y);
	}
	public Vector2 MousePosScreen { get { return inputController.MousePosScreen; } }
	public Vector2 MousePosWorld { get { return mousePosWorld; } }
	public Vector2 MousePosWorldDragging(Vector2 _mouseClickOffset) {
		return mousePosWorld + _mouseClickOffset;
	}
	public Vector2 MousePosWorldDraggingGrid(Vector2 _mouseClickOffset) { // Return the mouse position, scaled to the screen and snapped to the grid.
		Vector2 mousePos = MousePosWorldDragging(_mouseClickOffset);
		if (!Input.GetKey (KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) { // Hold down CONTROL to prevent snapping.
			mousePos = new Vector2(Mathf.Round(mousePos.x/gridSizeX)*gridSizeX, Mathf.Round(mousePos.y/gridSizeY)*gridSizeY);
		}
		return mousePos;
	}
	private LevelTile GetLevelTileAtPoint(Vector2 point) {
		if (CurrentWorldLevelTiles==null) { return null; } // Safety check for runtime compile.
		foreach (LevelTile tile in CurrentWorldLevelTiles) {
			Rect boundsGlobal = tile.BoundsGlobal;
			if (boundsGlobal.Contains(point+boundsGlobal.size*0.5f)) { // Note: convert back to center-aligned. SIGH. I would love to just make level's rect corner-aligned. Avoid any ambiguity.
				return tile;
			}
		}
		return null;
	}
	/*
	// Note: The following get/set code for connectionCircles is bulky and ugly. Slimming it down into something pretty and easier to read isn't worth my time, though.
	private LevelTile ConnectionCircleLevelTileOverA {
		get { return connectionCircleLevelTileOverA; }
		set {
			if (connectionCircleLevelTileOverA != value) {
				if (connectionCircleLevelTileOverA != null) { // Is the current one NOT null?? Update it!
					if (levelLinkViewSelected == null) { connectionCircleLevelTileOverA.IsLevelLinkViewSelectedOverMePrimary = connectionCircleLevelTileOverA.IsLevelLinkViewSelectedOverMeSecondary = false; }
					else if (levelLinkViewSelected.ConnectionCircleA.IsDraggingMe) { connectionCircleLevelTileOverA.IsLevelLinkViewSelectedOverMePrimary = false; }
					else { connectionCircleLevelTileOverA.IsLevelLinkViewSelectedOverMeSecondary = false; }
				}
				connectionCircleLevelTileOverA = value;
				if (connectionCircleLevelTileOverA != null) { // Is the new one NOT null?? Update it!
					if (levelLinkViewSelected.ConnectionCircleA.IsDraggingMe) { connectionCircleLevelTileOverA.IsLevelLinkViewSelectedOverMePrimary = true; }
					else { connectionCircleLevelTileOverA.IsLevelLinkViewSelectedOverMeSecondary = true; }
				}
			}
		}
	}
	private LevelTile ConnectionCircleLevelTileOverB {
		get { return connectionCircleLevelTileOverB; }
		set {
			if (connectionCircleLevelTileOverB != value) {
				if (connectionCircleLevelTileOverB != null) { // Is the current one NOT null?? Update it!
					if (levelLinkViewSelected == null) { connectionCircleLevelTileOverB.IsLevelLinkViewSelectedOverMePrimary = connectionCircleLevelTileOverB.IsLevelLinkViewSelectedOverMeSecondary = false; }
					else if (levelLinkViewSelected.ConnectionCircleB.IsDraggingMe) { connectionCircleLevelTileOverB.IsLevelLinkViewSelectedOverMePrimary = false; }
					else { connectionCircleLevelTileOverB.IsLevelLinkViewSelectedOverMeSecondary = false; }
				}
				connectionCircleLevelTileOverB = value;
				if (connectionCircleLevelTileOverB != null) { // Is the new one NOT null?? Update it!
					if (levelLinkViewSelected.ConnectionCircleB.IsDraggingMe) { connectionCircleLevelTileOverB.IsLevelLinkViewSelectedOverMePrimary = true; }
					else { connectionCircleLevelTileOverB.IsLevelLinkViewSelectedOverMeSecondary = true; }
				}
			}
		}
	}
//	private bool WillDeleteLevelLinkViewSelected() {
//		// No view actually selected? Umm, return false, I guess!
//		if (levelLinkViewSelected == null) { return false; }
//		// DO plan to delete levelLinkViewSelected if A) EITHER of its ends are NOT over a LevelTile, or B) BOTH its ends are over the SAME LevelTile!
//		return connectionCircleLevelTileOverA==null
//			|| connectionCircleLevelTileOverB==null
//			|| connectionCircleLevelTileOverA==connectionCircleLevelTileOverB;
//	}
	*/
	private bool IsMouseOverAnything() {
		/*
		// ConnectionCircles?
		if (connectionCircleLevelTileOverA != null || connectionCircleLevelTileOverB != null) {
			return true;
		}
		// LevelTiles?
		for (int i=0; i<levelTiles.Count; i++) {
			if (levelTiles[i].IsDragReadyMouseOverMe) {
				return true;
			}
		}
		return false;
		*/
		return GetLevelTileAtPoint (mousePosWorld) != null;
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
		editorCamera = GameObject.FindObjectOfType<MapEditorCamera>();
		levelTilePrefab = ResourcesHandler.Instance.MapEditor_LevelTile;//(GameObject)Resources.Load (GameProperties.MAP_EDITOR_PREFABS_PATH + "LevelLinkView");

		// Load settings
		MapEditorSettings.LoadAll();

//		// Set the whole map's scale to be the same as how the game's scale works.
//		this.transform.localScale = new Vector3 (GameProperties.WORLD_SCALE, GameProperties.WORLD_SCALE, 1);

		// Make the worldLayerGOs only once.
		MakeWorldLayerGOs ();
		
		// Reset values
		Time.timeScale = 1f;
		Cursor.visible = true;
		tilesSelected = new List<LevelTile> ();

		// Set current world
		ResetMap (dataManager.currentWorldIndex);

		// Move the camera to where we remember it was last time we was here!
		editorCamera.transform.localPosition = new Vector3 (SaveStorage.GetFloat (SaveKeys.MapEditor_CameraPosX, 0), SaveStorage.GetFloat (SaveKeys.MapEditor_CameraPosY, 0), editorCamera.transform.localPosition.z);
		float savedMapScale = SaveStorage.GetFloat (SaveKeys.MapEditor_MapScale, mapScaleNeutral);
		if (savedMapScale != 0) {
			SetMapScale (savedMapScale);
		}
	}

	private void SetCurrentWorld (int _currentWorldIndex) {
		if (_currentWorldIndex >= GameProperties.NUM_WORLDS) { return; } // Don't crash da game, bruddah.

		// Deselect any tiles that might be selected!
		DeselectAllLevelTiles ();

		// If we're CHANGING the currentWorld...!!
		if (currentWorldIndex != _currentWorldIndex) {
			// Tell all the tiles in the world we already were to hide their stuff!
			if (currentWorldIndex != -1) {
				for (int i=0; i<CurrentWorldLevelTiles.Count; i++) {
					CurrentWorldLevelTiles[i].HideContents ();
				}
			}
			currentWorldIndex = _currentWorldIndex;
		}

//		// Reload my linx!
//		ReloadCurrentLevelLinkViews ();
		
		// Tell all the tiles in the NEW world to show their stuff!
		for (int i=0; i<CurrentWorldLevelTiles.Count; i++) {
			CurrentWorldLevelTiles[i].ShowContents ();
		}
		
		// Set background colla
		editorCamera.SetBackgroundColor (currentWorldIndex);
	}

	private void ResetMap (int _worldIndex) {
		// Load up everything!
		LoadAllLevelTiles ();
		SetCurrentWorld (_worldIndex);
	}
	private void ResetCamera () {
		// Reset scale
		SetMapScale (mapScaleNeutral);
		Vector2 averageLevelPos = new Vector2 (0,0);
		foreach (LevelData ld in CurrentWorldData.LevelDatas.Values) {
			averageLevelPos += ld.PosGlobal;
		}
		averageLevelPos /= CurrentWorldData.LevelDatas.Count;
		editorCamera.transform.localPosition = new Vector3 (averageLevelPos.x,averageLevelPos.y, editorCamera.transform.localPosition.z);
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
		allLevelTiles = new List<List<LevelTile>> ();
		// For every world...
		for (int worldIndex=0; worldIndex<dataManager.NumWorldDatas; worldIndex++) {
			allLevelTiles.Add (new List<LevelTile>());
			// For every level in this world...
			WorldData wd = dataManager.GetWorldData (worldIndex);
			foreach (LevelData levelData in wd.LevelDatas.Values) {
				LevelTile newLevelTile = ((GameObject)Instantiate (levelTilePrefab)).GetComponent<LevelTile> ();
				newLevelTile.transform.SetParent (worldLayerGOs[worldIndex].transform);
				newLevelTile.Initialize (this, wd, levelData);
				allLevelTiles[worldIndex].Add (newLevelTile);
			}
		}
	}
	/*
	private void ReloadCurrentLevelLinkViews() {
		isRemakingAllLevelLinkViews = true;
		// Destroy 'em first!
		DestroyAllLevelLinkViews ();
		// Make 'em hot & fresh!
		levelLinkViews = new List<LevelLinkView> ();
		for (int i=0; i<CurrentWorldData.LevelLinkDatas.Count; i++) {
			AddLevelLinkView(CurrentWorldData.LevelLinkDatas[i]);
		}
		isRemakingAllLevelLinkViews = false;
	}
	private void AddLevelLinkView(LevelLinkData levelLinkDataRef) {
		LevelLinkView newLevelLinkView = ((GameObject) Instantiate(levelLinkPrefab)).GetComponent<LevelLinkView>();
		newLevelLinkView.transform.SetParent (worldLayerGOs[currentWorldIndex].transform);
		newLevelLinkView.Initialize (this, CurrentWorldData, levelLinkDataRef);
		levelLinkViews.Add(newLevelLinkView);
		// If I'm NOT initializing ALL the links here (i.e. I've just added ONE new link), then update the LevelTiles affected!!
		if (!isRemakingAllLevelLinkViews) {
			UpdateLevelTileWallsOfLevelLinkView (newLevelLinkView);
		}
	}
	*/

	private void ReloadAllWorldDatasAndRemakeMap () {
		dataManager.ReloadWorldDatas ();
		ResetMap (currentWorldIndex);
	}
		
		
		
	// ================================================================
	//  Doers
	// ================================================================
	private void SetMapScale(float _mapScale) {
		mapScale = _mapScale;
		mapScale = Mathf.Max (0.8f, Mathf.Min (10, mapScale)); // Don't let scale get TOO crazy now.
		editorCamera.SetScale (mapScale);
		// Update my level tiles' text scales!
		if (allLevelTiles != null) {
			foreach (LevelTile levelTile in CurrentWorldLevelTiles) {
				levelTile.OnMapScaleChanged (mapScale);
			}
		}
	}
	private void MoveCamera(float xMove, float yMove) {
		editorCamera.transform.localPosition += new Vector3 (xMove, yMove);
	}
	private void MoveCameraTo(float xPos, float yPos) {
		editorCamera.transform.localPosition = new Vector3 (xPos, yPos, editorCamera.transform.localPosition.z);
	}

	
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
		thisLevelTile.OnSelected (mousePosWorld);
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
		foreach (LevelTile levelTile in CurrentWorldLevelTiles) {
			SelectLevelTile(levelTile);
		}
	}

	/*
	public void SetLevelLinkViewSelected(LevelLinkView thisLevelLinkView, LevelLinkViewConnectionCircle connectionCircle) {
		levelLinkViewSelected = thisLevelLinkView;
		connectionCircle.StartDraggingMe ();
	}
	private void DeselectLevelLinkViewSelected() {
		levelLinkViewSelected.OnDeselected ();
		levelLinkViewSelected = null;
	}
	
	private void DeleteLevelLinkView(LevelLinkView levelLinkView) {
		if (levelLinkView != null) {
			// Remove from MY views
			levelLinkViews.Remove(levelLinkView);
			// Remove from levelLinks!
			CurrentWorldData.RemoveLevelLinkData(levelLinkView.LevelLinkDataRef, true);
			// Update the LevelTiles affected!!
			UpdateLevelTileWallsOfLevelLinkView (levelLinkView);
			// Destroy the levelLinkView!
			Destroy (levelLinkView.gameObject);
		}
	}

	private void UpdateLevelTileWallsOfLevelLinkView (LevelLinkView levelLinkView) {
		GetLevelTileByKey (levelLinkView.LevelLinkDataRef.LevelDataAKey).RemakeWallLines ();
		GetLevelTileByKey (levelLinkView.LevelLinkDataRef.LevelDataBKey).RemakeWallLines ();
	}
	*/

	// TODO: This! Convert to StreamingAssets.
	private void MoveLevelTilesSelectedLevelFilesToTrashFolder() {
		// No tiles selected? Womp, don't do anything LOL
		if (tilesSelected.Count == 0) { return; }
		// Simply delete all the LevelLinks involving these levels.
		for (int i=0; i<tilesSelected.Count; i++) {
			LevelData levelDeletingData = tilesSelected[i].LevelDataRef;
			// Move or delete LINKS affected!
			List<LevelLinkData> linksWithJustThisLevel = CurrentWorldData.GetLevelLinksConnectingLevel (levelDeletingData.LevelKey);
			foreach (LevelLinkData lld in linksWithJustThisLevel) {
				CurrentWorldData.RemoveLevelLinkData (lld, false);
			}
		}
		// Move the files!!
		for (int i=tilesSelected.Count-1; i>=0; --i) {
//			DeselectLevelTile (levelTilesSelected[i]);
			CurrentWorldData.MoveLevelFileToTrashFolder (tilesSelected[i].LevelKey);
		}

		// Tell the worldData to update its LevelLinks file!
		CurrentWorldData.ResaveLevelLinksFile ();
		
		// Reload everything right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
		#if UNITY_EDITOR
//		UnityEditor.AssetDatabase.ImportAsset (saveLocation + fileName);
		UnityEditor.AssetDatabase.Refresh ();
		#endif

		// Reload this map, yo.
		ReloadAllWorldDatasAndRemakeMap ();
	}

	private void MoveLevelTilesSelectedAndLinksToWorld (int worldIndexTo) {
		// No tiles selected? Womp, don't do anything LOL
		if (tilesSelected.Count == 0) { return; }
		int worldIndexFrom = tilesSelected[0].WorldIndex; // We can assume all levelTilesSelected are in the same world.
		WorldData worldDataTo = GetWorldData (worldIndexTo);
		WorldData worldDataFrom = GetWorldData (worldIndexFrom);
//		string worldToFolderName = GameProperties.GetWorldName (worldIndexTo); // name of the folda we're moving the files TO.
		// If we're trying to move these tiles to the world they're ALREADY in, do nothin'!
		if (worldIndexFrom == worldIndexTo) { return; }
		// Compile a list of the DATAS of all the levels we're moving.
		LevelData[] levelDatasMoving = new LevelData [tilesSelected.Count];
		for (int i=0; i<levelDatasMoving.Length; i++) {
			levelDatasMoving[i] = tilesSelected[i].LevelDataRef;
		}
		// MOVE LEVELS and UPDATE LINKS!!
		for (int i=0; i<levelDatasMoving.Length; i++) {
			string levelKey = levelDatasMoving[i].LevelKey;
			// Move or delete LINKS affected!
			List<LevelLinkData> linksWithJustThisLevel = GetWorldData (worldIndexFrom).GetLevelLinksConnectingLevel (levelKey);
			foreach (LevelLinkData lld in linksWithJustThisLevel) {
				string otherKey = lld.OtherKey (levelKey);
				// Is the OTHER level coming with? MOVE it!
				if (LevelUtils.IsLevelDataInArray (levelDatasMoving, otherKey)) {
					MoveLevelLinkData (lld, worldDataFrom, worldDataTo);
				}
				// The other level is NOT coming with?? DELETE it!
				else {
					worldDataFrom.RemoveLevelLinkData (lld, false);
				}
			}
			// Otherwise, move that glitterbomb!
//			DeselectLevelTile (levelTilesSelected[i]);
			CurrentWorldData.MoveLevelFileToWorldFolder (levelKey, worldIndexTo);
		}

		// NOW tell both worldDatas affected to update their LevelLinks files! :)
		worldDataFrom.ResaveLevelLinksFile ();
		worldDataTo.ResaveLevelLinksFile ();
		
		// Reload everything right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
		#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh ();
		#endif
		
		// Reload this map, yo.
		ReloadAllWorldDatasAndRemakeMap ();
	}

	private void MoveLevelLinkData (LevelLinkData levelLinkData, WorldData worldDataFrom, WorldData worldDataTo) {
		worldDataTo.AddLevelLinkData (levelLinkData, false);
		worldDataFrom.RemoveLevelLinkData (levelLinkData, false);
	}

	
	
	// ================================================================
	//  Events
	// ================================================================
	private void ClearLevelSearch () {
		isSearchingLevel = false;
		levelSearchString = "";
		UpdateLevelTilesFromSearchString ();
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
		/*
		// MAKING a LevelLinkData!
		else {
			// FIRST spot of the link??
			if (newLinkFirstLevelTile==null){// && isPlacingFirstPointOfNewLink) {
				newLinkFirstLevelTile = levelTile;
				newLinkFirstConnectionPos = GetConnectionPosRelativeToLevelTile(levelTile, MousePosWorld);
				// We're no longer placing the first point.
//				isPlacingFirstPointOfNewLink = false;
			}
			// SECOND spot of the link!...
			else if (newLinkFirstLevelTile!=null) {
				// SAME tile as the one I already clicked? Okay, umm, cancel linking anything.
				if (levelTile == newLinkFirstLevelTile) {
				}
				// DIFFERENT second tile!...
				else {
					// Add a new LevelLinkData!!
					string levelKeyA = newLinkFirstLevelTile.LevelKey;
					string levelKeyB = levelTile.LevelKey;
					Vector2 connectionPosA = newLinkFirstConnectionPos;
					Vector2 connectionPosB = GetConnectionPosRelativeToLevelTile(levelTile, MousePosWorld);
					LevelLinkData newLevelLinkData = CurrentWorldData.AddLevelLinkData(levelKeyA,levelKeyB, connectionPosA,connectionPosB, true);
					// Okay, add a new levelLinkView now.
					AddLevelLinkView(newLevelLinkData);
				}
				// Aand now nullify the making-link variables.
				newLinkFirstLevelTile = null;
				newLinkFirstConnectionPos = Vector2.zero;
			}
		}
		*/
	}

	private void UpdateComponentVisibilities() {
		for (int i=0; i<CurrentWorldLevelTiles.Count; i++) { CurrentWorldLevelTiles[i].UpdateComponentVisibilities(); }
//		for (int i=0; i<levelLinkViews.Count; i++) { levelLinkViews[i].UpdateComponentVisibilities(); }
	}
	private void ToggleLevelTileDesignerFlagsVisibility() {
		MapEditorSettings.DoShowDesignerFlags = !MapEditorSettings.DoShowDesignerFlags;
		UpdateComponentVisibilities ();
	}
	private void ToggleLevelTileNamesVisibility() {
		MapEditorSettings.DoShowLevelNames = !MapEditorSettings.DoShowLevelNames;
		UpdateComponentVisibilities ();
	}
	private void ToggleLevelTileStarsVisibility() {
		MapEditorSettings.DoShowLevelTileStars = !MapEditorSettings.DoShowLevelTileStars;
		UpdateComponentVisibilities ();
	}
	private void ToggleLevelPropsVisibility() {
		MapEditorSettings.DoShowLevelProps = !MapEditorSettings.DoShowLevelProps;
		UpdateComponentVisibilities ();
	}
	private void ToggleInstructionsVisibility() {
		MapEditorSettings.DoShowInstructions = !MapEditorSettings.DoShowInstructions;
	}
//	private void HighlightIncompleteLevelLinkConnections () {
//		for (int i=0; i<levelLinkViews.Count; i++) {
//			levelLinkViews[i].DisplayAsIncompleteIfBrokenConnection ();
//		}
//	}

	public void OnLevelTileSelectionRectDeactivated () {
		if (CurrentWorldLevelTiles==null) { return; } // Safety check for runtime compile.
		// Select all the extra ones selected by the selection rect!
		for (int i=0; i<CurrentWorldLevelTiles.Count; i++) {
			if (CurrentWorldLevelTiles[i].IsWithinLevelTileSelectionRect) {
				if (!tilesSelected.Contains(CurrentWorldLevelTiles[i])) {
					SelectLevelTile (CurrentWorldLevelTiles[i]);
				}
			}
		}
	}
	
	private void UpdateLevelTilesFromSearchString () {
		// Update their visibilities!
		for (int i=0; i<CurrentWorldLevelTiles.Count; i++) {
			CurrentWorldLevelTiles[i].UpdateVisibilityFromSearchCriteria (levelSearchString);
		}
	}

	private void ZoomMapAtPoint (Vector3 screenPoint, float deltaZoom) {
		float pmapScale = mapScale;
		SetMapScale (mapScale * (1 + deltaZoom));
		// If we DID change the zoom, then also move to focus on that point!
		if (mapScale - pmapScale != 0) {
			MoveCamera (screenPoint.x * deltaZoom / mapScale, screenPoint.y * deltaZoom / mapScale);
		}
	}
	
	
	
	
	// ================================================================
	//  Update
	// ================================================================
	private void Update() {
		if (inputController==null) { return; } // Safety check for runtime compile.

		UpdateMousePosWorld();
		RegisterKeyInputs ();
		RegisterMouseInputs ();
		UpdateLevelTileSelectionRectSelection ();
		UpdateCameraMovement ();
		UpdateUI ();

//		UpdateTilesFromLevelLinkViewSelected ();
	}

	private void UpdateMousePosWorld() {
//		inputManager.UpdateMousePosRelative (editorCamera.transform.localPosition, mapScale);
		mousePosWorld = inputController.MousePosScreen;// / ScreenHandler.ScreenScale;// - new Vector3(ScreenHandler.OriginalScreenSize.x,ScreenHandler.OriginalScreenSize.y,0)*0.5f;
//		mousePosWorld /= GameProperties.WORLD_SCALE * 2; // NOTE: Idk why *2 works. :P
		mousePosWorld /= mapScale;// /mapScaleNeutral;
		mousePosWorld += new Vector2(editorCamera.transform.localPosition.x, editorCamera.transform.localPosition.y);
	}

	private void UpdateCameraMovement () {
		// Grab Panning!
		if (isGrabPanning) {
			editorCamera.transform.localPosition = cameraPosOnMouseDown + new Vector3 (mousePosScreenOnDown.x-MousePosScreen.x, mousePosScreenOnDown.y-MousePosScreen.y, 0) / mapScale;
		}
		else if (isDragPanning) {
			editorCamera.transform.localPosition += new Vector3 (MousePosScreen.x-mousePosScreenOnDown.x, MousePosScreen.y-mousePosScreenOnDown.y, 0) * DRAG_PANNING_SPEED * fTS;
		}
	}

	private void UpdateUI () {
		// We can afford to update these every frame.
		// currentWorldText
		currentWorldText.text = currentWorldIndex.ToString ();
		// instructionsText
		if (MapEditorSettings.DoShowInstructions) {
			instructionsText.color = new Color (1,1,1, 0.36f);
			instructionsText.text = instructionsTextString_enabled;
		}
		else {
			instructionsText.color = new Color (1,1,1, 0.3f);
			instructionsText.text = instructionsTextString_disabled;
		}
	}

	/*
	private void UpdateTilesFromLevelLinkViewSelected() {
		// NO levelLinkView selected??
		if (levelLinkViewSelected == null) {
			// Make sure these dudes are just null, man.
			ConnectionCircleLevelTileOverA = null;
			ConnectionCircleLevelTileOverB = null;
		}
		// YES levelLinkViewSelected!
		else {
			// Set connectionCircleLevelTileOvers!
			ConnectionCircleLevelTileOverA = GetLevelTileAtPoint(levelLinkViewSelected.ConnectionCircleA.transform.localPosition);
			ConnectionCircleLevelTileOverB = GetLevelTileAtPoint(levelLinkViewSelected.ConnectionCircleB.transform.localPosition);
			// DO plan to delete levelLinkViewSelected if A) EITHER of its ends are NOT over a LevelTile, or B) BOTH its ends are over the SAME LevelTile!
			willDeleteLevelLinkViewSelected =  connectionCircleLevelTileOverA==null
											|| connectionCircleLevelTileOverB==null
											|| connectionCircleLevelTileOverA==connectionCircleLevelTileOverB;
		}
	}
	*/
	private void UpdateLevelTileSelectionRectSelection() {
		if (CurrentWorldLevelTiles==null) { return; } // Safety check for runtime compile.
		// Update which tiles are within the rect!
		for (int i=0; i<CurrentWorldLevelTiles.Count; i++) {
			CurrentWorldLevelTiles[i].IsWithinLevelTileSelectionRect = selectionRect.IsActive && selectionRect.SelectionRect.Contains(CurrentWorldLevelTiles[i].LevelDataRef.PosGlobal);
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
		if (InputController.GetMouseButtonDown() != -1) { OnMouseDown(); }
		else if (InputController.GetMouseButtonUp() != -1) { OnMouseUp(); }
		// Wheelies
		if (Input.mouseScrollDelta != Vector2.zero) {
			float zoomSpeedScale = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) ? 5 : 1; // hold down SHIFT to zoom fastahh!
			ZoomMapAtPoint (MousePosScreen, Input.mouseScrollDelta.y*MOUSE_SCROLL_ZOOM_RATIO * zoomSpeedScale);
		}
	}
	private void RegisterKeyInputs() {
		// DELETE/BACKSPACE = move selected files to this world's trash folder
		if (tilesSelected.Count>0 && !isSearchingLevel && (Input.GetKeyDown (KeyCode.Delete) || Input.GetKeyDown (KeyCode.Backspace))) {
			MoveLevelTilesSelectedLevelFilesToTrashFolder ();
		}
		
		// Backspace
		else if (Input.GetKeyDown (KeyCode.Backspace)) {//c == "\b"[0]) {
			if (levelSearchString.Length != 0) {
				levelSearchString = levelSearchString.Substring(0, levelSearchString.Length - 1);
				UpdateLevelTilesFromSearchString ();
			}
		}
		// SHIFT + Some typeable character = Search string!
		else if ((Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)) && Input.inputString.Length > 0) {
			char c = Input.inputString[0];
			// Typeable character
			if (char.IsLetterOrDigit(c) || char.IsPunctuation(c)) {
				levelSearchString += c;
				UpdateLevelTilesFromSearchString ();
			}
		}

		// CONTROL + A = Select ALL LevelTiles!
		if (Input.GetKeyDown (KeyCode.A)) {
			SelectAllLevelTiles ();
		}
		// CONTROL + J = Open LevelJump!
		else if (Input.GetKeyDown (KeyCode.J)) {
			OpenLevelJump ();
		}
		// CONTROL + R = Reset EVERYTHING!
		else if (Input.GetKeyDown(KeyCode.R)) {
			ReloadAllWorldDatasAndRemakeMap ();
		}

		// CONTROL/ALT + ____
		else if (Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightAlt) || Input.GetKey (KeyCode.RightControl)) {
			// CONTROL + [number] = Move all LevelTiles/LevelLinks selected to that world!!
			if (Input.GetKeyDown (KeyCode.Alpha0)) { MoveLevelTilesSelectedAndLinksToWorld (0); }
			else if (Input.GetKeyDown (KeyCode.Alpha1)) { MoveLevelTilesSelectedAndLinksToWorld (1); }
			else if (Input.GetKeyDown (KeyCode.Alpha2)) { MoveLevelTilesSelectedAndLinksToWorld (2); }
			else if (Input.GetKeyDown (KeyCode.Alpha3)) { MoveLevelTilesSelectedAndLinksToWorld (3); }
			else if (Input.GetKeyDown (KeyCode.Alpha4)) { MoveLevelTilesSelectedAndLinksToWorld (4); }
			else if (Input.GetKeyDown (KeyCode.Alpha5)) { MoveLevelTilesSelectedAndLinksToWorld (5); }
			else if (Input.GetKeyDown (KeyCode.Alpha6)) { MoveLevelTilesSelectedAndLinksToWorld (6); }
			else if (Input.GetKeyDown (KeyCode.Alpha7)) { MoveLevelTilesSelectedAndLinksToWorld (7); }
			else if (Input.GetKeyDown (KeyCode.Alpha8)) { MoveLevelTilesSelectedAndLinksToWorld (8); }
			else if (Input.GetKeyDown (KeyCode.Alpha9)) { MoveLevelTilesSelectedAndLinksToWorld (9); }
			else if (Input.GetKeyDown (KeyCode.Minus)) { MoveLevelTilesSelectedAndLinksToWorld (10); }
		}
		
		// Visibility togglin'
//		else if (Input.GetKeyDown(KeyCode.L)) { HighlightIncompleteLevelLinkConnections (); } // L = make all LevelLinks that DON'T seem to connect to a street in one of the levels turn RED!
		else if (Input.GetKeyDown(KeyCode.F)) { ToggleLevelTileDesignerFlagsVisibility(); } // F = toggle flags
		else if (Input.GetKeyDown(KeyCode.N)) { ToggleLevelTileNamesVisibility(); } // N = toggle names
		else if (Input.GetKeyDown(KeyCode.P)) { ToggleLevelPropsVisibility(); } // P = toggle props
		else if (Input.GetKeyDown(KeyCode.T)) { ToggleLevelTileStarsVisibility(); } // T = toggle stars
		else if (Input.GetKeyDown(KeyCode.I)) { ToggleInstructionsVisibility(); } // I = toggle instructions
		
		// LOAD DIFFERENT WORLDS
		else if (Input.GetKeyDown(KeyCode.Alpha0)) { SetCurrentWorld(0); }
		else if (Input.GetKeyDown(KeyCode.Alpha1)) { SetCurrentWorld(1); }
		else if (Input.GetKeyDown(KeyCode.Alpha2)) { SetCurrentWorld(2); }
		else if (Input.GetKeyDown(KeyCode.Alpha3)) { SetCurrentWorld(3); }
		else if (Input.GetKeyDown(KeyCode.Alpha4)) { SetCurrentWorld(4); }
		else if (Input.GetKeyDown(KeyCode.Alpha5)) { SetCurrentWorld(5); }
		else if (Input.GetKeyDown(KeyCode.Alpha6)) { SetCurrentWorld(6); }
		else if (Input.GetKeyDown(KeyCode.Alpha7)) { SetCurrentWorld(7); }
		else if (Input.GetKeyDown(KeyCode.Alpha8)) { SetCurrentWorld(8); }
		else if (Input.GetKeyDown(KeyCode.Alpha9)) { SetCurrentWorld(9); }
		else if (Input.GetKeyDown(KeyCode.Minus)) { SetCurrentWorld (10); }
		
		// DEBUG camera controls with these keys
		else if (Input.GetKeyDown (KeyCode.C))  { ResetCamera (); }
		else if (Input.GetKey (KeyCode.LeftArrow))  { MoveCamera(-ARROW_KEYS_PAN_SPEED/mapScale*fTS,0); }
		else if (Input.GetKey (KeyCode.RightArrow)) { MoveCamera( ARROW_KEYS_PAN_SPEED/mapScale*fTS,0); }
		else if (Input.GetKey (KeyCode.DownArrow))  { MoveCamera(0,-ARROW_KEYS_PAN_SPEED/mapScale*fTS); }
		else if (Input.GetKey (KeyCode.UpArrow))    { MoveCamera(0, ARROW_KEYS_PAN_SPEED/mapScale*fTS); }
		else if (Input.GetKey (KeyCode.Z)) { SetMapScale(mapScale/(1-ZOOM_SPEED_KEYBOARD*fTS)); }
		else if (Input.GetKey (KeyCode.X)) { SetMapScale(mapScale*(1-ZOOM_SPEED_KEYBOARD*fTS)); }

		// ESCAPE = Cancel searching
		else if (Input.GetKeyDown (KeyCode.Escape)) {
			ClearLevelSearch ();
		}
		// DELETE = delete levelLinkSelected
//		else if (Input.GetKeyDown(KeyCode.Delete)) { DeleteLevelLink(); }
		// CONTROL = set isCreatingLink to true!
//		else if (keyCode == CONTROL) {
//			if (editingState==STATE_DRAGGING_LINKS && newLinkFirstLevelTile==null) {
//				isPlacingFirstPointOfNewLink = true;
//			}
//		}

	}
	
	
	private void OnMouseDown() {
		int mouseButton = InputController.GetMouseButtonDown();
		// LEFT click?
		if (mouseButton == 0) {
			Debug.Log ("mousePosWorld: " + mousePosWorld);
			// Double-click?!
//			Debug.Log(Time.frameCount + " timeSinceMouseButtonDown: " + timeSinceMouseButtonDown + "     " + Vector2.Distance (MousePosScreen, mousePosScreenOnDown));
//			if (timeSinceMouseButtonDown < DOUBLE_CLICK_TIME && Vector2.Distance (MousePosScreen, mousePosScreenOnDown) < 4) {
			// Tell ALL selected tiles to update their click offset!
			foreach (LevelTile levelTile in tilesSelected) {
				levelTile.SetMouseClickOffset (mousePosWorld);
			}
			// Are we NOT over anything? Activate the selectionRect AND release any selected tiles!
			if (!IsMouseOverAnything ()) {
				selectionRect.Activate ();
				ReleaseLevelTilesSelected();
			}
		}
		// RIGHT click?
		else if (mouseButton == 1) {
			isDragPanning = true;
		}
		// MIDDLE click?
		else if (mouseButton == 2) {
			isGrabPanning = true;
		}
		// Update on-mouse-down vectors!
		cameraPosOnMouseDown = editorCamera.transform.localPosition;
		mousePosScreenOnDown = MousePosScreen;
	}
	private void OnMouseUp() {
		int mouseButton = InputController.GetMouseButtonUp ();

		// Dragging a LEVEL TILE(S), released the LEFT mouse button, AND not holding down the multi-selection key??
		if (tilesSelected.Count>0 && mouseButton==0 && !CanSelectMultipleTiles()) {
			// Save all dragging tiles' levelData to file (and clear out any snapshot datas)!
			foreach (LevelTile tile in tilesSelected) {
				LevelSaverLoader.UpdateLevelPropertiesInLevelFile(tile.LevelDataRef);
			}
			// Update the worlds' bounds!
			CurrentWorldData.SetAllLevelDatasFundamentalProperties ();
//			// Mouse up = release all levelTilesSelected!
//			ReleaseLevelTilesSelected();
		}
		/*
		// Dragging a LEVEL LINK??
		else if (levelLinkViewSelected != null) {
			// Are one of these guys null OR they're the same?? Delete the link!
			if (willDeleteLevelLinkViewSelected) {
				DeleteLevelLinkView(levelLinkViewSelected);
				levelLinkViewSelected = null;
			}
			// Otherwise, update the level link's LevelDatas!
			else {
				// Update the source LevelLink and the LevelDatas!!
				if (connectionCircleLevelTileOverA != null) { // Uhh, weird crash check.
					LevelLinkData levelLinkData = levelLinkViewSelected.LevelLinkDataRef;
					levelLinkData.SetLevelDataAKey(connectionCircleLevelTileOverA.LevelKey);
					levelLinkData.SetLevelDataBKey(connectionCircleLevelTileOverB.LevelKey);
					levelLinkViewSelected.SetLevelLinkConnectingPosFromConnectionCirclesPoses();
				}
				// Deselect the levelLinkViewSelected.
				DeselectLevelLinkViewSelected ();
				// Resave LevelLinks file!
				CurrentWorldData.ResaveLevelLinksFile();
			}
		}
		*/

		// RIGHT click?
		if (mouseButton == 1) {
			isDragPanning = false;
		}
		// MIDDLE click?
		else if (mouseButton == 2) {
			isGrabPanning = false;
		}
	}
	private void OnMouseDoubleClicked() {
		// Am I over any level?? Load it!!
		for (int i=CurrentWorldLevelTiles.Count-1; i>=0; --i) {
			LevelTile tile = CurrentWorldLevelTiles[i];
			if (tile.IsDragReadyMouseOverMe) {
				OpenGameplayScene(tile.WorldIndex, tile.LevelKey);
				break;
			}
		}
	}

	private void OpenScene(string sceneName) { StartCoroutine (OpenSceneCoroutine(sceneName)); }
	private IEnumerator OpenSceneCoroutine (string sceneName) {
		// First frame, blur the screen up, Craig
		editorCamera.BlurScreenForLoading ();
		yield return null;

		// Second frame, load up the gameplay scene!
		UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName);
		yield return null;
	}


	private void OpenGameplayScene(int worldIndex, string levelKey) {
//		GameplaySnapshotController.SetWorldAndLevelToLoad (worldIndex, levelKey);
		dataManager.currentLevelData = dataManager.GetLevelData(worldIndex, levelKey, true);
		OpenScene(SceneNames.Gameplay);
	}
	private void OpenLevelJump() {
		OpenScene(SceneNames.LevelJump);
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
//	private void DestroyAllLevelLinkViews() {
//		if (levelLinkViews != null) {
//			for (int i=0; i<levelLinkViews.Count; i++) {
//				Destroy (levelLinkViews[i].gameObject);
//			}
//			levelLinkViews.Clear();
//		}
//	}
	private void OnDestroy() {
		// Save values!
		MapEditorSettings.SaveAll();

		// Save where the camera is at!
		if (editorCamera != null) {
			SaveStorage.SetFloat (SaveKeys.MapEditor_CameraPosX, editorCamera.transform.localPosition.x);
			SaveStorage.SetFloat (SaveKeys.MapEditor_CameraPosY, editorCamera.transform.localPosition.y);
		}
		SaveStorage.SetFloat (SaveKeys.MapEditor_MapScale, mapScale);
	}


}






