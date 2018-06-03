using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : Collidable {


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

}
