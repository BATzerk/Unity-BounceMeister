using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EventManager {
	// Actions and Event Variables
	public delegate void NoParamAction ();
	public delegate void CoinAction(Coin coin);
	public delegate void FloatAction (float a);
	public delegate void FloatFloatAction (float a, float b);
	public delegate void IntAction (int a);
	public delegate void StringAction (string a);
	public delegate void PlayerAction (Player player);

	public event NoParamAction ScreenSizeChangedEvent;
	public event NoParamAction StartLevelEvent;
	public event NoParamAction CoinsCollectedChangedEvent;
	public event CoinAction CoinCollectedEvent;
	public event PlayerAction PlayerDashEvent;
	public event PlayerAction PlayerDashEndEvent;
	public event PlayerAction PlayerDieEvent;
	public event PlayerAction PlayerJumpEvent;
//	public event PlayerAction PlayerSpendPlungeEvent;
	public event PlayerAction PlayerStartPlungeEvent;
	public event PlayerAction PlayerRechargePlungeEvent;
	public event PlayerAction PlayerWallKickEvent;

	// Program Events
	public void OnScreenSizeChanged () { if (ScreenSizeChangedEvent!=null) { ScreenSizeChangedEvent (); } }
	// Game Events
	public void OnStartLevel() { if (StartLevelEvent!=null) { StartLevelEvent(); } }

	public void OnCoinCollected(Coin coin) { if (CoinCollectedEvent!=null) { CoinCollectedEvent(coin); } }
	public void OnCoinsCollectedChanged() { if (CoinsCollectedChangedEvent!=null) { CoinsCollectedChangedEvent(); } }

	public void OnPlayerDash(Player player) { if (PlayerDashEvent!=null) { PlayerDashEvent(player); } }
	public void OnPlayerDashEnd(Player player) { if (PlayerDashEndEvent!=null) { PlayerDashEndEvent(player); } }
	public void OnPlayerDie(Player player) { if (PlayerDieEvent!=null) { PlayerDieEvent(player); } }
	public void OnPlayerJump(Player player) { if (PlayerJumpEvent!=null) { PlayerJumpEvent(player); } }
//	public void OnPlayerSpendBounce(Player player) { if (PlayerSpendPlungeEvent!=null) { PlayerSpendPlungeEvent(player); } }
	public void OnPlayerStartPlunge(Player player) { if (PlayerStartPlungeEvent!=null) { PlayerStartPlungeEvent(player); } }
	public void OnPlayerRechargePlunge(Player player) { if (PlayerRechargePlungeEvent!=null) { PlayerRechargePlungeEvent(player); } }
	public void OnPlayerWallKick(Player player) { if (PlayerWallKickEvent!=null) { PlayerWallKickEvent(player); } }


}




