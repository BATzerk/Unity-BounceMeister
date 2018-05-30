using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ToggleGround : MonoBehaviour {
	// Components
	[SerializeField] private SpriteRenderer sr_fill=null;
	[SerializeField] private BoxCollider2D myCollider=null;
	// Properties
	[SerializeField] private bool startsOn=false;
	private bool isOn;
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
		GameManagers.Instance.EventManager.PlayerDashEndEvent += OnPlayerDashEnd;
	}
	private void OnDestroy() {
		// Remove event listeners!
//		GameManagers.Instance.EventManager.PlayerDashEvent -= OnPlayerDash;
		GameManagers.Instance.EventManager.PlayerDashEndEvent -= OnPlayerDashEnd;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
//	private void OnPlayerDash(Player player) {
//		ToggleIsOn();
//	}
	private void OnPlayerDashEnd(Player player) {
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
		myCollider.isTrigger = !isOn;
		sr_fill.color = isOn ? bodyColorOn : bodyColorOff;
	}




}
