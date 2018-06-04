using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSnapPosScale : MonoBehaviour {
	// Components
	private SpriteRenderer spriteRenderer;
	// Properties
	private bool doSnapSpriteSize; // true if I have a SpriteRenderer that's sliced or tiled! (In this scenario, our scale will probably stay at 1, and it's the SPRITE that's scaled.)

	private Vector3 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
	private Vector3 scale {
		get { return this.transform.localScale; }
		set { this.transform.localScale = value; }
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
	private void Update () {
		// Snap position.
		float pu = GameProperties.UnitSize*0.5f;
		pos = new Vector3(Mathf.Round(pos.x/pu)*pu, Mathf.Round(pos.y/pu)*pu, pos.z);
		// Snap scale.
		float su = GameProperties.UnitSize;
		scale = new Vector3(Mathf.Round(scale.x/su)*su, Mathf.Round(scale.y/su)*su, scale.z);
		scale = new Vector3(Mathf.Max(1, scale.x), Mathf.Max(1, scale.y)); // Don't let things get weird.
		// Snap sprite scale!
		if (doSnapSpriteSize) {
			spriteRenderer.size = new Vector3(Mathf.Round(spriteRenderer.size.x/su)*su, Mathf.Round(spriteRenderer.size.y/su)*su, scale.z);
			spriteRenderer.size = new Vector3(Mathf.Max(1, spriteRenderer.size.x), Mathf.Max(1, spriteRenderer.size.y)); // Don't let things get weird.
		}
	}


}



