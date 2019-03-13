using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	// Properties
	private bool isPaused = false;
	private bool isSlowMo = false;
	// References
	[SerializeField] private Transform tf_world;
	private Player player=null;
	private Level level=null;

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
		// We haven't provided a level to play and this is Gameplay scene? Ok, load up the last played level instead!
		if (dataManager.currentLevelData==null && SceneHelper.IsGameplayScene()) {
			int worldIndex = SaveStorage.GetInt(SaveKeys.LastPlayedWorldIndex);
			string levelKey = SaveStorage.GetString(SaveKeys.LastPlayedLevelKey);
			dataManager.currentLevelData = dataManager.GetLevelData(worldIndex, levelKey, false);
		}

		// We've defined our currentLevelData before this scene! Load up THAT level!!
		if (dataManager.currentLevelData != null) {
			StartGameAtLevel(dataManager.currentLevelData);
		}
		// We have NOT provided any currentLevelData!...
		else {
			// Initialize the existing level as a premade level! So we can start editing/playing/saving it right outta the scene.
			// TEMP! For converting scenes into level text files.
			level = GameObject.FindObjectOfType<Level>();
			if (level == null) {
				GameObject levelGO = GameObject.Find("Structure");
				if (levelGO==null) {
					levelGO = new GameObject();
					levelGO.transform.localPosition = Vector3.zero;
					levelGO.transform.localScale = Vector3.one;
				}
				level = levelGO.AddComponent<Level>();
			}
			if (tf_world == null) {
				tf_world = GameObject.Find("GameWorld").transform;
			}
//			player = GameObject.FindObjectOfType<Player>(); // Again, this is only for editing.
			MakePlayer(PlayerTypes.Alph, Vector2.zero);
			level.InitializeAsPremadeLevel(this);
			dataManager.SetCoinsCollected (0);
			UpdateTimeScale();
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
	public void StartGameAtLevel(LevelData levelData) { StartGameAtLevel(levelData.worldIndex, levelData.levelKey); }
	public void StartGameAtLevel (int worldIndex, string levelKey) {
		// Wipe everything totally clean.
		DestroyPlayer();
		DestroyLevel();

		LevelData levelData = dataManager.GetLevelData(worldIndex, levelKey, true);
		dataManager.currentLevelData = levelData;

		// Make Level!
		level = new GameObject().AddComponent<Level>();
		level.Initialize(this, tf_world, levelData);
		// Make Player!
		MakePlayer(PlayerTypes.Alph, levelData);

		// Reset things!
		dataManager.SetCoinsCollected (0);
		UpdateTimeScale();
		GameUtils.SetEditorCameraPos(levelData.posGlobal); // conveniently move the editor camera, too!

		// Save what's up!
		SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, worldIndex);
		SaveStorage.SetString(SaveKeys.LastPlayedLevelKey, levelKey);

		// Use this opportunity to call SAVE with SaveStorage, yo! (This causes a brief stutter, so I'm opting to call it when the game is already loading.)
		SaveStorage.Save ();
		// Dispatch the post-function event!
		eventManager.OnStartLevel(level);
	}

	private void MakePlayer(PlayerTypes type, LevelData levelData) {
		Vector2 startingPos = GetPlayerStartingPosInLevel(levelData);
		MakePlayer(type, startingPos);
	}
	private void MakePlayer(PlayerTypes type, Vector2 startingPos) {
		if (player != null) { DestroyPlayer(); } // Just in case.

		switch (type) {
		case PlayerTypes.Alph:
			player = Instantiate(ResourcesHandler.Instance.Alph).GetComponent<Alph>();
				break;
		case PlayerTypes.Britta:
			player = Instantiate(ResourcesHandler.Instance.Britta).GetComponent<Britta>();
			break;
		case PlayerTypes.Coco:
			player = Instantiate(ResourcesHandler.Instance.Coco).GetComponent<Coco>();
			break;
		default:
			Debug.LogError("Whoa! Player type totally not recognized: " + type);
			break;
		}
		PlayerData playerData = new PlayerData();
		playerData.pos = startingPos;
		player.Initialize(level, playerData);
	}


	private void DestroyLevel() {
		if (level != null) { Destroy(level.gameObject); }
		level = null;
	}
	private void DestroyPlayer() {
		if (player != null) { Destroy(player.gameObject); }
		player = null;
	}

	private void StartNewBlankLevel() {
		// Keep it in the current world, and give it a unique name.
		WorldData worldData = dataManager.GetWorldData(level.WorldIndex);
		string levelKey = worldData.GetUnusedLevelKey();
		LevelData emptyLevelData = worldData.GetLevelData(levelKey, true);
		StartGameAtLevel(emptyLevelData);
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
		const float extraDistToEnter = 3f; // Well let me just take an extra step in; I really wanna feel at "home".

		Vector2 posRelative = posExited - ld.posGlobal; // Convert the last known coordinates to this level's coordinates.
		return posRelative + new Vector2(offsetDir.x*extraDistToEnter, offsetDir.y*extraDistToEnter);
	}


	/** Super simple for now. One level per side. */
	private void OnPlayerEscapeLevelBounds(int sideEscaped) {
		WorldData currentWorldData = level.WorldDataRef;
		LevelData nextLevelData = currentWorldData.GetLevelAtSide(level.LevelDataRef, sideEscaped);
		if (nextLevelData != null) {
			Vector2 playerVel = player.Vel; // remember this so we can preserves it, ya see!
			dataManager.playerPosGlobalOnExitLevel = player.PosGlobal;
			dataManager.playerSideEnterNextLevel = Sides.GetOpposite(sideEscaped);
			StartGameAtLevel(nextLevelData);
			player.SetVel(playerVel); // messily restore the vel we had in the previous level.
		}
		else {
			Debug.LogWarning("Whoa! No level at this side: " + sideEscaped);
		}
	}



	// ----------------------------------------------------------------
	//  Doers - Gameplay
	// ----------------------------------------------------------------
    private void ReloadScene() { SceneHelper.ReloadScene(); }
	private void TogglePause () {
		isPaused = !isPaused;
		UpdateTimeScale ();
		eventManager.OnSetPaused(isPaused);
	}
	private void UpdateTimeScale () {
		if (isPaused) { Time.timeScale = 0; }
		else if (isSlowMo) { Time.timeScale = 0.2f; }
		else { Time.timeScale = 1; }
	}
