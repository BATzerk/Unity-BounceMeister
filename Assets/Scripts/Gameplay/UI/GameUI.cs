using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour {
    // Components
    //[SerializeField] private GameObject go_snacksCollected=null;
    [SerializeField] private GameObject go_infoSignText=null;
    [SerializeField] private Image i_pausedBorder=null;
    [SerializeField] private Text t_coinsCollected=null;
    [SerializeField] private TextMeshProUGUI t_infoSignText=null;
    //[SerializeField] private TextMeshProUGUI t_snacksCollected=null;
    // References
    //private Room currRoom;

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
        eventManager.NumSnacksEatenChangedEvent += OnNumSnacksEatenChanged;
        eventManager.PlayerTouchEnterInfoSignEvent += OnPlayerTouchEnterInfoSign;
        eventManager.PlayerTouchExitInfoSignEvent += OnPlayerTouchExitInfoSign;
        eventManager.SetPausedEvent += OnSetPaused;
        eventManager.StartRoomEvent += OnStartRoom;
    }
	private void OnDestroy() {
		// Remove event listeners!
		eventManager.CoinsCollectedChangedEvent -= OnCoinsCollectedChanged;
        eventManager.NumSnacksEatenChangedEvent -= OnNumSnacksEatenChanged;
        eventManager.PlayerTouchEnterInfoSignEvent -= OnPlayerTouchEnterInfoSign;
        eventManager.PlayerTouchExitInfoSignEvent -= OnPlayerTouchExitInfoSign;
        eventManager.SetPausedEvent -= OnSetPaused;
        eventManager.StartRoomEvent -= OnStartRoom;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnStartRoom(Room room) {
        //currRoom = room;
        UpdateCoinsCollectedText();
        UpdateSnacksTexts();
    }
    private void OnSetPaused(bool val) {
        i_pausedBorder.enabled = val;
    }
    private void OnCoinsCollectedChanged() {
        UpdateCoinsCollectedText();
    }
    private void OnNumSnacksEatenChanged() {
        UpdateSnacksTexts();
    }
    private void OnPlayerTouchEnterInfoSign(InfoSign infoSign) {
        go_infoSignText.SetActive(true);
        t_infoSignText.text = infoSign.MyText;
    }
    private void OnPlayerTouchExitInfoSign(InfoSign infoSign) {
        go_infoSignText.SetActive(false);
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateCoinsCollectedText() {
        t_coinsCollected.text = dm.CoinsCollected.ToString();
    }
    private void UpdateSnacksTexts() {
        //int numCollected = currRoom.MyWorldData.NumSnacksCollected;
        //int numTotal = currRoom.MyWorldData.NumSnacksTotal;
        // DISABLED snacksCollected text.
        //go_snacksCollected.SetActive(numTotal > 0); // Only show SnacksCollected if there ARE any Snacks in this World.
        //t_snacksCollected.text = numCollected + " / " + numTotal;
    }


}



