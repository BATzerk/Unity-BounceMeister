using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JettaBody : PlayerBody {
    // Components
    [SerializeField] private SpriteRenderer sr_jetFill=null;
    // Properties
    private readonly Color bodyColor_noFuel = new Color(0.3f,0.3f,0.3f, 0.4f);
    private readonly Color bodyColor_jetting = Color.gray;
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
        //GameUtils.SizeSpriteRenderer(sr_jetFill, myJetta.Size);
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
	//  Update
	// ----------------------------------------------------------------
    public void UpdateFillSprite() {
        if (myJetta == null) { return; } // Safety check.
        // Size it.
        float percentFull = myJetta.FuelLeft / Jetta.FuelCapacity;
        SizeFillSprite(percentFull);
        // Show/hide it.
        sr_jetFill.enabled = !myJetta.IsFuelFull;
        GameUtils.SetSpriteAlpha(sr_jetFill, myJetta.IsJetting ? 1 : 0.5f);
    }
    

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
	public void OnStartJet() {
		SetBodyColor(bodyColor_jetting);
        UpdateFillSprite();
	}
	public void OnStopJet() {
        UpdateFillSprite();
		if (myJetta.IsFuelEmpty) {
			SetBodyColor(bodyColor_noFuel);
		}
		else {
			SetBodyColor(bodyColor_neutral);
		}
	}

	public void OnRechargeJet() {
        UpdateFillSprite();
		SetBodyColor(bodyColor_neutral);
	}


}