//	public Vector3 GetLevelDoorPos(string levelDoorID) {
//		LevelDoor[] allDoors = GameObject.FindObjectsOfType<LevelDoor>();
//		LevelDoor correctDoor = null; // I'll specify next.
//		foreach (LevelDoor door in allDoors) {
//			if (correctDoor==null || door.MyID == levelDoorID) { // note: if there's no correctDoor yet, just set it to this first guy. So we at least end up at SOME door.
//				correctDoor = door;
//				break;
//			}
//		}
//		if (correctDoor != null) {
//			return correctDoor.PosLocal;
//		}
//		else {
//			Debug.LogWarning("Oops! Couldn't find a door with this ID: " + levelDoorID);
//			return Vector3.zero;
//		}
//	}


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
			SceneHelper.OpenScene(SceneNames.LevelJump);
		}
		else if (Input.GetKeyDown(KeyCode.M)) {
		    SceneHelper.OpenScene(SceneNames.MapEditor);
		}
		else if (Input.GetKeyDown(KeyCode.Return)) {
			SceneHelper.ReloadScene();
			return;
		}
		else if (Input.GetKeyDown(KeyCode.T)) {
			isSlowMo = !isSlowMo;
			UpdateTimeScale();
		}

		else if (Input.GetKeyDown(KeyCode.A)) { MakePlayer(PlayerTypes.Alph, level.LevelDataRef); }
		else if (Input.GetKeyDown(KeyCode.B)) { MakePlayer(PlayerTypes.Britta, level.LevelDataRef); }
		else if (Input.GetKeyDown(KeyCode.C)) { MakePlayer(PlayerTypes.Coco, level.LevelDataRef); }

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
            // CONTROL + DELETE = Clear all save data!
            if (Input.GetKeyDown(KeyCode.Delete)) {
                GameManagers.Instance.DataManager.ClearAllSaveData();
                SceneHelper.ReloadScene();
                return;
            }
			// CONTROL + N = Open new level!
			if (Input.GetKeyDown(KeyCode.N)) {
				StartNewBlankLevel();
			}

			// CONTROL + SHIFT + X = Flip Horizontal!
			if (isKey_shift && Input.GetKeyDown(KeyCode.X)) {
				if (level != null) { level.FlipHorz(); }
			}
		}
		// SHIFT + ___
		if (isKey_shift) {
		}
	}



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnPlayerDie(Player _player) {
		Invoke("ReloadScene", 1f);
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
    private void Debug_JumpToLevelAtSide(int side) {
        OnPlayerEscapeLevelBounds(side); // Pretend the player just exited in this direction.
        player.SetPosLocal(level.Debug_PlayerStartPosLocal()); // just put the player at the PlayerStart.
    }





}






