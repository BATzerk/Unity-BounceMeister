using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Ground : BaseGround, ITravelable {
	// Properties
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!

	// Getters (Private)
//	private bool IsInvincible { get { return numBouncesLeft < 0; } }


    static public Color GetBodyColor(Ground g) { return GetBodyColor(g.MyRoom.WorldIndex, g.isBouncy, g.mayBounce, g.doRechargePlayer); }
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
    private static Rect TrimmedRectToRoomBounds(Rect r, Room room) {
        Rect bounds = room.GetCameraBoundsLocal();
        bounds.yMin -= 1; // hacky-ish bloat top/bottom bounds.
        bounds.yMax += 1;
        return MathUtils.TrimRect(r, bounds);
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


    public override void Move(Vector2 delta) {
        Rect pr = MyRectBL(); // prev MyRect (bottom-left aligned).
        Rect nr = MyRectBL(); // new MyRect (bottom-left aligned).
        nr.position += delta; // Move.
        
        // INCREASE size.
        Rect camBounds = MyRoom.GetCameraBoundsLocal();
        Rect bA = MathUtils.BloatRect(camBounds, -0.8f); // bloated inward
        Rect bB = MathUtils.BloatRect(camBounds,  0.8f); // bloated outward
        if (pr.xMin<=bA.xMin && nr.xMin>=bB.xMin) { nr.xMin = bB.xMin; }
        if (pr.xMax>=bA.xMax && nr.xMax<=bB.xMax) { nr.xMax = bB.xMax; }
        if (pr.yMin<=bA.yMin && nr.yMin>=bB.yMin) { nr.yMin = bB.yMin; }
        if (pr.yMax>=bA.yMax && nr.yMax<=bB.yMax) { nr.yMax = bB.yMax; }
        
        // DECREASE size.
        nr = TrimmedRectToRoomBounds(nr, MyRoom); // finally, cut off the sides that aren't in bounds!
        
        // Return!
        nr.position = nr.center; // offset to CENTER aligned.
        SetMyRect(nr);
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
            travelMind = new TravelMindData(travelMind),
        };
        return data;
	}

}

