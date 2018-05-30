using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : Collidable {
	// Components
	[SerializeField] private BoxCollider2D myCollider=null;
	[SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	[SerializeField] private int hitsUntilBreak = -1;
	[SerializeField] private int numCoinsInMe = 0;
	private int numTimesHit = 0;


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	override public void OnCollideWithPlayer(Player player) {
		if (hitsUntilBreak < 0) { return; } // Unbreakable? Do nothin'.
		if (player.IsBouncing) {
			if (player.Vel.y < -0.6f) {
				GetHit();
			}
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GetHit() {
		numTimesHit ++;
		if (numTimesHit >= hitsUntilBreak) {
			BreakMe();
		}
	}
	private void BreakMe() {
		myCollider.enabled = false;
		sr_body.enabled = false;
		// Am I a pinata?!
		for (int i=0; i<numCoinsInMe; i++) {
			SpawnCoinInMe();
		}
	}

	private void SpawnCoinInMe() {
		Coin newCoin = Instantiate(ResourcesHandler.Instance.coin).GetComponent<Coin>();
		newCoin.transform.SetParent(this.transform.parent); // make its parent whatever mine is, too.
		newCoin.transform.localScale = Vector3.one * 0.25f; // HACK hardcoded
		newCoin.transform.localPosition = this.transform.localPosition;
		newCoin.transform.localPosition += new Vector3( // Put it randomly somewhere inside my box area (instead of putting them all exactly in the center).
			Random.Range(-myCollider.size.x*0.3f, myCollider.size.x*0.3f) * this.transform.localScale.x,
			Random.Range(-myCollider.size.y*0.3f, myCollider.size.y*0.3f) * this.transform.localScale.y);
	}

}
