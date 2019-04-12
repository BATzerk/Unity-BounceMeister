using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    // Components
    [SerializeField] private GameTimeController gameTimeController=null;
    // References
    [SerializeField] private Transform tf_world;
	private Player player=null;
	private Level level=null;
    // Properties
    //static private Vector2 playerDiedPos; // set when Player dies; used to respawn Player at closest PlayerStart.

    // Getters
    public Player Player { get { return player; } }

	private DataManager dm { get { return GameManagers.Instance.DataManager; } }
	private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start () {
		// We haven't provided a level to play and this is Gameplay scene? Ok, load up the last played level instead!
		if (dm.currLevelData==null && SceneHelper.IsGameplayScene()) {
			int worldIndex = SaveStorage.GetInt(SaveKeys.LastPlayedWorldIndex);
			string levelKey = SaveStorage.GetString(SaveKeys.LastPlayedLevelKey(worldIndex), GameProperties.GetFirstLevelName(worldIndex));
			dm.currLevelData = dm.GetLevelData(worldIndex, levelKey, false);
		}

		// We've defined our currentLevelData before this scene! Load up THAT level!!
		if (dm.currLevelData != null) {
			StartGameAtLevel(dm.currLevelData);
		}
		// We have NOT provided any currentLevelData!...
		else {
			// Initialize the existing level as a premade level! So we can start editing/playing/saving it right outta the scene.
			// TEMP! For converting scenes into level text files.
			level = FindObjectOfType<Level>();
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
			MakePlayer(new PlayerData{type=PlayerTypes.Plunga});
			level.InitializeAsPremadeLevel(this);
			dm.SetCoinsCollected (0);
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
    //public void StartGameAtLevel (int worldIndex, string levelKey) { StartGameAtLevel(dm.GetLevelData(worldIndex, levelKey, true)); }
    private void StartGameAtLevel(LevelData ld) {
        PlayerData playerData = new PlayerData {
            pos = GetPlayerStartingPosInLevel(ld),
            type = PlayerTypeHelper.LoadLastPlayedType(),
        };
        StartGameAtLevel(ld, playerData);
    }
    public void StartGameAtLevel(LevelData ld, PlayerData playerData) {
		// Wipe everything totally clean.
		DestroyPlayer();
		DestroyLevel();

		dm.currLevelData = ld;

		// Make Level and Player!
		level = Instantiate(ResourcesHandler.Instance.Level).GetComponent<Level>();
		level.Initialize(this, tf_world, ld);
        MakePlayer(playerData);

		// Reset things!
        dm.ResetLevelEnterValues();
		dm.SetCoinsCollected(0);
		GameUtils.SetEditorCameraPos(ld.posGlobal); // conveniently move the Unity Editor camera, too!

		// Save what's up!
		SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, ld.WorldIndex);
		SaveStorage.SetString(SaveKeys.LastPlayedLevelKey(ld.WorldIndex), ld.LevelKey);

		// Use this opportunity to call SAVE with SaveStorage, yo! (This causes a brief stutter, so I'm opting to call it when the game is already loading.)
		SaveStorage.Save();
		// Dispatch the post-function event!
		eventManager.OnStartLevel(level);

        // Expand the hierarchy!
        ExpandLevelHierarchy();
    }

    private void ExpandLevelHierarchy() {
        if (!GameUtils.IsEditorWindowMaximized()) { // If we're maximized, do nothing (we don't want to open up the Hierarchy if it's not already open).
            GameUtils.SetExpandedRecursive(level.gameObject, true); // Open up Level all the way down.
            for (int i=0; i<level.transform.childCount; i++) { // Ok, now (messily) close all its children.
                GameUtils.SetExpandedRecursive(level.transform.GetChild(i).gameObject, false);
            }
            GameUtils.FocusOnWindow("Game"); // focus back on Game window.
            //GameUtils.SetGOCollapsed(transform.parent, false);
            //GameUtils.SetGOCollapsed(tf_world, false);
            //GameUtils.SetGOCollapsed(level.transform, false);
        }
    }
    
    private void MakePlayer(PlayerTypes type, LevelData levelData) {
		Vector2 startingPos = GetPlayerStartingPosInLevel(levelData);
        PlayerData playerData = new PlayerData {
            pos = startingPos,
            type = type,
        };
		MakePlayer(playerData);
	}
	private void MakePlayer(PlayerData playerData) {
		if (player != null) { DestroyPlayer(); } // Just in case.
        // Make 'em!
        player = Instantiate(ResourcesHandler.Instance.Player(playerData.type)).GetComponent<Player>();
		player.Initialize(level, playerData);
        // Save lastPlayedType!
        PlayerTypeHelper.SaveLastPlayedType(player.PlayerType());
	}
    public void SwapPlayerType(PlayerTypes _type) {
        PlayerData playerData = player.SerializeAsData() as PlayerData;
        playerData.type = _type;
        MakePlayer(playerData);
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
		WorldData worldData = dm.GetWorldData(level.WorldIndex);
		string levelKey = worldData.GetUnusedLevelKey();
		LevelData emptyLevelData = worldData.GetLevelData(levelKey, true);
		StartGameAtLevel(emptyLevelData);
	}
    private void DuplicateCurrLevel() {
        // Add a new level file, yo!
        LevelData currLD = level.LevelDataRef;
        string newLevelKey = currLD.WorldDataRef.GetUnusedLevelKey(currLD.LevelKey);
        LevelSaverLoader.SaveLevelFileAs(currLD, currLD.WorldIndex, newLevelKey);
        dm.ReloadWorldDatas();
        LevelData newLD = dm.GetLevelData(currLD.WorldIndex,newLevelKey, false);
        newLD.SetPosGlobal(newLD.posGlobal + new Vector2(15,-15)*GameProperties.UnitSize); // offset its position a bit.
        LevelSaverLoader.UpdateLevelPropertiesInLevelFile(newLD); // update file!
        dm.currLevelData = newLD;
        SceneHelper.ReloadScene();
    }


    private Vector2 GetPlayerStartingPosInLevel(LevelData ld) {
        // Respawning from death?
        if (!dm.playerGroundedRespawnPos.Equals(Vector2Extensions.NaN)) {
            return dm.playerGroundedRespawnPos;
        }
        // Starting at LevelDoor?
        else if (!string.IsNullOrEmpty(dm.levelToDoorID)) {
            return ld.GetLevelDoorPos(dm.levelToDoorID);
        }
        // Totally undefined? Default to PlayerStart.
        else {
            return ld.DefaultPlayerStartPos();
        }
    }
    
    private Vector2 GetPlayerStartingPosFromPrevExitPos(LevelData ld, int sideEntering, Vector2 posExited) {
        Vector2Int offsetDir = MathUtils.GetOppositeDir(sideEntering);
        const float extraEnterDistX = 0; // How much extra step do I wanna take in to really feel at "home"?
        const float extraEnterDistY = 3; // How much extra step do I wanna take in to really feel at "home"?
        Vector2 posRelative = posExited - ld.posGlobal; // Convert the last known coordinates to this level's coordinates.
        return posRelative + new Vector2(offsetDir.x*extraEnterDistX, offsetDir.y*extraEnterDistY);
    }


	private void OnPlayerEscapeLevelBounds(int sideEscaped) {
		WorldData currWorldData = level.WorldDataRef;
		LevelData nextLD = currWorldData.GetLevelAtSide(level.LevelDataRef, Player.PosLocal, sideEscaped);
		if (nextLD != null) {
            int sideEntering = Sides.GetOpposite(sideEscaped);
            Vector2 posExited = player.PosGlobal;
            
            PlayerData pd = player.SerializeAsData() as PlayerData; // Remember Player's physical properties (e.g. vel) so we can preserve 'em.
            pd.pos = GetPlayerStartingPosFromPrevExitPos(nextLD, sideEntering, posExited);
			StartGameAtLevel(nextLD, pd);
		}
		else {
			Debug.LogWarning("Whoa! No level at this side: " + sideEscaped);
		}
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		RegisterButtonInput();
	}
    
	private void RegisterButtonInput () {
        // Canvas has a selected element? Ignore ALL button input.
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
            return;
        }

		bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // ESCAPE = Toggle Pause!
        if (Input.GetKeyDown(KeyCode.Escape)) {
            gameTimeController.TogglePause();
		}

		// ~~~~ DEBUG ~~~~
        // Level-Jumping
		if (Input.GetKeyDown(KeyCode.Equals))            { Debug_JumpToLevelAtSide(Sides.T); return; }
		else if (Input.GetKeyDown(KeyCode.RightBracket)) { Debug_JumpToLevelAtSide(Sides.R); return; }
		else if (Input.GetKeyDown(KeyCode.Quote))        { Debug_JumpToLevelAtSide(Sides.B); return; }
		else if (Input.GetKeyDown(KeyCode.LeftBracket))  { Debug_JumpToLevelAtSide(Sides.L); return; }
        // Scene Changing
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R)) {
            SceneHelper.ReloadScene(); return;
        }
        else if (Input.GetKeyDown(KeyCode.J)) {
			SceneHelper.OpenScene(SceneNames.LevelJump); return;
		}
		else if (Input.GetKeyDown(KeyCode.M)) {
		    SceneHelper.OpenScene(SceneNames.MapEditor); return;
		}
        // T = Toggle Slow-mo
        else if (Input.GetKeyDown(KeyCode.T)) {
            gameTimeController.ToggleSlowMo();
		}
        // Y = Execute one FixedUpdate step
        else if (Input.GetKeyDown(KeyCode.Y)) {
            gameTimeController.ExecuteApproximatelyOneFUStep();
        }


		// ALT + ___
		if (isKey_alt) {
            // ALT + Q, W, E = Switch Characters
            if (Input.GetKeyDown(KeyCode.Q)) { SwapPlayerType(PlayerTypes.Plunga); }
            else if (Input.GetKeyDown(KeyCode.W)) { SwapPlayerType(PlayerTypes.Slippa); }
            else if (Input.GetKeyDown(KeyCode.E)) { SwapPlayerType(PlayerTypes.Jetta); }
        }
        // SHIFT + ___
        if (isKey_shift) {
            // SHIFT + S = Save level as text file!
            if (Input.GetKeyDown(KeyCode.S)) {
                SaveLevelFile();
            }
        }
        // CONTROL + ___
        if (isKey_control) {
            // CONTROL + DELETE = Clear all save data!
            if (Input.GetKeyDown(KeyCode.Delete)) {
                GameManagers.Instance.DataManager.ClearAllSaveData();
                SceneHelper.ReloadScene();
                return;
            }
            // CONTROL + N = Create/Start new level!
            else if (Input.GetKeyDown(KeyCode.N)) {
                StartNewBlankLevel();
            }
            // CONTROL + D = Duplicate/Start new level!
            else if (Input.GetKeyDown(KeyCode.D)) {
                DuplicateCurrLevel();
            }
            // CONTROL + SHIFT + X = Flip Horizontal!
            else if (isKey_shift && Input.GetKeyDown(KeyCode.X)) {
				if (level != null) { level.FlipHorz(); }
			}
		}
	}

    private void SaveLevelFile() {
        // Save it!
        LevelSaverLoader.SaveLevelFile(level);
        // Update properties that may have changed.
        level.WorldDataRef.UpdateNumSnacks();
    }



    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnPlayerDie(Player _player) {
        //playerDiedPos = _player.PosLocal;
        StartCoroutine(Coroutine_ReloadSceneDelayed());
	}
    private IEnumerator Coroutine_ReloadSceneDelayed() {
        yield return new WaitForSecondsRealtime(1f);
        SceneHelper.ReloadScene();
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






