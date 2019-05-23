using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JettaBody : PlayerBody {
    // Components
    [SerializeField] private SpriteRenderer sr_jetFill=null;
    // Properties
    private readonly Color c_bodyNoFuel = new Color(0.3f,0.3f,0.3f, 0.4f);
    private readonly Color bodyColor_jetting = new Color(0.5f,0.5f,0.5f);
    [SerializeField] private Color c_fillNormal=Color.white;
    [SerializeField] private Color c_fillEnding=Color.white;
    private float FillWidth; // set in Start.
    private float FillHeightFull; // set in Start.
	// References
	private Jetta myJetta;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		myJetta = myBasePlayer as Jetta;
		base.Start();
        
        FillWidth = myJetta.Size.x;
        FillHeightFull = myJetta.Size.y;
        UpdateFillSprite();
	}
    

    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SizeFillSprite(float percentFull) {
        float _height = percentFull * FillHeightFull;
        GameUtils.SizeSpriteRenderer(sr_jetFill, FillWidth,_height);
        sr_jetFill.transform.localPosition = new Vector3(0, (-FillHeightFull+_height)*0.5f);
    }
    

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
	public void OnStartJet() {
		SetBodyColor(bodyColor_jetting);
        eyes.Set(EyeTypes.Squint);
        UpdateFillSprite();
	}
	public void OnStopJet() {
        UpdateFillSprite();
        // Depleted?
        if (myJetta.IsFuelEmpty) {
            SetBodyColor(c_bodyNoFuel);
        }
        // NOT depleted?
        else {
            SetBodyColor(c_bodyNeutral);
            eyes.Set(EyeTypes.Normal);
        }
	}
	public void OnRechargeJet() {
        UpdateFillSprite();
        if (!myJetta.IsJetting) { // If I'm no longer jetting...
            SetBodyColor(c_bodyNeutral);
            eyes.Set(EyeTypes.Normal);
        }
	}



    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        if (myJetta.IsJetting) {
            UpdateFillSprite();
        }
    }
    private void UpdateFillSprite() {
        if (myJetta == null) { return; } // Safety check.
        // Show/hide it.
        sr_jetFill.enabled = !myJetta.IsFuelFull;
        // Size it.
        float percentFull = myJetta.FuelLeft / Jetta.FuelCapacity;
        SizeFillSprite(percentFull);
        // Set color/alpha.
        float fillAlpha;
        if (myJetta.IsJetting) {
            if (percentFull > 0.3f) { // Oscillate normally.
                sr_jetFill.color = c_fillNormal;
                fillAlpha = MathUtils.SinRange(0.8f,1f, percentFull*30f);
            }
            else { // Almost out? Oscillate FAST!
                sr_jetFill.color = c_fillEnding;
                fillAlpha = MathUtils.SinRange(0.3f,1f, percentFull*90f);
            }
        }
        else {
            fillAlpha = 0.5f;
        }
        GameUtils.SetSpriteAlpha(sr_jetFill, fillAlpha);
    }

}
