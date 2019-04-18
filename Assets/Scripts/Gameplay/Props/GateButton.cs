using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GateButton : Prop {
	// Components
//	[SerializeField] private Collider2D myCollider;
	[SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	[SerializeField] private int channelID;
	private Color bodyColor=Color.red;
	private bool isPressed;

	// Getters (Public)
	public bool IsPressed { get { return isPressed; } }
	public int ChannelID { get { return channelID; } }
	// Getters (Private)
	private GateChannel MyChannel { get { return myLevel.GateChannels[channelID]; } }



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, GateButtonData data) {
		base.BaseInitialize(_myLevel, data);

		channelID = data.channelID;
		bodyColor = MyChannel.Color;
		sr_body.color = bodyColor;
        SetIsPressed(MyChannel.IsUnlocked);
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
    private void SetIsPressed(bool val) {
        isPressed = val;
        GameUtils.SetSpriteColor(sr_body, bodyColor, isPressed ? 0.1f : 1);
    }
	private void GetPressed() {
        SetIsPressed(true);
		if (MyChannel != null) {
			MyChannel.OnButtonPressed();
		}
	}



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnTriggerEnter2D(Collider2D otherCol) {
		// Ground??
		if (LayerMask.LayerToName(otherCol.gameObject.layer) == Layers.Player) {
			OnPlayerTouchMe(otherCol.GetComponentInChildren<Player>());
		}
	}
	private void OnPlayerTouchMe(Player player) {
		if (player==null) { Debug.LogError("Uh-oh! Calling OnPlayerTouchMe with a null Player. Hmm."); return; }

		GetPressed();
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
		GateButtonData data = new GateButtonData();
		data.pos = pos;
		data.channelID = channelID;
		return data;
	}

}
