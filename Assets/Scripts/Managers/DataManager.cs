using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager {
	// Properties
	private int coinsCollected; // the total value of all the coins we've collected!


	// ----------------------------------------------------------------
	//  Getters / Setters
	// ----------------------------------------------------------------
	public int CoinsCollected { get { return coinsCollected; } }
	public void ChangeCoinsCollected(int value) {
		SetCoinsCollected (coinsCollected + value);
	}
	public void SetCoinsCollected(int value) {
		coinsCollected = value;
		// Dispatch event!
		GameManagers.Instance.EventManager.OnCoinsCollectedChanged();
	}



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public DataManager() {
		Reset ();
	}
	private void Reset () {
		coinsCollected = 0;
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void ClearAllSaveData() {
		//		// What data do we wanna retain??
		//		int controllerType = GameManagers.Instance.InputManager.ControllerType;

		// NOOK IT
		SaveStorage.DeleteAll ();
		Reset ();
		Debug.Log ("All SaveStorage CLEARED!");

		//		// Pump back the data we retained!
		//		GameManagers.Instance.InputManager.SetControllerType (controllerType);
	}

}



