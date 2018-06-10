using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
	// Components
	[SerializeField] private Image i_pausedBorder=null;
	[SerializeField] private Text t_coinsCollected=null;
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
		eventManager.SetPausedEvent += OnSetPaused;
		eventManager.CoinsCollectedChangedEvent += OnCoinsCollectedChanged;
	}
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.SetPausedEvent -= OnSetPaused;
		eventManager.CoinsCollectedChangedEvent -= OnCoinsCollectedChanged;
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
	private void UpdateCoinsCollectedText() {
		t_coinsCollected.text = dataManager.CoinsCollected.ToString();
	}


}
