using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public sealed class ToggleGround : BaseGround {
	// Properties
	[SerializeField] private bool startsOn=false;
	private bool pstartsOn;
	private bool isOn;
	private bool isPlayerInMe=false;
	private bool isWaitingToTurnOn; // set to TRUE if we wanna turn on, but a Player's in me! In this case, we'll turn on, but not apply it until the Player's left me.
	private Color bodyColorOn, bodyColorOff;


	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();

		bodyColorOn = startsOn ? new Color(3/255f, 170/255f, 204/255f) : new Color(217/255f, 74/255f, 136/255f);
		bodyColorOff = new Color(bodyColorOn.r,bodyColorOn.g,bodyColorOn.b, bodyColorOn.a*0.14f);

		SetIsOn(startsOn);

		// Add event listeners!
		GameManagers.Instance.EventManager.PlayerStartPlungeEvent += OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerSpendBounceEvent += OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerJumpEvent += OnPlayerDidSomething;
	}
	private void OnDestroy() {
		// Remove event listeners!
		GameManagers.Instance.EventManager.PlayerStartPlungeEvent -= OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerSpendBounceEvent -= OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerJumpEvent -= OnPlayerDidSomething;
	}
	public void Initialize(Level _myLevel, ToggleGroundData data) {
		base.BaseGroundInitialize(_myLevel, data);

		startsOn = data.startsOn;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
//	private void OnPlayerDash(Player player) {
//		ToggleIsOn();
//	}
	private void OnPlayerDidSomething(Player player) {
		ToggleIsOn();
	}
//	private void OnTriggerExit2D(Collider2D otherCol) {
//		// Ground??
//		if (LayerMask.LayerToName(otherCol.gameObject.layer) == LayerNames.Player) {
//			myPlayer.OnFeetLeaveGround ();
//		}
//	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void ToggleIsOn() {
		SetIsOn (!isOn);
	}
	private void SetIsOn(bool _isOn) {
		isOn = _isOn;

		isWaitingToTurnOn = _isOn && isPlayerInMe;
		if (!isWaitingToTurnOn) { // I can turn on right away! So do it!
			ApplyIsOn();
		}
	}

	private void ApplyIsOn() {
//		myCollider.isTrigger = !isOn;
		myCollider.enabled = isOn;
		bodySprite.color = isOn ? bodyColorOn : bodyColorOff;

		// TEMP fragile solution: If I have any children, totally also enable/disable their colliders and sprites!
		if (this.transform.childCount > 0) {
			Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
			SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
			foreach (Collider2D col in childColliders) {
				col.enabled = isOn;
			}
			foreach (SpriteRenderer sprite in childSprites) {
				GameUtils.SetSpriteAlpha(sprite, isOn ? 1f : 0.14f);
			}
		}
	}


//	private void FixedUpdate() {
//		if (isWaitingToTurnOn && !isPlayerInMe) {
//			isWaitingToTurnOn = false;
//			ApplyIsOn();
//		}
//	}
//	private void OnCollisionEnter2D(Collision2D col) {
//		if (IsPlayer(col)) {
//			isPlayerInMe = true;
//		}
//	}
//	private void OnCollisionExit2D(Collision2D col) {
//		if (IsPlayer(col)) {
//			isPlayerInMe = false;
//		}
//	}
//	private void OnCollisionStay2D(Collision2D col) {
//		if (IsPlayer(col)) {
//			isPlayerInMe = true;
//		}
//	}


	// Editor Update
	private void Update() {
		// Kinda hacky just for the editor.
		if (pstartsOn != startsOn) {
			SetIsOn(startsOn);
		}
		pstartsOn = startsOn;
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
		ToggleGroundData data = new ToggleGroundData();
		data.myRect = MyRect();
		data.canEatGems = CanEatEdibles;
        data.isPlayerRespawn = IsPlayerRespawn;
        data.startsOn = startsOn;
		return data;
	}


}
