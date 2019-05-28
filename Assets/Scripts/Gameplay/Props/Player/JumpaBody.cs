using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpaBody : PlayerBody {
    // Components
    [SerializeField] private SpriteRenderer sr_highlight=null; // flashes white when recharge plunge.
    // Properties
    private readonly Color bodyColor_jumpsExhausted = new Color(0.2f,0.2f,0.2f, 0.3f);
	// References
	private Jumpa myJumpa;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
        myJumpa = myBasePlayer as Jumpa;
		base.Start();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    //override protected void SetVisualScale(Vector2 _scale) {
    //    base.SetVisualScale(_scale);
    //    GameUtils.SizeSpriteRenderer(sr_highlight, myBasePlayer.Size*_scale);
    //}


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
	public void OnJump() {
        if (myJumpa.IsJumpRecharged) {
			SetBodyColor(c_bodyNeutral);
		}
		else {
			SetBodyColor(bodyColor_jumpsExhausted);
		}
	}

	public void OnRechargeJumps() {
        SetBodyColor(c_bodyNeutral);
        // Flash me white!
        GameUtils.SetSpriteAlpha(sr_highlight, 1f);
        LeanTween.cancel(sr_highlight.gameObject);
        LeanTween.alpha(sr_highlight.gameObject, 0, 0.2f).setEaseOutQuad();
	}


}
