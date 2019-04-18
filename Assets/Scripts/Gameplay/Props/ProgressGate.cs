using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressGate : BaseGround {
    // Components
    [SerializeField] private SpriteRenderer sr_snackIcon=null;
    [SerializeField] private TextMesh myText=null;
	// Properties
    [SerializeField] private int numSnacksReq;
    
    // Getters (Private)
    private WorldData currWorldData { get { return GameManagers.Instance.DataManager.CurrWorldData; } }
    private int NumSnacksEaten { get { return GameManagers.Instance.DataManager.NumSnacksEaten; } }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, ProgressGateData data) {
		base.BaseGroundInitialize(_myRoom, data);

        numSnacksReq = data.numSnacksReq;
        UpdateText();
		UpdateIsOpen();
	}
    override protected void Start() {
        base.Start();
        
        myText.GetComponent<Renderer>().sortingOrder = 11;
        
        // Add event listeners!
        GameManagers.Instance.EventManager.NumSnacksEatenChangedEvent += OnNumSnacksEatenChanged;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.NumSnacksEatenChangedEvent -= OnNumSnacksEatenChanged;
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnNumSnacksEatenChanged() {
        UpdateText();
        UpdateIsOpen();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateText() {
        myText.text = NumSnacksEaten + " / " + numSnacksReq;
    }
    private void UpdateIsOpen() {
        bool isOpen = NumSnacksEaten >= numSnacksReq;
        SetIsOpen(isOpen);
    }
	private void SetIsOpen(bool isOpen) {
		myCollider.enabled = !isOpen;
        Color bodyColor = new Color255(35, 94, 42).ToColor();
		if (isOpen) {
            GameUtils.SetSpriteAlpha(sr_snackIcon, 0.1f);
            bodySprite.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, 0.1f);
        }
        else {
            GameUtils.SetSpriteAlpha(sr_snackIcon, 1);
            bodySprite.color = bodyColor;
		}
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        ProgressGateData data = new ProgressGateData {
            myRect = MyRect(),
            //numGemsReq = numGemsReq,
            numSnacksReq = numSnacksReq,
        };
        return data;
	}

}
