using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressGate : BaseGround {
    // Components
    [SerializeField] private TextMesh myText=null;
	// Properties
    [SerializeField] private int numGemsReq;
    [SerializeField] private int numSnacksReq;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, ProgressGateData data) {
		base.BaseGroundInitialize(_myLevel, data);

        numGemsReq = data.numGemsReq;
        numSnacksReq = data.numSnacksReq;
		UpdateIsOpen();
        
        // TEMP set text
        string str = "";
        if (numGemsReq > 0) { str += "gems: " + numGemsReq + "\n"; }
        if (numSnacksReq > 0) { str += "snacks: " + numSnacksReq; }
        myText.text = str;
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
    private void UpdateIsOpen() {
        
    }
	private void SetIsOpen(bool isOpen) {
		myCollider.enabled = isOpen;
        Color bodyColor = Color.green;
		if (isOpen) {
			bodySprite.color = bodyColor;
		}
		else {
			bodySprite.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, 0.1f);
		}
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        ProgressGateData data = new ProgressGateData {
            myRect = MyRect(),
            numGemsReq = numGemsReq,
            numSnacksReq = numSnacksReq,
        };
        return data;
	}

}
