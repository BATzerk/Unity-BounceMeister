using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class Ground : BaseGround {
	// Properties
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!
	[SerializeField] private int colorType = 0; // green, blue, purple

	// Getters (Private)
//	private bool IsInvincible { get { return numBouncesLeft < 0; } }


    static public Color GetBodyColor(Ground g) { return GetBodyColor(g.colorType, g.isBouncy, g.canBounce, g.doRechargePlayer); }
    static public Color GetBodyColor(GroundData g) { return GetBodyColor(g.colorType, g.isBouncy, g.canBounce, g.doRechargePlayer); }
	static public Color GetBodyColor(int colorType, bool isBouncy, bool canBounce, bool doRechargePlayer) {
		Color color;
		switch (colorType) {
			case 1: color = isBouncy ? new ColorHSB(190/360f, 0.73f, 0.83f).ToColor() : new ColorHSB(190/360f, 0.24f, 0.57f).ToColor(); break; // blue
			case 2: color = isBouncy ? new ColorHSB(280/360f, 0.76f, 0.83f).ToColor() : new ColorHSB(280/360f, 0.35f, 0.45f).ToColor(); break; // purple
			default: color = isBouncy ? new ColorHSB(76/360f, 0.84f, 0.83f).ToColor() : new ColorHSB(85/360f, 0.37f, 0.42f).ToColor(); break; // green
		}
		if (!canBounce) {
			color = Color.Lerp(color, Color.black, 0.7f);
		}
		if (!doRechargePlayer) {
			color = Color.Lerp(color, Color.black, 0.4f);
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
	public void Initialize(Room _myRoom, GroundData data) {
		base.BaseGroundInitialize(_myRoom, data);

		colorType = data.colorType;
		canBounce = data.canBounce;
		doRechargePlayer = data.doRechargePlayer;
        isBouncy = data.isBouncy;
		ApplyBodySpriteColor();
	}
	private void ApplyBodySpriteColor() {
		bodySprite.color = GetBodyColor(this);
	}




	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        GroundData data = new GroundData {
            myRect = MyRect(),
            canEatGems = CanEatEdibles,
            isPlayerRespawn = IsPlayerRespawn,
            isBouncy = isBouncy,
            canBounce = canBounce,
            doRechargePlayer = doRechargePlayer,
            colorType = colorType
        };
        return data;
	}


}

