using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GateButton : Prop, ISerializableData<GateButtonData> {
	// Components
//	[SerializeField] private Collider2D myCollider;
	[SerializeField] private SpriteRenderer sr_body;
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
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GetPressed() {
		isPressed = true;
		sr_body.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, 0.1f);
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
	public GateButtonData SerializeAsData() {
		GateButtonData data = new GateButtonData();
		data.pos = pos;
		data.channelID = channelID;
		return data;
	}

}
