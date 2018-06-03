using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(SpriteRenderer))]
public class Ground : Collidable {
	// Properties
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!
	[SerializeField] private int colorType = 0; // green, blue, purple
	private bool pisBouncy; // previous isBouncy. Only update my sprite if there's been a change.

	// Getters (Private)
//	private bool IsInvincible { get { return numBouncesLeft < 0; } }


	private Color GetBodySpriteColor() {
		switch (colorType) {
		case 1: return IsBouncy ? new ColorHSB(190/360f, 0.73f, 0.83f).ToColor() : new ColorHSB(190/360f, 0.24f, 0.57f).ToColor(); // blue
		case 2: return IsBouncy ? new ColorHSB(280/360f, 0.76f, 0.83f).ToColor() : new ColorHSB(280/360f, 0.35f, 0.45f).ToColor(); // purple
		default: return IsBouncy ? new ColorHSB(76/360f, 0.84f, 0.83f).ToColor() : new ColorHSB(85/360f, 0.37f, 0.42f).ToColor(); // green
		}
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	virtual protected void Start() {
		UpdateBodySpriteColor();
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	void Update () {
		if (pisBouncy != IsBouncy) {
			UpdateBodySpriteColor();
		}
		pisBouncy = IsBouncy;
	}

	private void UpdateBodySpriteColor() {
		Color color = GetBodySpriteColor();
		this.GetComponent<SpriteRenderer>().color = color;
	}


}

