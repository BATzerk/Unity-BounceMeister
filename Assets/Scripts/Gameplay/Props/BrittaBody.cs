using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrittaBody : PlayerBody {
//	// Properties
//	private readonly Color bodyColor_plunging = Color.green;
//	private readonly Color bodyColor_plungeExhausted = Color.gray;
	// References
//	private Britta myBritta;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
//		myBritta = myPlayer as Britta;
		bodyColor_neutral = new Color(185/255f, 125/255f, 25/255f);

		base.Start();
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
//	public void OnStartPlunge() {
//		SetBodyColor(bodyColor_plunging);
//	}
//	public void OnStopPlunge() {
//		if (myAlph.IsPlungeRecharged) {
//			SetBodyColor(bodyColor_neutral);
//		}
//		else {
//			SetBodyColor(bodyColor_plungeExhausted);
//		}
//	}
//
//	public void OnRechargePlunge() {
//		SetBodyColor(bodyColor_neutral);
//	}


}
