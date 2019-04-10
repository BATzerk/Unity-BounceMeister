using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressGate : BaseGround {
    // Components
    [SerializeField] private TextMesh myText=null;
	// Properties
    [SerializeField] private int numSnacksReq;
    
    // Getters (Private)
    private WorldData currWorldData { get { return GameManagers.Instance.DataManager.CurrWorldData; } }
    private int NumSnacksColl { get { return currWorldData.NumSnacksCollected; } }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, ProgressGateData data) {
		base.BaseGroundInitialize(_myLevel, data);
        
        myText.GetComponent<Renderer>().sortingOrder = 11;

        numSnacksReq = data.numSnacksReq;
        UpdateText();
		UpdateIsOpen();
	}
    override protected void Start() {
        base.Start();
        // Add event listeners!
        GameManagers.Instance.EventManager.SnacksCollectedChangedEvent += OnSnacksCollectedChanged;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.SnacksCollectedChangedEvent -= OnSnacksCollectedChanged;
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSnacksCollectedChanged(int worldIndex) {
        UpdateText();
        UpdateIsOpen();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateText() {
        myText.text = "snacks:\n" + NumSnacksColl + " / " + numSnacksReq;
    }
    private void UpdateIsOpen() {
        bool isOpen = NumSnacksColl >= numSnacksReq;
        SetIsOpen(isOpen);
    }
	private void SetIsOpen(bool isOpen) {
		myCollider.enabled = !isOpen;
        Color bodyColor = Color.green;
		if (isOpen) {
            bodySprite.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, 0.1f);
        }
        else {
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
