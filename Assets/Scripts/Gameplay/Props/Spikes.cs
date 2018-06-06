using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class Spikes : Collidable, ISerializableData<SpikesData> {
	// Components
	[SerializeField] private SpriteRenderer bodySprite;


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
//	private void OnCollisionEnter2D(Collision2D col) {
//		Player player = col.gameObject.GetComponent<Player>();
//		if (player != null) {
//			player.OnTouchSpikes(this);
//		}
//	}
	private void OnTriggerEnter2D(Collider2D col) {
		Player player = col.gameObject.GetComponent<Player>();
		if (player != null) {
			player.OnTouchSpikes(this);
		}
	}


	override public void OnPlayerTouchMe(Player player, int playerSide) {
		player.OnTouchSpikes(this);
	}
//		// Kinda hacked in for now. (Hacked 'cause we're using colliders and our custom system independently.)
//		Spikes spikes = surfaceCol.GetComponent<Spikes>();
//		if (spikes != null) {
//			OnTouchSpikes(spikes);
//		}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public SpikesData SerializeAsData() {
		SpikesData data = new SpikesData();
		data.myRect = MyRect;
		data.rotation = rotation;
		return data;
	}
}
