﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
    // Components
    [SerializeField] private GameTimeController gameTimeController=null;
    // References
    [SerializeField] private Transform tf_world=null;
	public Player Player { get; private set; }
	public Room CurrRoom { get; private set; }

    // Getters
    public GameTimeController GameTimeController { get { return gameTimeController; } }
    private CharLineup charLineup { get { return dm.CharLineup; } }
	private DataManager dm { get { return GameManagers.Instance.DataManager; } }
	private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }
    private bool CanCyclePlayerType() {
        if (!CurrRoom.Player.IsGrounded()) { return false; } // Not grounded? Can't cycle.
        return charLineup.CanCyclePlayerType(); // Ask CharLineup.
    }



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start () {
		// We haven't provided a room to play and this is Gameplay scene? Ok, load up the last played room instead!
		if (dm.currRoomData==null && SceneHelper.IsGameplayScene()) {
            RoomAddress address = dm.LastPlayedRoomAddress();
			dm.currRoomData = dm.GetRoomData(address, true);
		}

		// We've defined our currentRoomData before this scene! Load up THAT room!!
		if (dm.currRoomData != null) {
			StartGameAtRoom(dm.currRoomData);
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
    public void StartGameAtRoom(RoomData rd) {
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
		CurrRoom = Instantiate(ResourcesHandler.Instance.Room).GetComponent<Room>();
		CurrRoom.Initialize(this, tf_world, rd);
        MakePlayer(playerData);
        // Tell the RoomData it's on!
        rd.OnPlayerEnterMe();

        // Reset things!
        dm.ResetRoomEnterValues();
        dm.SetCoinsCollected(0);
        
		// Save what's up!
		SaveStorage.SetInt(SaveKeys.LastPlayedWorldIndex, rd.WorldIndex);
		SaveStorage.SetString(SaveKeys.LastPlayedRoomKey(rd.WorldIndex), rd.RoomKey);
        SaveStorage.SetFloat(SaveKeys.MapEditor_CameraPosX, rd.PosGlobal.x);
        SaveStorage.SetFloat(SaveKeys.MapEditor_CameraPosY, rd.PosGlobal.y);

		//// Use this opportunity to call SAVE with SaveStorage, yo! (This causes a brief stutter, so I'm opting to call it when the game is already loading.)
		//SaveStorage.Save();
		// Dispatch the post-function event!
		eventManager.OnStartRoom(CurrRoom);
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
		if (Player != null) { DestroyPlayer(); } // Just in case.
        // Make 'em!
        Player = Instantiate(ResourcesHandler.Instance.Player(playerData.type)).GetComponent<Player>();
		Player.Initialize(CurrRoom, playerData);
        // Save lastPlayedType!
        PlayerTypeHelper.SaveLastPlayedType(Player.PlayerType());
	}
    public void SwapPlayerType(PlayerTypes _type) {
        PlayerData playerData = Player.SerializeAsData() as PlayerData;
        playerData.type = _type;
        MakePlayer(playerData);
        GameManagers.Instance.EventManager.OnSwapPlayerType();
    }
    private void MaybeCyclePlayerType() {
        if (CanCyclePlayerType()) { // If we can...!
            PlayerTypes nextType = charLineup.GetNextPlayerType(CurrRoom.Player.PlayerType());
            SwapPlayerType(nextType);
        }
    }


	private void DestroyRoom() {
		if (CurrRoom != null) { Destroy(CurrRoom.gameObject); }
		CurrRoom = null;
	}
	private void DestroyPlayer() {
		if (Player != null) { Destroy(Player.gameObject); }
		Player = null;
	}


    private Vector2 GetPlayerStartingPosInRoom(RoomData rd) {
        // Starting at RoomDoor?
        if (!string.IsNullOrEmpty(dm.doorToID)) {
            return rd.GetRoomDoorPos(dm.doorToID);// + new Vector2(0, -playerHeight*0.5f);
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
        Vector2 posRelative = posExited - rd.PosGlobal; // Convert the last known coordinates to this room's coordinates.
        Vector2 returnPos = posRelative;
        if (sideEntering == Sides.B) { returnPos += new Vector2(0, 2); } // Coming up from below? Start a few steps farther up into the room!
        return returnPos;
    }

	private void OnPlayerEscapeRoomBounds(int sideEscaped) {
        // Exit current Room.
        OnPlayerExitRoom(CurrRoom);
        // Enter next Room.
		WorldData currWorldData = CurrRoom.MyWorldData;
		RoomData nextLD = currWorldData.GetRoomAtSide(CurrRoom.MyRoomData, Player.PosLocal, sideEscaped);
		if (nextLD != null) {
            int sideEntering = Sides.GetOpposite(sideEscaped);
            Vector2 posExited = Player.PosGlobal;
            
            PlayerData pd = Player.SerializeAsData() as PlayerData; // Remember Player's physical properties (e.g. vel) so we can preserve 'em.
            pd.pos = GetPlayerStartingPosFromPrevExitPos(nextLD, sideEntering, posExited);
            if (sideEntering == Sides.B) { // Entering from bottom?? Give min y-vel to boost us into the room!
                pd.vel = new Vector2(pd.vel.x, Mathf.Max(pd.vel.y, 0.6f));
            }
			StartGameAtRoom(nextLD, pd);
		}
		else {
			Debug.LogWarning("Whoa! No room at this side: " + sideEscaped);
		}
	}
    private void OnPlayerExitRoom(Room _room) {
        _room.OnPlayerExitMe();
        Player.EatEdiblesHolding(); // TEMP solution. Willdo: Bring Edibles between rooms.
    }
    

    
    public void OnPlayerTouchRoomDoor(RoomDoor rd) {
        //GoToMyRoom();TEMP TEST DISABLED RoomDoor functionality! Just open ClustSel for now!
        // Open ClustSel scene!
        OnPlayerExitRoom(rd.MyRoom);
        SceneHelper.OpenScene(SceneNames.ClustSelect);
    }
    private void GoToRoomDoorRoom(RoomDoor rd) {
        // Register exiting the Room!
        OnPlayerExitRoom(rd.MyRoom);
        // Set the door we're gonna start at!
        dm.doorToID = rd.DoorToID;
        // Load the room!
        int _worldIndex = rd.WorldToIndex==-1 ? rd.MyRoom.WorldIndex : rd.WorldToIndex; // Haven't defined worldToIndex? Stay in my world.
        RoomData rdTo = dm.GetRoomData(_worldIndex, rd.RoomToKey, false);
        if (rdTo == null) { // Safety check.
            Debug.LogWarning("RoomDoor can't go to RoomData; doesn't exist. World: " + _worldIndex + ", RoomKey: " + rd.RoomToKey);
        }
        else { // There IS a room to go to! Go!
            StartGameAtRoom(rdTo);
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

        // ESCAPE or P = Toggle Pause!
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) {
            gameTimeController.TogglePause();
        }
        
        // Cycle PlayerType!
        if (InputController.Instance.IsCycleChar_Press) {
            MaybeCyclePlayerType();
        }

		// ~~~~ DEBUG ~~~~
		// ALT + ___
		if (isKey_alt) {
            // ALT + __ = Switch Characters
            if (Input.GetKeyDown(KeyCode.C)) { SwapPlayerType(PlayerTypes.Clinga); }
            else if (Input.GetKeyDown(KeyCode.D)) { SwapPlayerType(PlayerTypes.Dilata); }
            else if (Input.GetKeyDown(KeyCode.F)) { SwapPlayerType(PlayerTypes.Flatline); }
            else if (Input.GetKeyDown(KeyCode.I)) { SwapPlayerType(PlayerTypes.Flippa); }
            else if (Input.GetKeyDown(KeyCode.J)) { SwapPlayerType(PlayerTypes.Jetta); }
            else if (Input.GetKeyDown(KeyCode.U)) { SwapPlayerType(PlayerTypes.Jumpa); }
            else if (Input.GetKeyDown(KeyCode.P)) { SwapPlayerType(PlayerTypes.Plunga); }
            else if (Input.GetKeyDown(KeyCode.S)) { SwapPlayerType(PlayerTypes.Slippa); }
            else if (Input.GetKeyDown(KeyCode.W)) { SwapPlayerType(PlayerTypes.Warpa); }
        }
        // SHIFT + ___
        if (isKey_shift) {
        }
        // CONTROL + ___
        if (isKey_control) {
            // CONTROL + DELETE = Clear ALL save data!
            if (Input.GetKeyDown(KeyCode.Delete)) {
                GameManagers.Instance.DataManager.ClearAllSaveData();
                SceneHelper.ReloadScene();
                return;
            }
		}
        
        // NOTHING + _____
        if (!isKey_alt && !isKey_shift && !isKey_control) {
            // BACKSPACE = Clear current Room save data.
            if (Input.GetKeyDown(KeyCode.Backspace)) {
                dm.ClearRoomSaveData(CurrRoom);
                SceneHelper.ReloadScene();
                return;
            }
            // Room-Jumping
            else if (Input.GetKeyDown(KeyCode.Equals))       { Debug_JumpToRoomAtSide(Sides.T); return; }
            else if (Input.GetKeyDown(KeyCode.RightBracket)) { Debug_JumpToRoomAtSide(Sides.R); return; }
            else if (Input.GetKeyDown(KeyCode.Quote))        { Debug_JumpToRoomAtSide(Sides.B); return; }
            else if (Input.GetKeyDown(KeyCode.LeftBracket))  { Debug_JumpToRoomAtSide(Sides.L); return; }
            // Scene Changing
            else if (Input.GetKeyDown(KeyCode.Return)) { SceneHelper.ReloadScene(); return; }
            else if (Input.GetKeyDown(KeyCode.C)) { SceneHelper.OpenScene(SceneNames.ClustSelect); return; }
            else if (Input.GetKeyDown(KeyCode.M)) { SceneHelper.OpenScene(SceneNames.MapEditor); return; }
            else if (Input.GetKeyDown(KeyCode.J)) { SceneHelper.OpenScene(SceneNames.RoomJump); return; }
            // F = Start/Stop Fast-mo
            else if (Input.GetKeyDown(KeyCode.F)) { gameTimeController.SetIsFastMo(true); }
            else if (Input.GetKeyUp  (KeyCode.F)) { gameTimeController.SetIsFastMo(false); }
            // T = Toggle Slow-mo
            else if (Input.GetKeyDown(KeyCode.T)) { gameTimeController.ToggleSlowMo(); }
            // Y = Execute one FixedUpdate step
            else if (Input.GetKeyDown(KeyCode.Y)) { gameTimeController.ExecuteApproximatelyOneFUStep(); }
        }
	}



    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnPlayerDie(Player _player) {
        StartCoroutine(Coroutine_ReloadSceneDelayed());
	}
    private IEnumerator Coroutine_ReloadSceneDelayed() {
        yield return new WaitForSeconds(1f);//Realtime
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
        Player.SetPosLocal(CurrRoom.Debug_PlayerStartPosLocal()); // just put the player at the PlayerStart.
        Player.SetVel(Vector2.zero);
    }

    private void OnGUI() {
        GUI.color = Color.black;
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        GUI.Label(new Rect(8,0, 400,100), "timeScale: " + Time.timeScale, style);
    }





}

//        // We have NOT provided any currentRoomData!...
//        else {
//            dm.currRoomData = dm.GetRoomData(0, "PremadeRoom", false);
//            // Initialize the existing room as a premade room! So we can start editing/playing/saving it right outta the scene.
//            // TEMP! For converting scenes into room text files.
//            currRoom = FindObjectOfType<Room>();
//            if (currRoom == null) {
//                GameObject roomGO = GameObject.Find("Structure");
//                if (roomGO==null) {
//                    roomGO = new GameObject();
//                    roomGO.transform.localPosition = Vector3.zero;
//                    roomGO.transform.localScale = Vector3.one;
//                }
//                roomGO.AddComponent<RoomGizmos>();
//                currRoom = roomGO.AddComponent<Room>();
//            }
//            if (tf_world == null) {
//                tf_world = GameObject.Find("GameWorld").transform;
//            }
////          player = GameObject.FindObjectOfType<Player>(); // Again, this is only for editing.
        //    MakePlayer(new PlayerData{type=PlayerTypes.Plunga});
        //    currRoom.InitializeAsPremadeRoom(this);
        //    dm.SetCoinsCollected (0);
        //    eventManager.OnStartRoom(currRoom);
        //}





