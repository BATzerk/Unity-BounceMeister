using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : MonoBehaviour {
	// Components
	private SpriteRenderer sr_body;
	// Properties
	[SerializeField] private float strength = 0.08f;
	private bool isCharacterInMe = false;
	private bool pisCharacterInMe = false; // so we can detect a change.

	// Getters
	private float angle { get { return transform.localEulerAngles.z * Mathf.Deg2Rad; } }
//	protected bool IsCharacter(Collision2D col) {
//		return col.gameObject.GetComponent<PlatformCharacter>() != null;
//	}
	private PlatformCharacter Character(Collider2D col) {
		return col.gameObject.GetComponent<PlatformCharacter>();
	}
//	private PlatformCharacter CharacterAffectedByMe(Collider2D col) {
//		PlatformCharacter character = Character(col);
//		if (character==null || !character.IsAffectedByLift()) { return null; }
//		return character;
//	}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		sr_body = GetComponent<SpriteRenderer>();
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
//	private void Update() {
//		if (pisCharacterInMe != isCharacterInMe) {
//			ApplyBodyAlpha();
//		}
//		pisCharacterInMe = isCharacterInMe;
//	}

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void ApplyBodyAlpha() {
		float alpha = isCharacterInMe ? 0.8f : 0.3f;
		GameUtils.SetSpriteAlpha(sr_body, alpha);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnTriggerEnter2D(Collider2D col) {
		// If there's any sort of character here, inform it!
		PlatformCharacter character = Character(col);
		if (character != null) {
			character.OnEnterLift();
		}
		// Now route to our enter-or-stay function. ;)
		OnTriggerEnterOrStay(col);
	}
	private void OnTriggerStay2D(Collider2D col) {
		OnTriggerEnterOrStay(col);
	}
	private void OnTriggerEnterOrStay(Collider2D col) {
		PlatformCharacter character = Character(col);//CharacterAffectedByMe
		if (character != null) {
			isCharacterInMe = true;
			ApplyBodyAlpha();
			Vector2 force = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * strength;
			character.ChangeVel(force);
		}
	}
	private void OnTriggerExit2D(Collider2D col) {
		// If there's any sort of character here, inform it!
		PlatformCharacter character = Character(col);
		if (character != null) {
			character.OnExitLift();
		}

//		PlatformCharacter character = CharacterAffectedByMe(col);
		if (character != null) {
			isCharacterInMe = false;
			ApplyBodyAlpha();
		}
//		if (IsCharacter(col)) {
//			isCharacterInMe = false;
//			ApplyBodyAlpha();
//		}
	}


}

