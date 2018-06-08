using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableGround : BaseGround, ISerializableData<DamageableGroundData> {
	// Properties
	[SerializeField] private bool dieFromBounce = true;
	[SerializeField] private bool dieFromVel = false;
	[SerializeField] private bool doRegen = false; // if TRUE, I'll come back after a moment!
	const float BreakVel = 0.4f; // the Player has to be moving at least this fast for me to get busted!
	const float RegenTime = 3f; // how long it takes for me to reappear after I've disappeared.

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, DamageableGroundData data) {
		base.BaseGroundInitialize(_myLevel, data);

		dieFromBounce = data.dieFromBounce;
		dieFromVel = data.dieFromVel;
		doRegen = data.doRegen;
	}

	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	override public void OnPlayerBounceOnMe(Player player) {
		if (dieFromBounce) {
			Disappear();
		}
	}

	override public void OnPlayerTouchMe(Player player, int playerSide) {
		if (dieFromVel) {
			// Left or Right sides
			if (playerSide==Sides.L || playerSide==Sides.R) {
				if (Mathf.Abs(player.Vel.x) > BreakVel) {
					Disappear();
				}
			}
			// Left or Right sides
			else if (playerSide==Sides.B || playerSide==Sides.T) {
				if (Mathf.Abs(player.Vel.y) > BreakVel) {
					Disappear();
				}
			}
		}
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
		if (myCollider!=null) { myCollider.enabled = _enabled; }
		if (bodySprite!=null) {
			if (doRegen) {
				GameUtils.SetSpriteAlpha (bodySprite, _enabled ? 1f : 0.15f);
			}
			else {
				bodySprite.enabled = _enabled;
			}
		}
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public DamageableGroundData SerializeAsData() {
		DamageableGroundData data = new DamageableGroundData();
		data.myRect = MyRect;
		data.dieFromBounce = dieFromBounce;
		data.dieFromVel = dieFromVel;
		data.doRegen = doRegen;
		return data;
	}



}
