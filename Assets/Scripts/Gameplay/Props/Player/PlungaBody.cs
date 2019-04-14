using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlungaBody : PlayerBody {
    // Components
    [SerializeField] private SpriteRenderer sr_highlight=null; // flashes white when recharge plunge.
    // Properties
    private readonly Color bodyColor_plunging = new Color(60/255f, 255/255f, 160/255f);
    private readonly Color bodyColor_plungeExhausted = new Color(0.2f,0.2f,0.2f, 0.3f);
	// References
	private Plunga myPlunga;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
        myPlunga = myBasePlayer as Plunga;

		base.Start();
        
        GameUtils.SizeSpriteRenderer(sr_highlight, myBasePlayer.Size);
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    override protected void SetVisualScale(Vector2 _scale) {
        base.SetVisualScale(_scale);
        GameUtils.SizeSpriteRenderer(sr_highlight, myBasePlayer.Size*_scale);
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnStartPlunge() {
		SetBodyColor(bodyColor_plunging);
        SetVisualScale(new Vector2(0.7f, 1.2f));
    }
	public void OnStopPlunge() {
        SetVisualScale(Vector2.one);
        if (myPlunga.IsPlungeRecharged) {
			SetBodyColor(bodyColor_neutral);
		}
		else {
			SetBodyColor(bodyColor_plungeExhausted);
		}
	}

	public void OnRechargePlunge() {
        SetVisualScale(Vector2.one);
        SetBodyColor(bodyColor_neutral);
        // Flash me white!
        GameUtils.SetSpriteAlpha(sr_highlight, 1f);
        LeanTween.cancel(sr_highlight.gameObject);
        LeanTween.alpha(sr_highlight.gameObject, 0, 0.2f).setEaseOutQuad();
	}


}
