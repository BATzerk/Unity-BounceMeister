using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Collidable : MonoBehaviour {
	[SerializeField] private bool doRechargePlayer = true; // if true, we recharge the Player's plunge when their feet touch me!
	[SerializeField] protected bool isBouncy = false;

	// Getters (Public)
	public bool DoRechargePlayer { get { return doRechargePlayer; } }
	public bool IsBouncy { get { return isBouncy; } }

//	virtual public void OnCollideWithCollidable(Collidable collidable, int otherSideCol) {} //abstract 
	virtual public void OnPlayerBounceOnMe(Player player) {}


//	public void OnCharacterTouchMe(PlatformCharacter character) {
//
//	}
//	public void OnCharacterLeaveMe(PlatformCharacter character) {
//		if (doDisappearOnCharacterLeave) {
//			Disappear();
//		}
//	}

}
