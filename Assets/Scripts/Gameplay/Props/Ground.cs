using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class Ground : BaseGround, ISerializableData<GroundData> {
	// Properties
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!
	[SerializeField] private int colorType = 0; // green, blue, purple

	// Getters (Private)
//	private bool IsInvincible { get { return numBouncesLeft < 0; } }


	private Color GetBodySpriteColor() {
		Color color;
		switch (colorType) {
			case 1: color = IsBouncy ? new ColorHSB(190/360f, 0.73f, 0.83f).ToColor() : new ColorHSB(190/360f, 0.24f, 0.57f).ToColor(); break; // blue
			case 2: color = IsBouncy ? new ColorHSB(280/360f, 0.76f, 0.83f).ToColor() : new ColorHSB(280/360f, 0.35f, 0.45f).ToColor(); break; // purple
			default: color = IsBouncy ? new ColorHSB(76/360f, 0.84f, 0.83f).ToColor() : new ColorHSB(85/360f, 0.37f, 0.42f).ToColor(); break; // green
		}
		if (!DoRechargePlayer) {
			color = Color.Lerp(color, Color.black, 0.7f);
		}
		return color;
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		base.Start();

		ApplyBodySpriteColor();
	}
	public void Initialize(Level _myLevel, GroundData data) {
		base.BaseGroundInitialize(_myLevel, data);

		colorType = data.colorType;
		doRechargePlayer = data.doRechargePlayer;
		ApplyBodySpriteColor();
	}
	private void ApplyBodySpriteColor() {
		bodySprite.color = GetBodySpriteColor();
	}




	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public GroundData SerializeAsData() {
		GroundData data = new GroundData();
		data.myRect = MyRect;
		data.doRechargePlayer = doRechargePlayer;
		data.colorType = colorType;
		return data;
	}


}

