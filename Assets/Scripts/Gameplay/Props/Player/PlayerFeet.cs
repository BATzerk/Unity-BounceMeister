using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Is this class used? Can we make it not Player-specific?
public class PlayerFeet : MonoBehaviour {
	// Components
	[SerializeField] private BoxCollider2D myCollider=null;
	// References
	[SerializeField] private Player myPlayer=null;

	// Getters
	public Player MyPlayer { get { return myPlayer; } }


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start () {
		
	}

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void OnSetBodySize(Vector2 bodySize) {
		myCollider.size = new Vector2(bodySize.x*0.99f, 0.5f);
		this.transform.localPosition = new Vector3(0, -bodySize.y*0.5f + myCollider.size.y*0.5f - 0.1f);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
//	private void OnTriggerEnter2D(Collider2D otherCol) {
//		// Ground??
//		if (LayerMask.LayerToName(otherCol.gameObject.layer) == LayerNames.Ground) {
//			myPlayer.OnFeetTouchGround ();
//		}
//	}
//	private void OnTriggerExit2D(Collider2D otherCol) {
//		// Ground??
//		if (LayerMask.LayerToName(otherCol.gameObject.layer) == LayerNames.Ground) {
//			myPlayer.OnFeetLeaveGround ();
//		}
//	}
//	private void OnTriggerStay2D(Collider2D otherCol) {
//		// Ground??
//		if (LayerMask.LayerToName(otherCol.gameObject.layer) == LayerNames.Ground) {
//			myPlayer.OnFeetTouchingGround ();
//		}
//	}


}


