using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
	// Components
	[SerializeField] private Text t_coinsCollected;
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
	}
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.CoinsCollectedChangedEvent -= OnCoinsCollectedChanged;
	}



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnCoinsCollectedChanged() {
		UpdateCoinsCollectedText();
	}
	private void UpdateCoinsCollectedText() {
		t_coinsCollected.text = dataManager.CoinsCollected.ToString();
	}


}
