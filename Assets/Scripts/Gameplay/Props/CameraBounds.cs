using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class CameraBounds : Prop, ISerializableData<CameraBoundsData> {
	// Components
	[SerializeField] private SpriteRenderer bodySprite=null;

	// Getters (Private)
	public Rect RectLocal { // Note that we save/load our Rect locally, not globally. (Otherwise we'd be wonked when levels are moved.)
		get {
			Rect newRect = new Rect();
			newRect.size = bodySprite.size;
			newRect.center = bodySprite.transform.localPosition;
//			if (myLevel != null) { // Only need this check for premade levels.
//				newRect.center += myLevel.PosGlobal; // Add my level's pos too!
//			}
			return newRect;
		}
	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		// Of course, hide the sprite! It's just for the editor.
		bodySprite.enabled = false;
	}
	public void Initialize(Level _myLevel, CameraBoundsData data) {
		base.BaseInitialize(_myLevel, data);

		bodySprite.size = data.myRect.size;
		bodySprite.transform.localPosition = data.myRect.center;
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public CameraBoundsData SerializeAsData() {
		CameraBoundsData data = new CameraBoundsData();
		data.myRect = RectLocal;
		return data;
	}


}
