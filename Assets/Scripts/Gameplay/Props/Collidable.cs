using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Collidable : Prop {
	// Properties
	[SerializeField] protected bool mayBounce = true; // if FALSE, NOBODY may bounce off of me! They'll just land.
	[SerializeField] protected bool doRechargePlayer = true; // if TRUE, we recharge Plunga's plunge when their feet touch me.
	[SerializeField] protected bool isBouncy = false; // if TRUE, PlatformCharacter will bounce on me instead of land.
    public Vector2 vel { get; protected set; }

	// Getters (Public)
	public bool MayBounce { get { return mayBounce; } }
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
    //virtual public void OnTriggerEnter2D(Collider2D col) { }
    //virtual public void OnTriggerExit2D(Collider2D col) { }

}
