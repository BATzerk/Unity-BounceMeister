using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class Spikes : Collidable {
	// Components
	[SerializeField] private SpriteRenderer bodySprite=null;

	// Getters
	private Rect MyRect {
		get {
			Vector2 center = bodySprite.transform.localPosition;
			Vector2 size = bodySprite.size;
			return new Rect(center, size);
		}
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, SpikesData data) {
		base.BaseInitialize(_myLevel, data);

		bodySprite.size = data.myRect.size;
		bodySprite.transform.localPosition = data.myRect.position;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnTriggerEnter2D(Collider2D col) {
		Player player = col.gameObject.GetComponent<Player>();
		if (player != null) {
			player.OnTouchSpikes(this);
		}
	}


	override public void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
		Player player = character as Player;
		if (player != null) {
			player.OnTouchSpikes(this);
		}
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
		SpikesData data = new SpikesData();
		data.myRect = MyRect;
		data.rotation = rotation;
		return data;
	}
}
