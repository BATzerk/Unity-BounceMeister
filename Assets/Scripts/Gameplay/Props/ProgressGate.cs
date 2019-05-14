using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressGate : BaseGround {
    //// Constants
    //private readonly Color bodyColor = new Color255(35, 94, 42).ToColor();
    // Components
    [SerializeField] private ParticleSystem ps_openBurst=null;
    [SerializeField] private SpriteRenderer sr_snackAura=null;
    [SerializeField] private SpriteRenderer sr_snackIcon=null;
    [SerializeField] private TextMesh myText=null;
	// Properties
    [SerializeField] private int numSnacksReq;
    private bool isOpen;
    private bool isReadyToOpen; // true when we've got enough Snacks, but HAVEN'T yet been touched.
    private int myIndex;
    
    // Getters (Private)
    private WorldData currWorldData { get { return GameManagers.Instance.DataManager.CurrWorldData; } }
    private int TotalSnacksEatenGame { get { return GameManagers.Instance.DataManager.SnackCountGame.Eaten_All; } }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, ProgressGateData data, int myIndex) {
        this.myIndex = myIndex;
		base.BaseGroundInitialize(_myRoom, data);

        numSnacksReq = data.numSnacksReq;
        UpdateText();
        // Load openness!
        isOpen = SaveStorage.GetBool(SaveKeys.IsProgressGateOpen(MyRoom, myIndex));
		UpdateIsReadyToOpen();
        UpdateOpennessVisuals();
	}
    override protected void Start() {
        base.Start();
        
        myText.GetComponent<Renderer>().sortingOrder = 11;
        
        // Add event listeners!
        GameManagers.Instance.EventManager.SnackCountGameChangedEvent += OnSnackCountGameChanged;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.SnackCountGameChangedEvent -= OnSnackCountGameChanged;
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSnackCountGameChanged() {
        UpdateText();
        UpdateIsReadyToOpen();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateText() {
        myText.text = TotalSnacksEatenGame + " / " + numSnacksReq;
    }
    
    private void UpdateIsReadyToOpen() {
        isReadyToOpen = !isOpen && TotalSnacksEatenGame >= numSnacksReq;
    }
    //private void UpdateIsOpen() {
    //    bool isOpen = 
    //    SetIsOpen(isOpen);
    //}
	private void SetIsOpen(bool _isOpen) {
        isOpen = _isOpen;
        SaveStorage.SetBool(SaveKeys.IsProgressGateOpen(MyRoom, myIndex), isOpen);
        UpdateIsReadyToOpen();
        UpdateOpennessVisuals();
    }
    
    private void UpdateOpennessVisuals() {
		//myCollider.enabled = !isOpen;NOTE: DISABLED ProgressGates!!
		if (isOpen) {
            GameUtils.SetSpriteAlpha(sr_snackAura, 0.1f);
            GameUtils.SetSpriteAlpha(sr_snackIcon, 0.1f);
            GameUtils.SetSpriteAlpha(bodySprite, 0.1f);
            GameUtils.SetTextMeshAlpha(myText, 0.2f);
        }
        else {
            if (isReadyToOpen) { }
            else {
                GameUtils.SetSpriteAlpha(sr_snackAura, 0.5f);
                GameUtils.SetSpriteAlpha(sr_snackIcon, 1);
                GameUtils.SetSpriteAlpha(bodySprite, 1);
                GameUtils.SetTextMeshAlpha(myText, 1);
            }
		}
	}



    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision) {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null) {
            OnPlayerCollisionEnter(player);
        }
    }
    private void OnPlayerCollisionEnter(Player player) {
        if (isReadyToOpen) {
            // Open me!
            SetIsOpen(true);
            ps_openBurst.Emit(30);
            //// Boost the Player backward.
            //Vector2 force = 
            //player.vel += 
        }
    }
    

    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        if (isReadyToOpen) {
            float alpha = MathUtils.SinRange(0.3f,0.8f, Time.time*12f);
            GameUtils.SetSpriteAlpha(bodySprite, alpha);
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
