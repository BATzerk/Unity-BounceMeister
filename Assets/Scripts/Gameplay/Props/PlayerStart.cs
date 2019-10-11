using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStart : Prop {



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, PlayerStartData data) {
		base.InitializeAsProp(_myRoom, data);

		//this.GetComponent<SpriteRenderer>().enabled = false; // Of course, hide my sprite. It's just for the editor.
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData ToData() {
		PlayerStartData data = new PlayerStartData();
		data.pos = PosLocal;
		return data;
	}

}
