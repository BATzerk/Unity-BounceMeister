﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour {
	// Constants
	private float RegenDuration = 2f; // how many SECONDS until I regenerate.
	// Components
	[SerializeField] private SpriteRenderer sr_body;
	// References
//	[SerializeField] private Sprite s_bodyFull;
//	[SerializeField] private Sprite s_bodyEmpty;
	// Properties
	private bool isUsed;
	private float timeWhenRegen;



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		SetIsUsed(false);
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		UpdateRegen();
	}
	private void UpdateRegen() {
		if (isUsed && Time.time >= timeWhenRegen) {
			Regenerate();
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void SetIsUsed(bool _isUsed) {
		isUsed = _isUsed;
		UpdateBodySprite();
	}
	private void UpdateBodySprite() {
//		sr_body.sprite = isUsed ? s_bodyEmpty : s_bodyFull;
		sr_body.enabled = !isUsed;
	}
	private void PlayerUseMe(Player player) {
		player.OnUseBattery();
		timeWhenRegen = Time.time + RegenDuration;
		SetIsUsed(true);
	}
	private void Regenerate() {
		SetIsUsed(false);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnTriggerEnter2D(Collider2D col) {
		OnTriggerEnterOrStay2D(col);
	}
	private void OnTriggerStay2D(Collider2D col) {
		OnTriggerEnterOrStay2D(col);
	}
	private void OnTriggerEnterOrStay2D(Collider2D col) {
		if (LayerMask.LayerToName(col.gameObject.layer) != LayerNames.Player) { // Ignore anything that's NOT the Player.
			return;
		}
		Player player = col.GetComponent<Player>();
		// The player is thirsty!!
		if (!player.IsBounceRecharged) {
			PlayerUseMe(player);
		}
	}



}
