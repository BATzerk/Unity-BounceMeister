﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class Ground : BaseGround {
	// Properties
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!

	// Getters (Private)
//	private bool IsInvincible { get { return numBouncesLeft < 0; } }


    static public Color GetBodyColor(Ground g) { return GetBodyColor(g.myRoom.WorldIndex, g.isBouncy, g.canBounce, g.doRechargePlayer); }
    static public Color GetBodyColor(GroundData g, int worldIndex) { return GetBodyColor(worldIndex, g.isBouncy, g.canBounce, g.doRechargePlayer); }
    static public Color GetBodyColor(int worldIndex, bool isBouncy, bool canBounce, bool doRechargePlayer) {
		Color color = Colors.GroundBaseColor(worldIndex);
        if (isBouncy) { // Bouncy? Brighten it much!
            ColorHSB colorHSB = new ColorHSB(color);
            colorHSB.s = Mathf.Max(1, 0.4f + colorHSB.s*1.5f);
            colorHSB.b = Mathf.Max(1, colorHSB.b*1.3f);
            color = colorHSB.ToColor();
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
            mayPlayerEat = MayPlayerEatHere,
            isPlayerRespawn = IsPlayerRespawn,
            isBouncy = isBouncy,
            canBounce = canBounce,
            doRechargePlayer = doRechargePlayer,
        };
        return data;
	}


}

