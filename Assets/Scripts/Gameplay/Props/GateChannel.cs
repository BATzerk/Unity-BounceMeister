using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateChannel {
	// Properties
    public bool IsUnlocked { get; private set; } // this value's saved/loaded!
    private bool doSaveIsUnlocked; // a bit weird: we don't want all GateChannels to save their locked-ness.
    private int channelID=-1;
	// References
	private Room myRoom;
	private List<Gate> myGates = new List<Gate>();
	private List<GateButton> myButtons = new List<GateButton>();

	public Color Color {
		get {
			switch (channelID) {
				case 0: return new ColorHSB(332/360f, 1.0f, 0.53f).ToColor();
				case 1: return new ColorHSB(297/360f, 1.0f, 0.53f).ToColor();
				case 2: return new ColorHSB(254/360f, 1.0f, 0.53f).ToColor();
				case 3: return new ColorHSB(211/360f, 1.0f, 0.53f).ToColor();
				case 4: return new ColorHSB(183/360f, 1.0f, 0.53f).ToColor();
				default: return Color.red;
			}
		}
	}
	private bool AreAllMyButtonsPressed() {
		foreach (GateButton button in myButtons) {
			if (button==null || button.ChannelID!=channelID) { continue; }
			if (!button.IsPressed) { return false; }
		}
		return true;
	}


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public GateChannel(Room myRoom, int channelID) {
		this.myRoom = myRoom;
		this.channelID = channelID;
        this.doSaveIsUnlocked = channelID < 3; // Hacky: We don't wanna save gate-unlockedness for Channels 3 and up.
        
        if (doSaveIsUnlocked) {
            IsUnlocked = SaveStorage.GetBool(SaveKeys.IsGateUnlocked(myRoom, channelID));
        }
	}

	//public void Reset() {
    //    SetIsUnlocked(false);
    //}
    private void SetIsUnlocked(bool val) {
        if (IsUnlocked != val) {
            IsUnlocked = val;
    		foreach (Gate gate in myGates) {
    			if (gate==null || gate.ChannelID!=channelID) { continue; }
    			gate.SetIsOn(!IsUnlocked);
    		}
        }
	}
    public void OnPlayerExitMyRoom() {
        // Save when the Player EXITS this Room.
        if (doSaveIsUnlocked) {
            SaveStorage.SetBool(SaveKeys.IsGateUnlocked(myRoom, channelID), IsUnlocked);
        }
    }
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
	public void AddGate(Gate _gate) {
		myGates.Add(_gate);
	}
	public void AddButton(GateButton _button) {
		myButtons.Add(_button);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnButtonPressed() {
        if (!IsUnlocked) { // I'm not yet unlocked...!
    		if (AreAllMyButtonsPressed()) { // All my buttons are pressed?? Unlock me!!
                SetIsUnlocked(true);
    		}
        }
	}



}
