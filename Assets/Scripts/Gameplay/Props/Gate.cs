using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Gate : BaseGround, ISerializableData<GateData> {
	// Properties
	[SerializeField] private int channelID;
	private Color bodyColor=Color.red;

	// Getters (Public)
	public int ChannelID { get { return channelID; } }
	// Getters (Private)
	private GateChannel MyChannel { get { return myLevel.GateChannels[channelID]; } }



	// ----------------------------------------------------------------
	//  Gizmos
	// ----------------------------------------------------------------
//	private void OnDrawGizmos() {
//		if (myButtons==null) { return; }
//		Gizmos.color = Color.Lerp(Color.white, bodyColor, 0.5f);
//		foreach (GateButton button in myButtons) {
//			if (button==null) { continue; }
//			Gizmos.DrawLine(this.transform.position, button.transform.position);
//		}
//	}


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, GateData data) {
		base.BaseGroundInitialize(_myLevel, data);

		channelID = data.channelID;
		bodyColor = MyChannel.Color;
		SetIsOn(true);
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void UnlockMe() {
		SetIsOn(false);
		//todo: some animation or something, I guess
	}
	public void SetIsOn(bool _isOn) {
		myCollider.enabled = _isOn;
		if (_isOn) {
			bodySprite.color = bodyColor;
		}
		else {
			bodySprite.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, 0.1f);
		}
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public GateData SerializeAsData() {
		GateData data = new GateData();
		data.myRect = MyRect();
		data.canEatGems = CanEatEdibles;
        data.isPlayerRespawn = IsPlayerRespawn;
        data.channelID = channelID;
		return data;
	}

}
