using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift : MonoBehaviour {
	// Properties
	[SerializeField] private float yForce = 0.08f;

	private void OnTriggerStay2D(Collider2D col) {
		PlatformCharacter character = col.gameObject.GetComponent<PlatformCharacter>();
		if (character != null && !character.feetOnGround) { // TEST with feetOnGround
			character.ChangeVel(new Vector2(0, yForce));
		}
	}
}
