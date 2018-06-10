using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSnapPosScale : MonoBehaviour {
	// Components
	private SpriteRenderer spriteRenderer;
	// Properties
	private bool doSnapSpriteSize; // true if I have a SpriteRenderer that's sliced or tiled! (In this scenario, our scale will probably stay at 1, and it's the SPRITE that's scaled.)

	// Getters
	private float rotation { get { return this.transform.localEulerAngles.z; } }
	private Vector3 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
	private Vector3 scale {
		get {
			if (doSnapSpriteSize) { return spriteRenderer.size; }
			else { return this.transform.localScale; }
		}
		set {
			if (doSnapSpriteSize) { spriteRenderer.size = value; }
			else { this.transform.localScale = value; }
		}
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start () {
		spriteRenderer = this.GetComponent<SpriteRenderer>();
		doSnapSpriteSize = spriteRenderer!=null && spriteRenderer.drawMode!=SpriteDrawMode.Simple;
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private Vector3 ppos;
	private Vector3 pscale;
	private void Update () {
		// If scale or pos has changed, snap me!
		if (scale != pscale || pos != ppos) {
			SnapPosAndScale();
			pscale = scale;
			ppos = pos;
		}
	}
	private void SnapPosAndScale() {
		float us = GameProperties.UnitSize;
		// Snap scale.
		scale = new Vector3(Mathf.Round(scale.x/us)*us, Mathf.Round(scale.y/us)*us, scale.z);
		scale = new Vector3(Mathf.Max(1, scale.x), Mathf.Max(1, scale.y)); // Don't let things get weird.
		// Snap position.
		bool isHalfPosX;
		bool isHalfPosY;
		if (rotation%180 == 0) { // Standard rotation.
			isHalfPosX = scale.x%2 == 1;
			isHalfPosY = scale.y%2 == 1;
		}
		else { // Rotated at a 90-degree angle?? FLIP the half-ness of x and y poses! (Note that obviously this class only works with 90-degree rotations.)
			isHalfPosX = scale.y%2 == 1;
			isHalfPosY = scale.x%2 == 1;
		}
		Vector2 posOffset = new Vector2(isHalfPosX?us*0.5f:0, isHalfPosY?us*0.5f:0); // not super neat, but it's ok.
		pos = new Vector3(
			Mathf.Round((pos.x-posOffset.x)/us)*us+posOffset.x,
			Mathf.Round((pos.y-posOffset.y)/us)*us+posOffset.y,
			pos.z);
		
//		// Snap sprite scale!
//		if (doSnapSpriteSize) {
//			spriteRenderer.size = new Vector3(Mathf.Round(spriteRenderer.size.x/us)*us, Mathf.Round(spriteRenderer.size.y/us)*us, scale.z);
//			spriteRenderer.size = new Vector3(Mathf.Max(1, spriteRenderer.size.x), Mathf.Max(1, spriteRenderer.size.y)); // Don't let things get weird.
//		}
	}


}



