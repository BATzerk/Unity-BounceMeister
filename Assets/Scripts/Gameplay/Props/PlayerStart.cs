using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStart : Prop {



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, PlayerStartData data) {
		base.BaseInitialize(_myLevel, data);

		//this.GetComponent<SpriteRenderer>().enabled = false; // Of course, hide my sprite. It's just for the editor.
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
		PlayerStartData data = new PlayerStartData();
		data.pos = PosLocal;
		return data;
	}

}
