using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSnapPos : BaseGridSnap {
	// Properties
	[SerializeField] private Vector2 posOffset=Vector2.zero; // e.g. (0, 0.1): we'll be 1/10th of the way higher to the next row. Useful for things that we want ON grounds (e.g. platforms and spikes).
    
    
    // Start
    private void Start() {
        SnapPos();
    }
    
    // Doers
    private void SnapPos() {
        pos = new Vector3(
            Mathf.Round((pos.x-posOffset.x)/UnitSize*2f)*UnitSize*0.5f + posOffset.x, // half-snap!
            Mathf.Round((pos.y-posOffset.y)/UnitSize*2f)*UnitSize*0.5f + posOffset.y,
            pos.z);
    }
    
    // Update
    #if UNITY_EDITOR
	private void Update() {
        SnapPos();
    }
    #endif


}



