using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Collidable : Prop {
	// Properties
	[SerializeField] protected bool canBounce = true; // if false, Alph cannot bounce off of me! He'll just land.
	[SerializeField] protected bool doRechargePlayer = true; // if true, we recharge Alph's plunge when their feet touch me!
	[SerializeField] protected bool isBouncy = false;

	// Getters (Public)
	public bool CanBounce { get { return canBounce; } }
	public bool DoRechargePlayer { get { return doRechargePlayer; } }
	public bool IsBouncy { get { return isBouncy; } }
	// Getters (Protected)
	protected bool IsPlayer(Collision2D col) {
		return col.gameObject.GetComponent<Player>() != null;
	}

//	virtual public void OnPlayerTouchMe(Player player, int playerSide) { }
//	virtual public void OnCollideWithCollidable(Collidable collidable, int otherSideCol) {} //abstract 
	virtual public void OnPlayerBounceOnMe(Player player) {}


	virtual public void OnCharacterTouchMe(int charSide, PlatformCharacter character) { }
	virtual public void OnCharacterLeaveMe(int charSide, PlatformCharacter character) { }

}
