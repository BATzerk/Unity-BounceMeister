using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlungaBody : PlayerBody {
	// Properties
	private readonly Color bodyColor_plunging = Color.green;
	private readonly Color bodyColor_plungeExhausted = Color.gray;
	// References
	private Plunga myPlunga;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
        myPlunga = myBasePlayer as Plunga;
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
		if (myPlunga.IsPlungeRecharged) {
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
