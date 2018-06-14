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
	public void Initialize(Level _myLevel, GemData data) {
		base.BaseInitialize(_myLevel, data);
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
		bodyRotation = 0;
		isEaten = true;
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
			targetPos = playerHoldingMe.PosLocal + new Vector2(0, 2.8f);
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
