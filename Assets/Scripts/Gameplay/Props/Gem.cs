using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : Prop, ISerializableData<GemData> {
	// Components
	[SerializeField] private BoxCollider2D myCollider=null;
	[SerializeField] private ParticleSystem ps_collectedBurst=null;
	[SerializeField] private SpriteRenderer sr_body=null;
	[SerializeField] private GameObject go_body=null;
	// References
	private Player playerHoldingMe=null; // Player's gotta land on safe ground before they can eat me.
	// Properties
	private bool isEaten=false;
    private bool wasEverEaten=false; // Gems that've been eaten in the past show up as ghosts.
    private int myIndex; // used to save/load who was eaten.

    // Getters (Private)
    private float bodyRotation {
		get { return go_body.transform.localEulerAngles.z; }
		set { go_body.transform.localEulerAngles = new Vector3(0, 0, value); }
	}
	private Vector2 bodyPos {
		get { return go_body.transform.localPosition; }
		set { go_body.transform.localPosition = value; }
	}


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, GemData data, int myIndex) {
		base.BaseInitialize(_myLevel, data);
        this.myIndex = myIndex;

        // Load wasEverEaten!
        wasEverEaten = SaveStorage.GetBool(SaveKeys.DidEatGem(myLevel, myIndex));

        // Set wasEverEaten visuals.
        if (wasEverEaten) {
            sr_body.color = new Color(0.2f,0.2f,0.2f, 0.25f);
        }
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D otherCol) {
		// Player??
		Player player = otherCol.GetComponent<Player>();
		if (player != null) {//LayerMask.LayerToName(otherCol.gameObject.layer) == Layers.Player) {
			player.OnTouchGem(this);
		}
	}
	public void OnPlayerPickMeUp(Player player) {
		playerHoldingMe = player;
		// Oh, disable my gridSnap script so it doesn't interfere with our positioning.
		GridSnapPos snapScript = GetComponent<GridSnapPos>();
		snapScript.enabled = false;
	}
	public void GetEaten() {
        // Update and save!
        isEaten = true;
        wasEverEaten = true;
        SaveStorage.SetBool(SaveKeys.DidEatGem(myLevel, myIndex), true);
        // Visuals!
        bodyRotation = 0;
		myCollider.enabled = false;
		sr_body.enabled = false;
		ps_collectedBurst.Emit(15);
		playerHoldingMe = null;
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate() {
		UpdateBodyPosRot();
	}
	private void UpdateBodyPosRot() {
		if (isEaten) { return; } // If I'm toast, don't do any position updating.

		bodyRotation = Mathf.Sin(Time.time*1.4f) * 20f;

		Vector2 driftOffset = new Vector2(
			Mathf.Cos(Time.time*3f) * 0.2f,
			Mathf.Sin(Time.time*4f)*0.3f);
		Vector2 targetPos;
		if (playerHoldingMe != null) {
			targetPos = playerHoldingMe.PosLocal + new Vector2(0, 3.3f);
		}
		else {
			targetPos = this.pos;
		}
		targetPos += driftOffset;

		// Make it relative.
		targetPos -= this.pos;
		bodyPos += (targetPos - bodyPos) / 6f;
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public GemData SerializeAsData() {
		GemData data = new GemData();
		data.pos = pos;
		return data;
	}

}
