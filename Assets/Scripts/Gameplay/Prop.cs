using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour {
	// References
	protected Level myLevel;

	virtual protected void BaseInitialize (Level myLevel) {
		this.myLevel = myLevel;
		this.transform.SetParent (myLevel.transform);
		this.transform.localScale = Vector3.one;
	}

}
