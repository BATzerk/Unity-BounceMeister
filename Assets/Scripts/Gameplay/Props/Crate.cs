using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : Collidable {
	// Components
	[SerializeField] private BoxCollider2D myCollider;
	[SerializeField] private SpriteRenderer sr_body;
	// Properties
	[SerializeField] private int hitsUntilBreak = -1;
	private int numTimesHit = 0;


	override public void OnCollideWithPlayer(Player player) {
		if (player.IsBouncing) {
			if (player.Vel.y < -0.6f) {
				GetHit();
			}
		}
	}

	private void GetHit() {
		numTimesHit ++;
		if (numTimesHit >= hitsUntilBreak) {
			BreakMe();
		}
	}
	private void BreakMe() {
		myCollider.enabled = false;
		sr_body.enabled = false;
	}

}
