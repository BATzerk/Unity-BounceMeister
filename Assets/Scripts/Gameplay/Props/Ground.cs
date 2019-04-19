using System.Collections;
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
    static private Color GetBaseColor(int worldIndex) {
        //switch (worldIndex) {
        //    case 1: color = isBouncy ? new ColorHSB(190/360f, 0.73f, 0.83f).ToColor() : new ColorHSB(190/360f, 0.24f, 0.57f).ToColor(); break; // blue
        //    case 2: color = isBouncy ? new ColorHSB(280/360f, 0.76f, 0.83f).ToColor() : new ColorHSB(280/360f, 0.35f, 0.45f).ToColor(); break; // purple
        //    default: color = isBouncy ? new ColorHSB(76/360f, 0.84f, 0.83f).ToColor() : new ColorHSB(85/360f, 0.37f, 0.42f).ToColor(); break; // green
        //}
        switch (worldIndex) {
            case 0:  return new Color255( 83,104, 73).ToColor();
            case 1:  return new Color255( 84,101, 76).ToColor();
            case 2:  return new Color255( 53, 74, 72).ToColor();
            case 3:  return new Color255( 81, 63, 96).ToColor();
            case 4:  return new Color255(134,132,102).ToColor();
            case 5:  return new Color255( 67, 95, 81).ToColor();
            case 6:  return new Color255( 88, 88, 88).ToColor();
            case 7:  return new Color255(  1,  1,  1).ToColor();
            case 8:  return new Color255(254,254,254).ToColor();
            default: return new Color255( 84,101, 76).ToColor();
        }
    }
    static public Color GetBodyColor(int worldIndex, bool isBouncy, bool canBounce, bool doRechargePlayer) {
		Color color = GetBaseColor(worldIndex);
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

