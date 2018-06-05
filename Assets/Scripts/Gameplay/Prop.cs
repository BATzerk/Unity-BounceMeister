using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour {
	// References
	protected Level myLevel;

	// Getters
	public Vector2 Pos { get { return pos; } }
	protected Vector2 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}

	virtual protected void BaseInitialize (Level myLevel, PropData data) {
		this.myLevel = myLevel;
		this.transform.SetParent (myLevel.transform);
		this.transform.localScale = Vector3.one;
		this.transform.localPosition = data.pos; // note that this is just a convenience default. Any grounds will set their pos from their rect.
	}

}
