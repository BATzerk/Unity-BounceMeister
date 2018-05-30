using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSnapPos: MonoBehaviour {

	private Vector3 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		// Snap position.
		float pu = GameProperties.UnitSize*0.5f;
		pos = new Vector3(Mathf.Round(pos.x/pu)*pu, Mathf.Round(pos.y/pu)*pu, pos.z);
	}


}



