using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GridSnapPosScale : MonoBehaviour {
	// Components
//	private BoxCollider2D myCollider;
//	private SpriteRenderer myCollider;

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
//		myCollider = this.GetComponent<BoxCollider2D>();
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
	}


}



