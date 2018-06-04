using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	// Properties
	private bool isPaused = false;
	private bool debug_isSlowMo = false;
	// References
	[SerializeField] private Player player=null;

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
		// Nbd for now.
		player = GameObject.FindObjectOfType<Player>();

		// Reset things!
		dataManager.SetCoinsCollected (0);
		UpdateTimeScale();
		ResetPlayerAtLevelDoor(dataManager.levelToDoorID);
		eventManager.OnStartLevel();

		// Add event listeners!
		eventManager.PlayerDieEvent += OnPlayerDie;
	}
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.PlayerDieEvent -= OnPlayerDie;
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
	private void ResetPlayerAtLevelDoor(string levelDoorID) {
		LevelDoor[] allDoors = GameObject.FindObjectsOfType<LevelDoor>();
		LevelDoor correctDoor = null; // I'll specify next.
		foreach (LevelDoor door in allDoors) {
			if (door.MyID == levelDoorID) {
				correctDoor = door;
				break;
			}
		}
		if (correctDoor != null) {
			player.SetPos(correctDoor.Pos);
		}
		else {
			Debug.LogWarning("Oops! Couldn't find a door with this ID: " + levelDoorID);
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
			player.SetPos(mousePosWorld());
		}
	}
	private void RegisterButtonInput () {
		bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		// Game Flow
		if (Input.GetKeyDown(KeyCode.L)) {
			OpenScene(SceneNames.LevelSelect);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			TogglePause();
		}

		// ~~~~ DEBUG ~~~~
		if (Input.GetKeyDown(KeyCode.Return)) {
			ReloadScene();
			return;
		}
		else if (Input.GetKeyDown(KeyCode.T)) {
			debug_isSlowMo = !debug_isSlowMo;
			UpdateTimeScale();
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
	//  Events
	// ----------------------------------------------------------------
	private void OnPlayerDie(Player player) {
		Invoke("ReloadScene", 1f);
	}






}






