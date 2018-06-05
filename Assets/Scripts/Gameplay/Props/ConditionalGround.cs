using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ConditionalGround : BaseGround {
	// Properties
	[SerializeField] private bool isOffWhenPlungeSpent = false; // we disappear when the Player spends their bounce, and re-appear when the Player recharges!
	[SerializeField] private bool isOffWhenBounceRecharged = false; // the exact INVERSE of the above. (Obviously, these both can't be set to true.)
	// References
	[SerializeField] private Sprite s_bodyFull=null;
	[SerializeField] private Sprite s_bodyEmpty=null;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();

		if (isOffWhenBounceRecharged) {
			TurnOff();
		}

		// Add event listeners!
		GameManagers.Instance.EventManager.PlayerStartPlungeEvent += OnPlayerStartPlunge;
		GameManagers.Instance.EventManager.PlayerRechargePlungeEvent += OnPlayerRechargeBounce;
	}
	private void OnDestroy() {
		// Remove event listeners!
		GameManagers.Instance.EventManager.PlayerStartPlungeEvent -= OnPlayerStartPlunge;
		GameManagers.Instance.EventManager.PlayerRechargePlungeEvent -= OnPlayerRechargeBounce;
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void TurnOn() {
		myCollider.enabled = true;
		bodySprite.sprite = s_bodyFull;
	}
	private void TurnOff() {
		myCollider.enabled = false;
		bodySprite.sprite = s_bodyEmpty;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnPlayerStartPlunge(Player player) {
		if (isOffWhenPlungeSpent) {
			TurnOff();
		}
		else if (isOffWhenBounceRecharged) {
			TurnOn();
		}
	}
	private void OnPlayerRechargeBounce(Player player) {
		if (isOffWhenPlungeSpent) {
			TurnOn();
		}
		else if (isOffWhenBounceRecharged) {
			TurnOff();
		}
	}


}
