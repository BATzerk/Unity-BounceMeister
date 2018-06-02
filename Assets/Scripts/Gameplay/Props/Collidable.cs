using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Collidable : MonoBehaviour {
	[SerializeField] protected bool isBouncy = true;
	[SerializeField] protected bool doDisappearOnCharacterLeave = false;
	const float RegenTime = 2f; // how long it takes for me to reappear after I've disappeared.

	public bool IsBouncy { get { return isBouncy; } }

//	virtual public void OnCollideWithCollidable(Collidable collidable, int otherSideCol) {} //abstract 
	virtual public void OnPlayerBounceOnMe(Player player) {}


	public void OnCharacterTouchMe(PlatformCharacter character) {

	}
	public void OnCharacterLeaveMe(PlatformCharacter character) {
		if (doDisappearOnCharacterLeave) {
			Disappear();
		}
	}

	// Kinda hacked in for now.
	private void Disappear() {
		SetSpriteColliderEnabled(false);
		Invoke("EnableSpriteCollider", RegenTime);
	}
	private void EnableSpriteCollider() {
		SetSpriteColliderEnabled(true);
	}
	private void SetSpriteColliderEnabled(bool _enabled) {
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		Collider2D collider = GetComponent<Collider2D>();
		if (spriteRenderer!=null) { spriteRenderer.enabled = _enabled; }
		if (collider!=null) { collider.enabled = _enabled; }
	}
}
