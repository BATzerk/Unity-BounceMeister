using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSnapPos: MonoBehaviour {
	// Properties
	[SerializeField] private Vector2 posOffset; // e.g. (0, 0.1): we'll be 1/10th of the way higher to the next row. Useful for things that we want ON grounds (e.g. platforms and spikes).


	private float UnitSize { get { return GameProperties.UnitSize; } }
	private Vector3 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		// Snap position.
		float pu = UnitSize*0.5f;
		pos = new Vector3(
			Mathf.Round((pos.x-posOffset.x)/pu)*pu + posOffset.x,
			Mathf.Round((pos.y-posOffset.y)/pu)*pu + posOffset.y,
			pos.z);
//		pos += new Vector3(posOffset.x, posOffset.y, 0);
//		pos += new Vector3(gridOffset.x*UnitSize, gridOffset.y*UnitSize, 0);
	}


}



