using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour {
	// Components
	[SerializeField] private Image i_pausedBorder=null;
	[SerializeField] private Text t_coinsCollected=null;
	[SerializeField] private TextMeshProUGUI t_levelName=null;
	// References
//	[SerializeField] private GameController gameControllerRef;

	// Getters
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }


	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start () {
		UpdateCoinsCollectedText();

		// Add event listeners!
		eventManager.CoinsCollectedChangedEvent += OnCoinsCollectedChanged;
		eventManager.SetPausedEvent += OnSetPaused;
        eventManager.StartLevelEvent += OnStartLevel;
    }
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.CoinsCollectedChangedEvent -= OnCoinsCollectedChanged;
		eventManager.SetPausedEvent -= OnSetPaused;
        eventManager.StartLevelEvent -= OnStartLevel;
    }


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnSetPaused(bool isPaused) {
		i_pausedBorder.enabled = isPaused;
	}
	private void OnCoinsCollectedChanged() {
		UpdateCoinsCollectedText();
	}
    private void OnStartLevel(Level level) {
        t_levelName.text = level.LevelKey;
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateCoinsCollectedText() {
		t_coinsCollected.text = dataManager.CoinsCollected.ToString();
	}


}



