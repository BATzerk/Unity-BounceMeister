using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphBody : PlayerBody {
	// Properties
	private readonly Color bodyColor_plunging = Color.green;
	private readonly Color bodyColor_plungeExhausted = Color.gray;
	// References
	private Alph myAlph;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		myAlph = myPlayer as Alph;
		bodyColor_neutral = new Color(25/255f, 175/255f, 181/255f);

		base.Start();
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnStartPlunge() {
		SetBodyColor(bodyColor_plunging);
	}
	public void OnStopPlunge() {
		if (myAlph.IsPlungeRecharged) {
			SetBodyColor(bodyColor_neutral);
		}
		else {
			SetBodyColor(bodyColor_plungeExhausted);
		}
	}

	public void OnRechargePlunge() {
		SetBodyColor(bodyColor_neutral);
	}


}
