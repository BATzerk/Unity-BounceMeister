using System.Collections;
using UnityEngine;

/** Put persistent-across-program stuff as a child in this GameObject and it'll persist across scenes! */
public class PersistentHouse : MonoBehaviour {
	// There can only be one.
	private static PersistentHouse instance;

	private void Awake () {
		// T... two??
		if (instance != null) {
			// THERE CAN ONLY BE ONE.
			DestroyImmediate (this.gameObject);
			return;
		}

		// There could only be one. :)
		instance = this;
		DontDestroyOnLoad (this.gameObject);
	}
}
