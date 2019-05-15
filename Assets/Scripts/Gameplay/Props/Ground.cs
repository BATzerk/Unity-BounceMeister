using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class Ground : BaseGround {
	// Properties
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!

	// Getters (Private)
//	private bool IsInvincible { get { return numBouncesLeft < 0; } }


    static public Color GetBodyColor(Ground g) {
        return GetBodyColor(g.MyRoom.WorldIndex, g.isBouncy, g.mayBounce, g.doRechargePlayer); }
    static public Color GetBodyColor(GroundData g, int worldIndex) { return GetBodyColor(worldIndex, g.isBouncy, g.mayBounce, g.doRechargePlayer); }
    static public Color GetBodyColor(int worldIndex, bool isBouncy, bool mayBounce, bool doRechargePlayer) {
		Color color = Colors.GroundBaseColor(worldIndex);
        if (isBouncy) { // Bouncy? Brighten it much!
            ColorHSB colorHSB = new ColorHSB(color);
            colorHSB.s = Mathf.Min(1, 0.3f + colorHSB.s*1.4f);
            colorHSB.b = Mathf.Min(1f, 0.3f + colorHSB.b*1.4f);
            color = colorHSB.ToColor();
        }
		if (!mayBounce) {
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
	public void Initialize(Room _myRoom, GroundData data) {
		base.BaseGroundInitialize(_myRoom, data);

		mayBounce = data.mayBounce;
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
            mayBounce = mayBounce,
            doRechargePlayer = doRechargePlayer,
        };
        return data;
	}


}

