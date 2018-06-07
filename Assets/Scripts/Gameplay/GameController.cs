using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Fix next-level side being inaccurate
// TODO: Fix next level not working...
// TODO: When die, restart at last entered level/position
//TODO: Wall-grind particles, and wall-grind sprite change (just a diagonal line from player-to-wall is enough)

public class GameController : MonoBehaviour {
	// Properties
	private bool isPaused = false;
	private bool debug_isSlowMo = false;
	// References
	[SerializeField] private Transform tf_world;
	[SerializeField] private Player player=null;
	[SerializeField] private Level level=null; // TODO: load this dynamically instead! Do our editing in the editor.

	// Getters
	public Player Player { get { return player; } }

	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }
	private InputController inputController { get { return InputController.Instance; } }

	private Vector2 mousePosWorld() {
		return Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}


	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start () {
		// We've defined our currentLevelData before this scene! Load up THAT level!!
		if (dataManager.currentLevelData != null) {
			StartGameAtLevel(dataManager.currentLevelData);
		}
		// We have NOT provided any currentLevelData!...
		else {
			// Initialize the existing level as a premade level! So we can start editing/playing/saving it right outta the scene.
			// TEMP! TEMP! For converting scenes into level text files.
			if (level==null) {
				level = GameObject.FindObjectOfType<Level>();
				if (level == null) {
					GameObject levelGO = GameObject.Find("Structure");
					level = levelGO.AddComponent<Level>();
				}
			}
			if (tf_world == null) {
				tf_world = GameObject.Find("GameWorld").transform;
			}
			player = GameObject.FindObjectOfType<Player>(); // Again, this is only for editing.
			level.InitializeAsPremadeLevel(this);
			dataManager.SetCoinsCollected (0);
			UpdateTimeScale();
//			ResetPlayerAtLevelDoor(dataManager.levelToDoorID);
			eventManager.OnStartLevel(level);
		}

		// Add event listeners!
		eventManager.PlayerDieEvent += OnPlayerDie;
		eventManager.PlayerEscapeLevelBoundsEvent += OnPlayerEscapeLevelBounds;
	}
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.PlayerDieEvent -= OnPlayerDie;
		eventManager.PlayerEscapeLevelBoundsEvent -= OnPlayerEscapeLevelBounds;
	}


	// ----------------------------------------------------------------
	//  Doers - Loading Level
	// ----------------------------------------------------------------
	private void ReloadScene () { OpenScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
	private void OpenScene (string sceneName) { StartCoroutine (OpenSceneCoroutine (sceneName)); }
	private IEnumerator OpenSceneCoroutine (string sceneName) {
//		// First frame: Blur it up.
//		cameraController.DarkenScreenForSceneTransition ();
//		yield return null;

		// Second frame: Load up that business.
		UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName);
		yield return null;
	}

	/** This actually shows "Loading" overlay FIRST, THEN next frame loads the world. */
	public void StartGameAtLevel(LevelData levelData) { StartGameAtLevel(levelData.worldIndex, levelData.levelKey); }
	public void StartGameAtLevel (int worldIndex, string levelKey) {
//		StartCoroutine (StartGameAtLevelCoroutine(worldIndex, levelKey));
//	}
//	private IEnumerator StartGameAtLevelCoroutine (int worldIndex, string levelKey) {
////		// It's all a blur to me!
////		cameraController.BlurScreenForLoading ();
//		yield return null;

		// Wipe everything totally clean.
		DestroyPlayer();
		DestroyLevel();


		LevelData levelData = dataManager.GetLevelData(worldIndex, levelKey, true);
		dataManager.currentLevelData = levelData;

		// Make Level!
		level = new GameObject().AddComponent<Level>();
		level.Initialize(this, tf_world, levelData);
		// Make Player!
		player = Instantiate(ResourcesHandler.Instance.Player).GetComponent<Player>();
		PlayerData playerData = new PlayerData();
		playerData.pos = GetPlayerStartingPosInLevel(levelData);
		Debug.Log(Time.frameCount + " GetPlayerStartingPosInLevel: " + levelData.levelKey + "  " + playerData.pos);
//		playerData.pos = GetLevelDoorPos(dataManager.levelToDoorID);
		player.Initialize(level, playerData);

		// Reset things!
		dataManager.SetCoinsCollected (0);
		UpdateTimeScale();
//		ResetPlayerAtLevelDoor(dataManager.levelToDoorID);

		// Use this opportunity to call SAVE with SaveStorage, yo! (This causes a brief stutter, so I'm opting to call it when the game is already loading.)
		SaveStorage.Save ();
		// Dispatch the post-function event!
		eventManager.OnStartLevel(level);
//		yield return null;
	}

	private void DestroyLevel() {
		if (level != null) { Destroy(level.gameObject); }
		level = null;
	}
	private void DestroyPlayer() {
		if (player != null) { Destroy(player.gameObject); }
		player = null;
	}


	private Vector2 GetPlayerStartingPosInLevel(LevelData ld) {
		Vector2 posExited = dataManager.playerPosGlobalOnExitLevel;
		int sideEntering = dataManager.playerSideEnterNextLevel;
		// Undefined?? Return the PlayerStart!
		if (posExited.Equals(Vector2Extensions.NaN)) {
			return ld.PlayerStartPos();
		}
		// Otherwise, use the knowledge we have!
		dataManager.playerPosGlobalOnExitLevel = Vector2Extensions.NaN; // Make sure to "clear" this. It's been used!
		dataManager.playerSideEnterNextLevel = -1; // Make sure to "clear" this. It's been used!
//		Vector2 originalPos = posExited - ld.posGlobal; // Convert the last known coordinates to this level's coordinates.
//		int sideEntered = MathUtils.GetSidePointIsOn(ld.BoundsGlobal, posExited);
		Vector2Int offsetDir = MathUtils.GetOppositeDir(sideEntering);
		const float extraDistToEnter = 3f; // Well let me just take an extra step in, really wanna feel at "home".

		Vector2 posRelative = posExited - ld.posGlobal; // Convert the last known coordinates to this level's coordinates.
		return posRelative + new Vector2(offsetDir.x*extraDistToEnter, offsetDir.y*extraDistToEnter);
	}



	/*
	public void SetCurrentWorld (int worldIndex, string levelKey) {
		// Is this ALREADY the CurrentWorld?? Don't do anything. :)
		if (currentLevel!=null && currentLevel.WorldIndex==worldIndex) { return; }

		// First, ALWAYS restore any modified levels (so we can't teleport out of a level and leave it in a potentially unsolvable state).
		RestoreChangedLevelsToLastSnapshot (true);
		currentWorldIndex = worldIndex;

		// Load the most recent snapshot of the player!
		PlayerData playerData = snapshotController.GetSnapshotPlayerData (worldIndex, levelKey);

		// SET the playerData snapshot!
		snapshotController.GetSnapshotData (worldIndex).playerData = playerData;//LoadAndSetSnapshotPlayerData(worldIndex, levelKey);
		// Initialize this world's content manually!
		CurrentWorld.OnSetAsCurrentWorld (playerData.currentLevelKey);
		// Reset player from snapshot, which will ALSO set the currentLevel!
		ResetPlayer (playerData);
		// Tell the camera to update its world values!
		cameraController.OnSetCurrentWorld (worldIndex);
	}


	// ================================================================
	//  Transitioning Between Levels
	// ================================================================
	private void SetCurrentLevel (Level _currentLevel) {
		Level levelExited = currentLevel;

		// Notify both the old and the new!
		if (currentLevel != null) { currentLevel.IsCurrentLevel = false; }
		if (_currentLevel != null) { _currentLevel.IsCurrentLevel = true; }

		// Did we ALREADY have a currentLevel??
		if (currentLevel != null) {
			// Analytics!
			OnPlayerLeaveCurrentLevel ();
		}

		currentLevel = _currentLevel;

		// Have we provided a NULL level?? Stop doing things here. We're probably doing something special.
		if (currentLevel == null) {
			return;
		}

		// If the player exists, then tell this level to light the heck up!
		if (player != null) {
			currentLevel.LightUpLevel (player.Head.Street, player.Head.LocCenter);
		}
		// Tell the World we've totally been in this level, man!
		currentLevel.WorldRef.OnPlayerEnterLevel (currentLevel, levelExited);

		// Update this for good measure.
		UpdateResetButtonActionAvailable ();

		if (GameProperties.DoSaveSnapshotWhenSetCurrentWorld (currentLevel.LevelKey)) { // HACKY, man. I really don't like these separated values: it de-unifies things. (Though I can't think of an easy solution ATM.)
			// We're ALSO not in ChallengeMode...!
			if (!ChallengeModeController.IsChallengeMode) {
				// Save that we've most recently loaded up this world!
				GameManagers.Instance.DataManager.SetWorldIndexOnLoadGameScene (currentWorldIndex);
			}
		}

		// Update CanSaveToSaveStorage!
		UpdateCanSaveToSaveStorage ();

		// Dispatch event!
		GameManagers.Instance.EventManager.OnGameControllerSetCurrentLevel (currentLevel);
	}
	*/


	/** Super simple for now. One level per side. */
	private void OnPlayerEscapeLevelBounds(int sideEscaped) {
		WorldData currentWorldData = level.WorldDataRef;
		LevelData nextLevelData = currentWorldData.GetLevelAtSide(level.LevelDataRef, sideEscaped);
		if (nextLevelData != null) {
			dataManager.playerPosGlobalOnExitLevel = player.PosGlobal;
			dataManager.playerSideEnterNextLevel = MathUtils.GetOppositeSide(sideEscaped);
			StartGameAtLevel(nextLevelData);
		}
		else {
			Debug.LogWarning("Whoa! No level at this side: " + sideEscaped);
		}
	}



	// ----------------------------------------------------------------
	//  Doers - Gameplay
	// ----------------------------------------------------------------
	private void TogglePause () {
		isPaused = !isPaused;
		UpdateTimeScale ();
	}
	private void UpdateTimeScale () {
		if (isPaused) { Time.timeScale = 0; }
		else if (debug_isSlowMo) { Time.timeScale = 0.2f; }
		else { Time.timeScale = 1; }
	}
	public Vector3 GetLevelDoorPos(string levelDoorID) {
		LevelDoor[] allDoors = GameObject.FindObjectsOfType<LevelDoor>();
		LevelDoor correctDoor = null; // I'll specify next.
		foreach (LevelDoor door in allDoors) {
			if (correctDoor==null || door.MyID == levelDoorID) { // note: if there's no correctDoor yet, just set it to this first guy. So we at least end up at SOME door.
				correctDoor = door;
				break;
			}
		}
		if (correctDoor != null) {
			return correctDoor.PosLocal;
		}
		else {
			Debug.LogWarning("Oops! Couldn't find a door with this ID: " + levelDoorID);
			return Vector3.zero;
		}
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		RegisterMouseInput();
		RegisterButtonInput ();
	}

	private void RegisterMouseInput() {
		// ~~~~ DEBUG ~~~~
		if (Input.GetMouseButton(1) && player!=null) {
			player.SetPosGlobal(mousePosWorld());
		}
	}
	private void RegisterButtonInput () {
		bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

//		// Game Flow
//		if (Input.GetKeyDown(KeyCode.Q)) {
//			OpenScene(SceneNames.LevelSelect);
//		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			TogglePause();
		}

		// ~~~~ DEBUG ~~~~
		if (Input.GetKeyDown(KeyCode.Equals)) { Debug_JumpToLevelAtSide(Sides.T); return; }
		else if (Input.GetKeyDown(KeyCode.RightBracket)) { Debug_JumpToLevelAtSide(Sides.R); return; }
		else if (Input.GetKeyDown(KeyCode.Quote)) { Debug_JumpToLevelAtSide(Sides.B); return; }
		else if (Input.GetKeyDown(KeyCode.LeftBracket)) { Debug_JumpToLevelAtSide(Sides.L); return; }
		else if (Input.GetKeyDown(KeyCode.J)) {
			OpenScene(SceneNames.LevelJump);
		}
		else if (Input.GetKeyDown(KeyCode.M)) {
			OpenScene(SceneNames.MapEditor);
		}
		else if (Input.GetKeyDown(KeyCode.Return)) {
			ReloadScene();
			return;
		}
		else if (Input.GetKeyDown(KeyCode.T)) {
			debug_isSlowMo = !debug_isSlowMo;
			UpdateTimeScale();
		}

		else if (Input.GetKeyDown(KeyCode.S)) { // S = Save level as text file!
			LevelSaverLoader.SaveLevelFile(level);
		}
		else if (Input.GetKeyDown(KeyCode.R)) { // R = Reload current Level.
			StartGameAtLevel(level.LevelDataRef);
		}

			
		// ALT + ___
		if (isKey_alt) {
			
		}
		// CONTROL + ___
		if (isKey_control) {
		}
		// SHIFT + ___
		if (isKey_shift) {
		}
	}



	// ----------------------------------------------------------------
	//  Debug
	// ----------------------------------------------------------------
	private void Debug_JumpToLevelAtSide(int side) {
		OnPlayerEscapeLevelBounds(side); // Pretend the player just exited in this direction.
		player.SetPosGlobal(level.PosGlobal); // just put the player in the center of the level.
	}



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnPlayerDie(Player player) {
		Invoke("ReloadScene", 1f);
	}






}






