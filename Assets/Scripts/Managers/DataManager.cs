using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager {
	// Properties


	// ----------------------------------------------------------------
	//  Getters
	// ----------------------------------------------------------------



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public DataManager() {
		Reset ();
	}
	private void Reset () {
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



