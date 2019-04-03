using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JettaBody : PlayerBody {
	// Properties
	private readonly Color bodyColor_noFuel = Color.gray;
	private readonly Color bodyColor_jetting = Color.green;
	// References
	private Jetta myJetta;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		myJetta = myBasePlayer as Jetta;
		bodyColor_neutral = new ColorHSB(290/360f, 0.7f, 0.7f).ToColor();

		base.Start();
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	// TODO: Do this in an update loop
	// TODO: fill up the body with another sprite
	public void OnStartJet() {
		SetBodyColor(bodyColor_jetting);
	}
	public void OnStopJet() {
		if (myJetta.IsFuelEmpty) {
			SetBodyColor(bodyColor_noFuel);
		}
		else {
			SetBodyColor(bodyColor_neutral);
		}
	}

	public void OnRechargeJet() {
		SetBodyColor(bodyColor_neutral);
	}


}
