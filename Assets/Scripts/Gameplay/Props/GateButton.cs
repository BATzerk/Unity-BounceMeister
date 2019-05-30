using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GateButton : Prop {
	// Components
    [SerializeField] private SpriteRenderer sr_aura=null;
    [SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	[SerializeField] private int channelID;
	private Color bodyColor=Color.red;
	private bool isPressed;

	// Getters (Public)
	public bool IsPressed { get { return isPressed; } }
	public int ChannelID { get { return channelID; } }
	// Getters (Private)
	private GateChannel MyChannel { get { return MyRoom.GateChannels[channelID]; } }



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, GateButtonData data) {
		base.BaseInitialize(_myRoom, data);
        
		channelID = data.channelID;
		bodyColor = MyChannel.Color;
		sr_body.color = bodyColor;
        sr_aura.color = Color.Lerp(bodyColor, Color.white, 0.3f);
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


    private void Update() {
        float auraAlpha = isPressed ? 0.1f : MathUtils.SinRange(0.3f,0.5f, Time.time*4-(pos.x+pos.y)*0.2f);
        GameUtils.SetSpriteAlpha(sr_aura, auraAlpha);
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
        GateButtonData data = new GateButtonData {
            pos = pos,
            channelID = channelID
        };
        return data;
	}

}
