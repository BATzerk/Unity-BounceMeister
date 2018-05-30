using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {
	// Components
	[SerializeField] private CircleCollider2D myCollider;
	[SerializeField] private SpriteRenderer sr_body;
	[SerializeField] private ParticleSystem ps_collectedBurst;
	// Properties
	[SerializeField] private int value = 1; // how much I'm worth.

	// Getters
	public int Value { get { return value; } }


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnCollisionEnter2D(Collision2D col) {
		Player player = col.gameObject.GetComponent<Player>();
		if (player != null) {
			GetCollected();
		}
	}

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GetCollected() {
		// Disable my collider and sprite!
		myCollider.enabled = false;
		sr_body.enabled = false;
		// Particle burst!
		ps_collectedBurst.Emit (15);
		// Pump up our funds, yo!
		GameManagers.Instance.DataManager.ChangeCoinsCollected(value);
	}

}
