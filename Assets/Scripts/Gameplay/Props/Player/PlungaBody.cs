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
		bodyColor_neutral = new Color(25/255f, 175/255f, 181/255f);

		base.Start();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    override public void SetSize(Vector2 _size) {
        base.SetSize(_size);
        GameUtils.SizeSpriteRenderer(sr_highlight,_size);
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnStartPlunge() {
		SetBodyColor(bodyColor_plunging);
        SetSize(new Vector2(1f,2f));//TEST
    }
	public void OnStopPlunge() {
        SetSize(myBasePlayer.Size);//new Vector2(2f,1.1f));//TEST
        if (myPlunga.IsPlungeRecharged) {
			SetBodyColor(bodyColor_neutral);
		}
		else {
			SetBodyColor(bodyColor_plungeExhausted);
		}
	}

	public void OnRechargePlunge() {
		SetBodyColor(bodyColor_neutral);
        SetSize(myBasePlayer.Size);//TEST
        // Flash me white!
        GameUtils.SetSpriteAlpha(sr_highlight, 1f);
        LeanTween.cancel(sr_highlight.gameObject);
        LeanTween.alpha(sr_highlight.gameObject, 0, 0.2f).setEaseOutQuad();
	}


}
