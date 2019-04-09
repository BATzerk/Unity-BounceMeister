using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Prop : MonoBehaviour {
    // Overrideables
    virtual public bool DoSaveInLevelFile() { return true; } // by default, ALL Props wanna get saved into the Level text file. But some (e.g. Player) do NOT.
	// References
	protected Level myLevel;

	// Getters
	public Vector2 PosLocal { get { return pos; } }
	public Vector2 PosGlobal {
		get {
			if (myLevel==null) { return PosLocal; } // Safety check.
			return PosLocal + myLevel.PosGlobal;
		}
	}
	protected Vector2 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
	protected float rotation {
		get { return this.transform.localEulerAngles.z; }
		set { this.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, value); }
	}

	virtual protected void BaseInitialize (Level myLevel, PropData data) {
		this.myLevel = myLevel;
		this.transform.SetParent (myLevel.transform);
		this.transform.localScale = Vector3.one;
		this.transform.localEulerAngles = Vector3.zero;

		this.transform.localPosition = data.pos; // note that this is just a convenience default. Any grounds will set their pos from their rect.
		rotation = data.rotation;
	}

	virtual public void FlipHorz() {
		pos = new Vector2(-pos.x, pos.y);
		rotation = -rotation;
	}
    
    abstract public PropData SerializeAsData();

}
