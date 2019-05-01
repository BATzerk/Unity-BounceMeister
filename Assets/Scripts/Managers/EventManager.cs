using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EventManager {
	// Actions and Event Variables
	public delegate void NoParamAction ();
	public delegate void BoolAction(bool b);
	public delegate void CoinAction(Coin coin);
	public delegate void FloatAction (float a);
	public delegate void FloatFloatAction (float a, float b);
	public delegate void IntAction (int a);
	public delegate void RoomAction(Room room);
	public delegate void PlayerAction (Player player);
    public delegate void InfoSignAction(InfoSign infoSign);
	public delegate void StringAction (string a);

	public event NoParamAction EditorSaveRoomEvent;
	public event NoParamAction ScreenSizeChangedEvent;
	public event NoParamAction CoinsCollectedChangedEvent;
    public event NoParamAction PlayerUseBatteryEvent;
    public event NoParamAction NumSnacksEatenChangedEvent;
    public event BoolAction SetIsEditModeEvent;
    public event BoolAction SetPausedEvent;
    public event CoinAction CoinCollectedEvent;
    public event RoomAction StartRoomEvent;
    public event IntAction PlayerEscapeRoomBoundsEvent;
    public event IntAction MapEditorSetCurrWorldEvent;
    public event PlayerAction PlayerDieEvent;
    public event PlayerAction PlayerInitEvent;
    public event PlayerAction PlayerJumpEvent;
    public event PlayerAction PlayerStartHoverEvent;
//	public event PlayerAction PlayerSpendPlungeEvent;
	public event PlayerAction PlayerStartPlungeEvent;
	public event PlayerAction PlayerRechargePlungeEvent;
	public event PlayerAction PlayerWallKickEvent;
    public event InfoSignAction PlayerTouchEnterInfoSignEvent;
    public event InfoSignAction PlayerTouchExitInfoSignEvent;

	// Program Events
	public void OnScreenSizeChanged () { if (ScreenSizeChangedEvent!=null) { ScreenSizeChangedEvent (); } }
	// Game Events
    public void OnMapEditorSetCurrWorld(int worldIndex) { if (MapEditorSetCurrWorldEvent!=null) { MapEditorSetCurrWorldEvent(worldIndex); } }
    //public void OnMapEditorSetCurrWorldIndex(int worldIndex) { MapEditorSetCurrWorldIndexEvent?.Invoke(worldIndex); }
    
	public void OnEditorSaveRoom() { if (EditorSaveRoomEvent!=null) { EditorSaveRoomEvent(); } }
    public void OnSetIsEditMode(bool isEditMode) { if (SetIsEditModeEvent!=null) { SetIsEditModeEvent(isEditMode); } }
    public void OnSetPaused(bool isPaused) { if (SetPausedEvent!=null) { SetPausedEvent(isPaused); } }
    public void OnStartRoom(Room room) { if (StartRoomEvent!=null) { StartRoomEvent(room); } }

	public void OnCoinCollected(Coin coin) { if (CoinCollectedEvent!=null) { CoinCollectedEvent(coin); } }
	public void OnCoinsCollectedChanged() { if (CoinsCollectedChangedEvent!=null) { CoinsCollectedChangedEvent(); } }
    public void OnNumSnacksEatenChanged() { if (NumSnacksEatenChangedEvent!=null) { NumSnacksEatenChangedEvent(); } }

    public void OnPlayerEscapeRoomBounds(int side) { if (PlayerEscapeRoomBoundsEvent!=null) { PlayerEscapeRoomBoundsEvent(side); } }
	public void OnPlayerDie(Player player) { if (PlayerDieEvent!=null) { PlayerDieEvent(player); } }
    public void OnPlayerInit(Player player) { if (PlayerInitEvent!=null) { PlayerInitEvent(player); } }
    public void OnPlayerJump(Player player) { if (PlayerJumpEvent!=null) { PlayerJumpEvent(player); } }
    public void OnPlayerUseBattery() { if (PlayerUseBatteryEvent!=null) { PlayerUseBatteryEvent(); } }
    public void OnPlayerStartHover(Player player) { if (PlayerStartHoverEvent!=null) { PlayerStartHoverEvent(player); } }
//	public void OnPlayerSpendBounce(Player player) { if (PlayerSpendPlungeEvent!=null) { PlayerSpendPlungeEvent(player); } }
	public void OnPlayerStartPlunge(Player player) { if (PlayerStartPlungeEvent!=null) { PlayerStartPlungeEvent(player); } }
    public void OnPlayerTouchEnterInfoSign(InfoSign infoSign) { if (PlayerTouchEnterInfoSignEvent!=null) { PlayerTouchEnterInfoSignEvent(infoSign); } }
    public void OnPlayerTouchExitInfoSign(InfoSign infoSign) { if (PlayerTouchExitInfoSignEvent!=null) { PlayerTouchExitInfoSignEvent(infoSign); } }
	public void OnPlayerRechargePlunge(Player player) { if (PlayerRechargePlungeEvent!=null) { PlayerRechargePlungeEvent(player); } }
	public void OnPlayerWallKick(Player player) { if (PlayerWallKickEvent!=null) { PlayerWallKickEvent(player); } }


}




