using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Gate : MonoBehaviour {
	// Components
	[SerializeField] private BoxCollider2D myCollider;
	[SerializeField] private SpriteRenderer sr_body;
	// References
	[SerializeField] private GateButton[] myButtons;
	// Properties
	private Color bodyColor; // set in Awake.

	// Getters
	public Color BodyColor { get { return bodyColor; } }
	private bool AreAllMyButtonsPressed() {
		foreach (GateButton button in myButtons) {
			if (button==null) { continue; }
			if (!button.IsPressed) { return false; }
		}
		return true;
	}


	// ----------------------------------------------------------------
	//  Gizmos
	// ----------------------------------------------------------------
	private void OnDrawGizmos() {
		if (myButtons==null) { return; }
		Gizmos.color = Color.Lerp(Color.white, BodyColor, 0.5f);
		foreach (GateButton button in myButtons) {
			if (button==null) { continue; }
			Gizmos.DrawLine(this.transform.position, button.transform.position);
		}
	}


	// ----------------------------------------------------------------
	//  Awake
	// ----------------------------------------------------------------
	private void Awake() {
		bodyColor = sr_body.color;

		// Tell all my buttons that I'M their guy!
		foreach (GateButton button in myButtons) {
			if (button==null) { continue; }
			button.SetMyGate(this);
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void UnlockMe() {
		myCollider.enabled = false;
		sr_body.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, 0.1f);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnButtonPressed() {
		// Check if all buttons have been pressed!
		if (AreAllMyButtonsPressed()) {
			UnlockMe();
		}
	}




}
