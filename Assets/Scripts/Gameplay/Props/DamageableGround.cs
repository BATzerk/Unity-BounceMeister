using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableGround : BaseGround, ISerializableData<DamageableGroundData> {
	// Properties
	[SerializeField] private bool dieFromBounce = true;
	[SerializeField] private bool dieFromVel = false;
	[SerializeField] private bool dieFromPlayerLeave = false;
	[SerializeField] private bool doRegen = false; // if TRUE, I'll come back after a moment!
	private Color bodyColor; // depends on my properties, ya hear?
	const float BreakVel = 0.4f; // the Player has to be moving at least this fast for me to get busted!
	const float RegenTime = 2.2f; // how long it takes for me to reappear after I've disappeared.
	// References
	[SerializeField] private Sprite s_bodyOn;
	[SerializeField] private Sprite s_bodyOff;
	private Player playerTouchingMe;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, DamageableGroundData data) {
		base.BaseGroundInitialize(_myLevel, data);

		dieFromBounce = data.dieFromBounce;
		dieFromPlayerLeave = data.dieFromPlayerLeave;
		dieFromVel = data.dieFromVel;
		doRegen = data.doRegen;
		// Color me impressed!
		if (doRegen) {
			bodyColor = new ColorHSB(280/360f, 0.4f, 0.6f).ToColor();
		}
		else {
			bodyColor = new ColorHSB(280/360f, 0.1f, 0.56f).ToColor();
		}
	}

	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	override public void OnPlayerBounceOnMe(Player player) {
		if (dieFromBounce) {
			TurnOff();
		}
	}

//	override public void OnPlayerTouchMe(Player player, int playerSide) {
//		base.OnPlayerTouchMe(player, playerSide);
//		if (dieFromVel) {
//			// Left or Right sides
//			if (playerSide==Sides.L || playerSide==Sides.R) {
//				if (Mathf.Abs(player.Vel.x) > BreakVel) {
//					Disappear();
//				}
//			}
//			// Left or Right sides
//			else if (playerSide==Sides.B || playerSide==Sides.T) {
//				if (Mathf.Abs(player.Vel.y) > BreakVel) {
//					Disappear();
//				}
//			}
//		}
//	}

	override public void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
		if (charSide != Sides.B && MyRect.size.y==1) { return; } // Currently, we ONLY care about FEET if we're thin. Kinda a test.
		if (character is Player) {
			playerTouchingMe = character as Player;
			if (dieFromVel) {
				// Left or Right sides
				if (charSide==Sides.L || charSide==Sides.R) {
					if (Mathf.Abs(character.Vel.x) > BreakVel) {
						TurnOff();
					}
				}
				// Left or Right sides
				else if (charSide==Sides.B || charSide==Sides.T) {
					if (Mathf.Abs(character.Vel.y) > BreakVel) {
						TurnOff();
					}
				}
			}
		}
	}
	override public void OnCharacterLeaveMe(int charSide, PlatformCharacter character) {
		if (charSide != Sides.B && MyRect.size.y==1) { return; } // Currently, we ONLY care about FEET if we're thin. Kinda a test.
		if (dieFromPlayerLeave && character is Player) {
			TurnOff();
			playerTouchingMe = null;
		}
	}

	// Kinda hacked in for now.
	private void TurnOff() {
		SetIsOn(false);
		if (doRegen) {
			Invoke("TurnOn", RegenTime);
		}
	}
	private void TurnOn() {
		SetIsOn(true);
	}
	private void SetIsOn(bool _isOn) {
		GameUtils.SetSpriteAlpha (bodySprite, 1); // Always reset my alpha here.
		if (myCollider!=null) { myCollider.enabled = _isOn; }
		if (bodySprite!=null) {
			bodySprite.sprite = _isOn ? s_bodyOn : s_bodyOff;
			if (doRegen) {
//				GameUtils.SetSpriteAlpha (bodySprite, _isOn ? 1f : 0.15f);
			}
			else {
				bodySprite.enabled = _isOn;
			}
		}
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update() {
		if (dieFromPlayerLeave && playerTouchingMe != null) {
			float alpha = 0.6f + Mathf.Sin(Time.time*20f)*0.3f;
			GameUtils.SetSpriteAlpha (bodySprite, alpha);
		}
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public DamageableGroundData SerializeAsData() {
		DamageableGroundData data = new DamageableGroundData();
		data.myRect = MyRect;
		data.dieFromBounce = dieFromBounce;
		data.dieFromPlayerLeave = dieFromPlayerLeave;
		data.dieFromVel = dieFromVel;
		data.doRegen = doRegen;
		return data;
	}



}
