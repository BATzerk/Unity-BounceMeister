using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour {
    // Components
    [SerializeField] private GameObject go_snacksCollected=null;
    [SerializeField] private Image i_pausedBorder=null;
    [SerializeField] private Text t_coinsCollected=null;
    [SerializeField] private TextMeshProUGUI t_snacksCollected=null;
    // References
    private Level currLevel;

    // Getters
    private DataManager dm { get { return GameManagers.Instance.DataManager; } }
	private EventManager eventManager { get { return GameManagers.Instance.EventManager; } }
    //private int currWorldIndex { get { return dm.currentWorldIndex; } }


    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake () {
        // Add event listeners!
        eventManager.CoinsCollectedChangedEvent += OnCoinsCollectedChanged;
        eventManager.SnacksCollectedChangedEvent += OnSnacksCollectedChanged;
        eventManager.SetPausedEvent += OnSetPaused;
        eventManager.StartLevelEvent += OnStartLevel;
    }
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.CoinsCollectedChangedEvent -= OnCoinsCollectedChanged;
        eventManager.SnacksCollectedChangedEvent -= OnSnacksCollectedChanged;
        eventManager.SetPausedEvent -= OnSetPaused;
        eventManager.StartLevelEvent -= OnStartLevel;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnStartLevel(Level level) {
        currLevel = level;
        UpdateCoinsCollectedText();
        UpdateSnacksCollectedText();
    }
    private void OnSetPaused(bool val) {
        i_pausedBorder.enabled = val;
    }
    private void OnCoinsCollectedChanged() {
        UpdateCoinsCollectedText();
    }
    private void OnSnacksCollectedChanged(int worldIndex) {
        UpdateSnacksCollectedText();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateCoinsCollectedText() {
        t_coinsCollected.text = dm.CoinsCollected.ToString();
    }
    private void UpdateSnacksCollectedText() {
        int numCollected = currLevel.WorldDataRef.NumSnacksCollected;
        int numTotal = currLevel.WorldDataRef.NumSnacksTotal;
        go_snacksCollected.SetActive(numTotal > 0); // Only show SnacksCollected if there ARE any Snacks in this World.
        t_snacksCollected.text = numCollected + " / " + numTotal;
    }


}



