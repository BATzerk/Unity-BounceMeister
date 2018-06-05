using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class CameraBounds : Prop, ISerializableData<CameraBoundsData> {
	// Components
	[SerializeField] private SpriteRenderer bodySprite=null;

	// Getters (Private)
	public Rect MyRect {
		get {
			Vector2 center = bodySprite.transform.localPosition;
			if (myLevel != null) { // Only need this check for premade levels.
				center += myLevel.PosWorld; // Add my level's pos too!
			}
			Vector2 size = bodySprite.transform.localScale;
			return new Rect(center, size);
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

		bodySprite.transform.localScale = data.myRect.size;
		bodySprite.transform.localPosition = data.myRect.position;
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public CameraBoundsData SerializeAsData() {
		CameraBoundsData data = new CameraBoundsData();
		data.myRect = MyRect;
		return data;
	}


}
