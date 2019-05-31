using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : BaseGround {
	// Properties
	[SerializeField] private int hitsUntilBreak = -1;
	[SerializeField] private int numCoinsInMe = 0;
	private int numTimesHit = 0;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, CrateData data) {
		base.BaseGroundInitialize(_myRoom, data);

		hitsUntilBreak = data.hitsUntilBreak;
		numCoinsInMe = data.numCoinsInMe;
	}

	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
//	override public void OnCollideWithCollidable(Collidable collidable, int otherColSide) {
//		if (hitsUntilBreak < 0) { return; } // Unbreakable? Do nothin'.
//		// Other collidable's bottom hit me?
//		if (otherColSide == Sides.B) {
//			// Player?!
//			Player player = collidable as Player;
//			if (player != null) {
//				if (player.IsBouncing) {
//					if (player.Vel.y < -0.6f) {
//						GetHit();
//					}
//				}
//			}
//		}
//	}
	override public void OnPlayerBounceOnMe(Player player) {
		if (hitsUntilBreak < 0) { return; } // Unbreakable? Do nothin'.
		GetHit();
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
		bodySprite.enabled = false;
		// Am I a pinata?!
		for (int i=0; i<numCoinsInMe; i++) {
			SpawnCoinInMe();
		}
	}

	private void SpawnCoinInMe() {
		Coin newCoin = Instantiate(ResourcesHandler.Instance.Coin).GetComponent<Coin>();
		newCoin.transform.SetParent(this.transform.parent); // make its parent whatever mine is, too.
		newCoin.transform.localScale = Vector3.one * 0.25f; // HACK hardcoded
		newCoin.transform.localPosition = this.transform.localPosition;
		newCoin.transform.localPosition += new Vector3( // Put it randomly somewhere inside my box area (instead of putting them all exactly in the center).
			Random.Range(-myCollider.size.x*0.3f, myCollider.size.x*0.3f) * this.transform.localScale.x,
			Random.Range(-myCollider.size.y*0.3f, myCollider.size.y*0.3f) * this.transform.localScale.y);
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData ToData() {
        CrateData data = new CrateData {
            myRect = MyRect(),
            hitsUntilBreak = hitsUntilBreak,
            numCoinsInMe = numCoinsInMe,
            travelMind = new TravelMindData(travelMind),
        };
        return data;
	}


}
