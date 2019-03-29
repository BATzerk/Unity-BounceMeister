using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
		eventManager.CoinsCollectedChangedEvent += OnCoinsCollectedChanged;
        eventManager.SetPausedEvent += OnSetPaused;
    }
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.CoinsCollectedChangedEvent -= OnCoinsCollectedChanged;
        eventManager.SetPausedEvent -= OnSetPaused;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSetPaused(bool val) {
        i_pausedBorder.enabled = val;
    }
    private void OnCoinsCollectedChanged() {
		UpdateCoinsCollectedText();
	}


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateCoinsCollectedText() {
		t_coinsCollected.text = dataManager.CoinsCollected.ToString();
	}


}



