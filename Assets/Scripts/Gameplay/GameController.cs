using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    // Components
    [SerializeField] private GameTimeController gameTimeController=null;
    // References
    [SerializeField] private Transform tf_world;
	private Player player=null;
	private Room room=null;
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
		// We haven't provided a room to play and this is Gameplay scene? Ok, load up the last played room instead!
		if (dm.currRoomData==null && SceneHelper.IsGameplayScene()) {
			int worldIndex = SaveStorage.GetInt(SaveKeys.LastPlayedWorldIndex);
			string roomKey = SaveStorage.GetString(SaveKeys.LastPlayedRoomKey(worldIndex), GameProperties.GetFirstRoomName(worldIndex));
			dm.currRoomData = dm.GetRoomData(worldIndex, roomKey, true);
		}

		// We've defined our currentRoomData before this scene! Load up THAT room!!
		if (dm.currRoomData != null) {
			StartGameAtRoom(dm.currRoomData);
		}
		// We have NOT provided any currentRoomData!...
		else {
            dm.currRoomData = dm.GetRoomData(0, "PremadeRoom", false);
			// Initialize the existing room as a premade room! So we can start editing/playing/saving it right outta the scene.
			// TEMP! For converting scenes into room text files.
			room = FindObjectOfType<Room>();
			if (room == null) {
				GameObject roomGO = GameObject.Find("Structure");
				if (roomGO==null) {
					roomGO = new GameObject();
					roomGO.transform.localPosition = Vector3.zero;
					roomGO.transform.localScale = Vector3.one;
				}
				roomGO.AddComponent<RoomGizmos>();
				room = roomGO.AddComponent<Room>();
			}
			if (tf_world == null) {
				tf_world = GameObject.Find("GameWorld").transform;
			}
//			player = GameObject.FindObjectOfType<Player>(); // Again, this is only for editing.
			MakePlayer(new PlayerData{type=PlayerTypes.Plunga});
			room.InitializeAsPremadeRoom(this);
			dm.SetCoinsCollected (0);
			eventManager.OnStartRoom(room);
		}

		// Add event listeners!
		eventManager.PlayerDieEvent += OnPlayerDie;
        eventManager.PlayerEscapeRoomBoundsEvent += OnPlayerEscapeRoomBounds;
    }
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.PlayerDieEvent -= OnPlayerDie;
		eventManager.PlayerEscapeRoomBoundsEvent -= OnPlayerEscapeRoomBounds;
    }


    // ----------------------------------------------------------------
    //  Doers - Loading Room
    // ----------------------------------------------------------------
    //public void StartGameAtRoom (int worldIndex, string roomKey) { StartGameAtRoom(dm.GetRoomData(worldIndex, roomKey, true)); }
    private void StartGameAtRoom(RoomData rd) {
        PlayerData playerData = new PlayerData {
            pos = GetPlayerStartingPosInRoom(rd),
            type = PlayerTypeHelper.LoadLastPlayedType(),
        };
        StartGameAtRoom(rd, playerData);
    }
    public void StartGameAtRoom(RoomData rd, PlayerData playerData) {
		// Wipe everything totally clean.
		DestroyPlayer();
		DestroyRoom();

		dm.currRoomData = rd;

		// Make Room and Player!
		room = Instantiate(ResourcesHandler.Instance.Room).GetComponent<Room>();
		room.Initialize(this, tf_world, rd);
        MakePlayer(playerData);
        // Tell the RoomData it's on!
        rd.OnPlayerEnterMe();

        // Reset things!
        dm.ResetRoomEnterValues();
        dm.SetCoinsCollected(0);
        
		// Save what's up!
		SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, rd.WorldIndex);
		SaveStorage.SetString(SaveKeys.LastPlayedRoomKey(rd.WorldIndex), rd.RoomKey);

		// Use this opportunity to call SAVE with SaveStorage, yo! (This causes a brief stutter, so I'm opting to call it when the game is already loading.)
		SaveStorage.Save();
		// Dispatch the post-function event!
		eventManager.OnStartRoom(room);

        // Expand the hierarchy for easier Room-editing!
        ExpandRoomHierarchy();
        GameUtils.SetEditorCameraPos(rd.posGlobal); // conveniently move the Unity Editor camera, too!
    }

    private void ExpandRoomHierarchy() {
        if (!GameUtils.IsEditorWindowMaximized()) { // If we're maximized, do nothing (we don't want to open up the Hierarchy if it's not already open).
            GameUtils.SetExpandedRecursive(room.gameObject, true); // Open up Room all the way down.
            for (int i=0; i<room.transform.childCount; i++) { // Ok, now (messily) close all its children.
                GameUtils.SetExpandedRecursive(room.transform.GetChild(i).gameObject, false);
            }
            GameUtils.FocusOnWindow("Game"); // focus back on Game window.
            //GameUtils.SetGOCollapsed(transform.parent, false);
            //GameUtils.SetGOCollapsed(tf_world, false);
            //GameUtils.SetGOCollapsed(room.transform, false);
        }
    }
    
    private void MakePlayer(PlayerTypes type, RoomData roomData) {
		Vector2 startingPos = GetPlayerStartingPosInRoom(roomData);
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
		player.Initialize(room, playerData);
        // Save lastPlayedType!
        PlayerTypeHelper.SaveLastPlayedType(player.PlayerType());
	}
    public void SwapPlayerType(PlayerTypes _type) {
        PlayerData playerData = player.SerializeAsData() as PlayerData;
        playerData.type = _type;
        MakePlayer(playerData);
        GameManagers.Instance.EventManager.OnSwapPlayerType();
    }


	private void DestroyRoom() {
		if (room != null) { Destroy(room.gameObject); }
		room = null;
	}
	private void DestroyPlayer() {
		if (player != null) { Destroy(player.gameObject); }
		player = null;
	}


    private Vector2 GetPlayerStartingPosInRoom(RoomData rd) {
        // Starting at RoomDoor?
        if (!string.IsNullOrEmpty(dm.roomToDoorID)) {
            return rd.GetRoomDoorPos(dm.roomToDoorID);// + new Vector2(0, -playerHeight*0.5f);
        }
        // Respawning from death?
        else if (!dm.playerGroundedRespawnPos.Equals(Vector2Extensions.NaN)) {
            return dm.playerGroundedRespawnPos;
        }
        // Totally undefined? Default to PlayerStart.
        else {
            return rd.DefaultPlayerStartPos();
        }
    }
    
    private Vector2 GetPlayerStartingPosFromPrevExitPos(RoomData rd, int sideEntering, Vector2 posExited) {
        //Vector2Int offsetDir = MathUtils.GetOppositeDir(sideEntering);
        //const float extraEnterDistX = 0; // How much extra step do I wanna take in to really feel at "home"?
        //const float extraEnterDistY = 3; // How much extra step do I wanna take in to really feel at "home"?
        //return posRelative + new Vector2(offsetDir.x*extraEnterDistX, offsetDir.y*extraEnterDistY);
        Vector2 posRelative = posExited - rd.posGlobal; // Convert the last known coordinates to this room's coordinates.
        Vector2 returnPos = posRelative;
        if (sideEntering == Sides.B) { returnPos += new Vector2(0, 3); } // Coming up from below? Start a few steps farther up into the room!
        return returnPos;
    }

	private void OnPlayerEscapeRoomBounds(int sideEscaped) {
        Player.EatEdiblesHolding(); // TEMP solution. Willdo: Bring Edibles between rooms.
		WorldData currWorldData = room.MyWorldData;
		RoomData nextLD = currWorldData.GetRoomAtSide(room.MyRoomData, Player.PosLocal, sideEscaped);
		if (nextLD != null) {
            int sideEntering = Sides.GetOpposite(sideEscaped);
            Vector2 posExited = player.PosGlobal;
            
            PlayerData pd = player.SerializeAsData() as PlayerData; // Remember Player's physical properties (e.g. vel) so we can preserve 'em.
            pd.pos = GetPlayerStartingPosFromPrevExitPos(nextLD, sideEntering, posExited);
			StartGameAtRoom(nextLD, pd);
		}
		else {
			Debug.LogWarning("Whoa! No room at this side: " + sideEscaped);
		}
	}
    

    private void StartNewBlankRoom() {
        // Keep it in the current world, and give it a unique name.
        WorldData worldData = dm.GetWorldData(room.WorldIndex);
        string roomKey = worldData.GetUnusedRoomKey();
        RoomData emptyRoomData = worldData.GetRoomData(roomKey, true);
        StartGameAtRoom(emptyRoomData);
    }
    private void DuplicateCurrRoom() {
        // Add a new room file, yo!
        RoomData currRD = room.MyRoomData;
        string newRoomKey = currRD.MyWorldData.GetUnusedRoomKey(currRD.RoomKey);
        RoomSaverLoader.SaveRoomFileAs(currRD, currRD.WorldIndex, newRoomKey);
        dm.ReloadWorldDatas();
        RoomData newLD = dm.GetRoomData(currRD.WorldIndex,newRoomKey, false);
        newLD.SetPosGlobal(newLD.posGlobal + new Vector2(15,-15)*GameProperties.UnitSize); // offset its position a bit.
        RoomSaverLoader.UpdateRoomPropertiesInRoomFile(newLD); // update file!
        dm.currRoomData = newLD;
        SceneHelper.ReloadScene();
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
		// ALT + ___
		if (isKey_alt) {
            // ALT + Q, W, E = Switch Characters
            if (Input.GetKeyDown(KeyCode.Q)) { SwapPlayerType(PlayerTypes.Plunga); }
            else if (Input.GetKeyDown(KeyCode.W)) { SwapPlayerType(PlayerTypes.Flatline); }
            else if (Input.GetKeyDown(KeyCode.E)) { SwapPlayerType(PlayerTypes.Slippa); }
            else if (Input.GetKeyDown(KeyCode.R)) { SwapPlayerType(PlayerTypes.Jetta); }
        }
        // SHIFT + ___
        if (isKey_shift) {
            // SHIFT + S = Save room as text file!
            if (Input.GetKeyDown(KeyCode.S)) {
                SaveRoomFile();
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
            // CONTROL + N = Create/Start new room!
            else if (Input.GetKeyDown(KeyCode.N)) {
                StartNewBlankRoom();
            }
            // CONTROL + D = Duplicate/Start new room!
            else if (Input.GetKeyDown(KeyCode.D)) {
                DuplicateCurrRoom();
            }
            // CONTROL + SHIFT + X = Flip Horizontal!
            else if (isKey_shift && Input.GetKeyDown(KeyCode.X)) {
				if (room != null) { room.FlipHorz(); }
			}
		}
        
        // NOTHING + _____
        if (!isKey_alt && !isKey_shift && !isKey_control) {
            // BACKSPACE = Clear current Room save data.
            if (Input.GetKeyDown(KeyCode.Backspace)) {
                dm.ClearRoomSaveData(room);
                SceneHelper.ReloadScene();
                return;
            }
            // Room-Jumping
            else if (Input.GetKeyDown(KeyCode.Equals))       { Debug_JumpToRoomAtSide(Sides.T); return; }
            else if (Input.GetKeyDown(KeyCode.RightBracket)) { Debug_JumpToRoomAtSide(Sides.R); return; }
            else if (Input.GetKeyDown(KeyCode.Quote))        { Debug_JumpToRoomAtSide(Sides.B); return; }
            else if (Input.GetKeyDown(KeyCode.LeftBracket))  { Debug_JumpToRoomAtSide(Sides.L); return; }
            // Scene Changing
            else if (Input.GetKeyDown(KeyCode.Return)) {
                SceneHelper.ReloadScene(); return;
            }
            else if (Input.GetKeyDown(KeyCode.J)) {
                SceneHelper.OpenScene(SceneNames.RoomJump); return;
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
        }
	}

    private void SaveRoomFile() {
        // Save it!
        RoomSaverLoader.SaveRoomFile(room);
        // Update properties that may have changed.
        if (room.MyClusterData != null) {
            room.MyClusterData.RefreshSnackCount();
        }
        // Update total edibles counts!
        dm.RefreshSnackCountGame();
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
    private void Debug_JumpToRoomAtSide(int side) {
        OnPlayerEscapeRoomBounds(side); // Pretend the player just exited in this direction.
        player.SetPosLocal(room.Debug_PlayerStartPosLocal()); // just put the player at the PlayerStart.
    }





}






