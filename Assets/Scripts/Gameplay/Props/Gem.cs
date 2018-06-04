using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour {
	// Components
	[SerializeField] private BoxCollider2D myCollider;
	[SerializeField] private ParticleSystem ps_collectedBurst;
	[SerializeField] private SpriteRenderer sr_body;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start () {
//		// Size me right!
//		float diameter = 2f;
//		GameUtils.SizeSpriteRenderer(sr_body, diameter,diameter);
//		myCollider.size = diameter*0.5f;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnTriggerEnter2D(Collider2D otherCol) {
		// Ground??
		if (LayerMask.LayerToName(otherCol.gameObject.layer) == LayerNames.Player) {
			GetCollected();
		}
	}
	private void GetCollected() {
		myCollider.enabled = false;
		sr_body.enabled = false;
		ps_collectedBurst.Emit(15);
	}



}
