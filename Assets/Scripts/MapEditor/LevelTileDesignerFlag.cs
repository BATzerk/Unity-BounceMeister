﻿using UnityEngine;
using System.Collections;

public class LevelTileDesignerFlag : MonoBehaviour {
	// Constants
	private const float SPRITE_ALPHA_DEFAULT = 0.9f;
	private const float SPRITE_ALPHA_MOUSE_OVER = 0.4f;
	// Components
	[SerializeField] private BoxCollider2D boxCollider;
	[SerializeField] private SpriteRenderer flagSprite;
	// References
	[SerializeField] private LevelTile levelTileRef;
	[SerializeField] private Sprite[] designerFlagSprites;


	private void Start () {
		GameUtils.SetSpriteAlpha (flagSprite, SPRITE_ALPHA_DEFAULT);
	}
	
	public void UpdateDesignerFlagButtonVisuals() {
		flagSprite.sprite = designerFlagSprites [levelTileRef.LevelDataRef.DesignerFlag];
	}
	public void ApplyPosAndSize (Rect rect) {
		const float w = 300;
		const float h = 300;
//		float x = tileX - (tileW-w)*0.5f;
//		float y = tileY + (tileH-h)*0.5f;
		float x = rect.center.x - (rect.size.x-w)*0.5f;
		float y = rect.center.y + (rect.size.y-h)*0.5f;
		this.transform.localPosition = new Vector3 (x, y, -1); // Move closer to camera than the main contents bodyCollider.
		boxCollider.size = new Vector2 (w, h);
		GameUtils.SizeSpriteRenderer (flagSprite, w,h, true);
	}
	
	
	
	private void OnMouseEnter() {
		GameUtils.SetSpriteAlpha (flagSprite, SPRITE_ALPHA_MOUSE_OVER);
	}
	private void OnMouseExit() {
		GameUtils.SetSpriteAlpha (flagSprite, SPRITE_ALPHA_DEFAULT);
	}
	private void OnMouseDown() {
		// Determine the new value of our flag!
		int newDesignerFlagValue = levelTileRef.LevelDataRef.DesignerFlag + 1;
		if (newDesignerFlagValue >= DesignerFlags.NumFlags) { newDesignerFlagValue = 0; } // Loop back to 0.
		// Tell levelData to save the new flag! :)
		levelTileRef.LevelDataRef.SetDesignerFlag (newDesignerFlagValue, true);
		// Update the designerFlag button
		UpdateDesignerFlagButtonVisuals ();
	}


}




