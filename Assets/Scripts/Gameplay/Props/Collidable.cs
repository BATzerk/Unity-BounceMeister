using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Collidable : MonoBehaviour {
	[SerializeField] protected bool isBouncy = true;

	public bool IsBouncy { get { return isBouncy; } }

//	virtual public void OnCollideWithCollidable(Collidable collidable, int otherSideCol) {} //abstract 
	virtual public void OnPlayerBounceOnMe(Player player) {}
}
