using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class GridSnapPos : BaseGridSnap {
	// Properties
	[SerializeField] private Vector2 posOffset=Vector2.zero; // e.g. (0, 0.1): we'll be 1/10th of the way higher to the next row. Useful for things that we want ON grounds (e.g. platforms and spikes).

	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		// Snap position.
		float pu = UnitSize;
		pos = new Vector3(
			Mathf.Round((pos.x-posOffset.x)/pu)*pu + posOffset.x,
			Mathf.Round((pos.y-posOffset.y)/pu)*pu + posOffset.y,
			pos.z);
//		pos += new Vector3(posOffset.x, posOffset.y, 0);
//		pos += new Vector3(gridOffset.x*UnitSize, gridOffset.y*UnitSize, 0);
	}


}



