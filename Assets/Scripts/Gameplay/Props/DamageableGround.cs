using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableGround : Ground {
	// Properties
	[SerializeField] private bool doRegen = false; // if TRUE, I'll come back after a moment!
	const float RegenTime = 3f; // how long it takes for me to reappear after I've disappeared.



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	override public void OnPlayerBounceOnMe(Player player) {
		Disappear();
	}
	// Kinda hacked in for now.
	private void Disappear() {
		SetSpriteColliderEnabled(false);
		if (doRegen) {
			Invoke("EnableSpriteCollider", RegenTime);
		}
	}
	private void EnableSpriteCollider() {
		SetSpriteColliderEnabled(true);
	}
	private void SetSpriteColliderEnabled(bool _enabled) {
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		Collider2D collider = GetComponent<Collider2D>();
		if (collider!=null) { collider.enabled = _enabled; }
		if (spriteRenderer!=null) {
			if (doRegen) {
				GameUtils.SetSpriteAlpha (spriteRenderer, _enabled ? 1f : 0.15f);
			}
			else {
				spriteRenderer.enabled = _enabled;
			}
		}
	}



}
