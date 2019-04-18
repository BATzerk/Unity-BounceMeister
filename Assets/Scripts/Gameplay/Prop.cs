﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Prop : MonoBehaviour {
    // Overrideables
    virtual public bool DoSaveInRoomFile() { return true; } // by default, ALL Props wanna get saved into the Room text file. But some (e.g. Player) do NOT.
	// References
	protected Room myRoom;

	// Getters
	public Vector2 PosLocal { get { return pos; } }
	public Vector2 PosGlobal {
		get {
			if (myRoom==null) { return PosLocal; } // Safety check.
			return PosLocal + myRoom.PosGlobal;
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

	virtual protected void BaseInitialize(Room myRoom, PropData data) {
		this.myRoom = myRoom;
        GameUtils.ParentAndReset(this.gameObject, myRoom.transform);

		this.transform.localPosition = data.pos; // note that this is just a convenience default. Any grounds will set their pos from their rect.
		rotation = data.rotation;
	}

	virtual public void FlipHorz() {
		pos = new Vector2(-pos.x, pos.y);
		rotation = -rotation;
	}
    
    abstract public PropData SerializeAsData();

}
