using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour {


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

}
