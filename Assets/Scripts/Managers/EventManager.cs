using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EventManager {
	// Actions and Event Variables
	public delegate void NoParamAction ();
	public delegate void FloatAction (float a);
	public delegate void FloatFloatAction (float a, float b);
	public delegate void IntAction (int a);
	public delegate void StringAction (string a);
	public delegate void PlayerAction (Player player);

	public event NoParamAction ScreenSizeChangedEvent;
	public event PlayerAction PlayerDashEvent;
	public event PlayerAction PlayerDashEndEvent;
	public event PlayerAction PlayerJumpEvent;

	// Program Events
	public void OnScreenSizeChanged () { if (ScreenSizeChangedEvent!=null) { ScreenSizeChangedEvent (); } }
	// Game Events
	public void OnPlayerDash(Player player) { if (PlayerDashEvent!=null) { PlayerDashEvent(player); } }
	public void OnPlayerDashEnd(Player player) { if (PlayerDashEndEvent!=null) { PlayerDashEndEvent(player); } }
	public void OnPlayerJump(Player player) { if (PlayerJumpEvent!=null) { PlayerJumpEvent(player); } }

}




