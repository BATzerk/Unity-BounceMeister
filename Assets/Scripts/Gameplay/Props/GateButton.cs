using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GateButton : MonoBehaviour {
	// Components
	[SerializeField] private BoxCollider2D myCollider;
	[SerializeField] private SpriteRenderer sr_body;
	// References
	private Gate myGate;
	// Properties
	private bool isPressed;
	private Color bodyColor = Color.red;

	// Getters
	public bool IsPressed { get { return isPressed; } }
	/** Kind of a weird function. We ONLY use my rotation to estimate what side button we are, based on the grid. */
	private int GetSideToPressMe() {
//		Vector2 pos = transform.localPosition;
//		float pu = GameProperties.UnitSize;
//		Vector2 snappedPos = new Vector3(Mathf.Round(pos.x/pu)*pu, Mathf.Round(pos.y/pu)*pu);
//		Vector2 diff = snappedPos - pos;
//		if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) {
//			if (diff.x < 0) { return Sides.L; }
//			else { return Sides.R; }
//		}
//		else {
//			if (diff.y < 0) { return Sides.B; }
//			else { return Sides.T; }
//		}
		int side = Mathf.RoundToInt(this.transform.localEulerAngles.z/90f);
		return side;
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void SetMyGate(Gate gate) {
		this.myGate = gate;
		bodyColor = myGate.BodyColor;
		sr_body.color = bodyColor;

		// test
//		int side = GetSideToPressMe();
//		Debug.Log("GetSideToPressMe: " + side);
	}
	private void GetPressed() {
		isPressed = true;
		sr_body.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, 0.1f);
		if (myGate != null) {
			myGate.OnButtonPressed();
		}
	}



	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnTriggerEnter2D(Collider2D otherCol) {
		// Ground??
		if (LayerMask.LayerToName(otherCol.gameObject.layer) == LayerNames.Player) {
			OnPlayerTouchMe(otherCol.GetComponentInChildren<Player>());
		}
	}
	private void OnPlayerTouchMe(Player player) {
		if (player==null) { Debug.LogError("Uh-oh! Calling OnPlayerTouchMe with a null Player. Hmm."); return; }
//		int playerSideMoving = MathUtils.GetSide(player.DashDir.ToVector2Int());


//		int sideToPressMe = GetSideToPressMe();
//
//		if (playerSideMoving == sideToPressMe) {
			GetPressed();
//		}
	}


}
