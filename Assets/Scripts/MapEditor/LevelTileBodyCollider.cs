﻿using UnityEngine;
using System.Collections;

/** This guy's just so we can have MULTIPLE colliders that serve multiple purposes in a LevelTile. */
public class LevelTileBodyCollider : MonoBehaviour {
	// Components
	[SerializeField] private BoxCollider2D boxCollider;
	// References
	[SerializeField] private LevelTile levelTileRef;


//	public void UpdatePosAndSize (float x,float y, float w,float h) {
	public void UpdatePosAndSize(Rect rect) {
		boxCollider.transform.localPosition = new Vector3 (rect.center.x,rect.center.y, 0);
		boxCollider.size = rect.size;
	}

	
	private void OnMouseEnter() {
		levelTileRef.OnMouseEnterBodyCollider();
	}
	private void OnMouseExit() {
		levelTileRef.OnMouseExitBodyCollider();
	}


}
