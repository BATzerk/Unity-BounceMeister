using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ToggleGround : Collidable {
	// Components
	[SerializeField] private SpriteRenderer sr_fill=null;
	[SerializeField] private BoxCollider2D myCollider=null;
	// Properties
	[SerializeField] private bool startsOn=false;
	private bool pstartsOn;
	private bool isOn;
	private bool isPlayerInMe;
	private bool isWaitingToTurnOn; // set to TRUE if we wanna turn on, but a Player's in me! In this case, we'll turn on, but not apply it until the Player's left me.
	private Color bodyColorOn, bodyColorOff;


	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start () {
		bodyColorOn = startsOn ? new Color(3/255f, 170/255f, 204/255f) : new Color(217/255f, 74/255f, 136/255f);
		bodyColorOff = new Color(bodyColorOn.r,bodyColorOn.g,bodyColorOn.b, bodyColorOn.a*0.14f);

		// Size our sliced sprite properly!
//		sr_stroke.transform.localScale = new Vector3(1/this.transform.localScale.x, 1/this.transform.localScale.y, 1);
//		sr_stroke.sprite.

		SetIsOn(startsOn);

		// Add event listeners!
		//		GameManagers.Instance.EventManager.PlayerDashEvent += OnPlayerDash;
		GameManagers.Instance.EventManager.PlayerStartPlungeEvent += OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerSpendBounceEvent += OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerJumpEvent += OnPlayerDidSomething;
	}
	private void OnDestroy() {
		// Remove event listeners!
		//		GameManagers.Instance.EventManager.PlayerDashEvent -= OnPlayerDash;
		GameManagers.Instance.EventManager.PlayerStartPlungeEvent -= OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerSpendBounceEvent -= OnPlayerDidSomething;
//		GameManagers.Instance.EventManager.PlayerJumpEvent -= OnPlayerDidSomething;
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
		sr_fill.color = isOn ? bodyColorOn : bodyColorOff;

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




}
