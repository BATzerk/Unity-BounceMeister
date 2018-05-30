using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	// Properties
	private bool isPaused = false;
	private bool debug_isSlowMo = false;
	// Components
	[SerializeField] private GameObject go_structure; // the physical level layout

	// Getters / Setters
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }
	private InputController inputController { get { return InputController.Instance; } }



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start () {
		dataManager.SetCoinsCollected (0); // Reset this.
//		ResetLevel ();

		// Add event listeners!
//		eventManager.CoinCollectedEvent += OnCoinCollected;
	}
	private void OnDestroy() {
		// Remove event listeners!
//		eventManager.CoinCollectedEvent -= OnCoinCollected;
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
		RegisterButtonInput ();
	}

	private void RegisterButtonInput () {
		bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		// Game Flow
		if (Input.anyKeyDown) {
//			if (gameState==GameStates.PostGame && Time.unscaledTime>unscaledTimeSinceGameEnded+2f) { // 2 second delay to start again.
//				ReloadScene();
//			}
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







}






