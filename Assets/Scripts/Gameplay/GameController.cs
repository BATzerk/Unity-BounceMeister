﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		// We haven't provided a level to play and this is Gameplay scene? Ok, load up the last played level instead!
		if (dataManager.currentLevelData==null && thisSceneName==SceneNames.Gameplay) {
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
			// TEMP! TEMP! For converting scenes into level text files.
			if (level==null) {
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
			}
			if (tf_world == null) {
				tf_world = GameObject.Find("GameWorld").transform;
			}
			player = GameObject.FindObjectOfType<Player>(); // Again, this is only for editing.
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
	private string thisSceneName { get { return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; } }
	private void ReloadScene () { OpenScene (thisSceneName); }
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

		// Save what's up!
		SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, worldIndex);
		SaveStorage.SetString(SaveKeys.LastPlayedLevelKey, levelKey);

		// Use this opportunity to call SAVE with SaveStorage, yo! (This causes a brief stutter, so I'm opting to call it when the game is already loading.)
		SaveStorage.Save ();
		// Dispatch the post-function event!
		eventManager.OnStartLevel(level);
	}

	private void MakePlayer(PlayerTypes type, LevelData levelData) {
		if (player != null) { DestroyPlayer(); } // Just in case.

		switch (type) {
		case PlayerTypes.Alph:
			player = Instantiate(ResourcesHandler.Instance.Alph).GetComponent<Alph>();
			break;
		case PlayerTypes.Britta:
			player = Instantiate(ResourcesHandler.Instance.Britta).GetComponent<Britta>();
			break;
		default:
			Debug.LogError("Whoa! Player type totally not recognized: " + type);
			break;
		}
		PlayerData playerData = new PlayerData();
		playerData.pos = GetPlayerStartingPosInLevel(levelData);
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
		eventManager.OnSetPaused(isPaused);
	}
	private void UpdateTimeScale () {
		if (isPaused) { Time.timeScale = 0; }
		else if (debug_isSlowMo) { Time.timeScale = 0.2f; }
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

		else if (Input.GetKeyDown(KeyCode.A)) { MakePlayer(PlayerTypes.Alph, level.LevelDataRef); }
		else if (Input.GetKeyDown(KeyCode.B)) { MakePlayer(PlayerTypes.Britta, level.LevelDataRef); }

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






