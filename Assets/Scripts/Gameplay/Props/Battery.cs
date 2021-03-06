﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Prop {
	// Constants
	private float RegenDuration = 1f; // how many SECONDS until I regenerate.
	// Components
	[SerializeField] private ParticleSystem ps_usedBurst=null;
	[SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	private bool isUsed;
	private float timeWhenRegen;



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
    override protected void Start() {
        base.Start();
		SetIsUsed(false);
	}
	public void Initialize(Room _myRoom, BatteryData data) {
		base.InitializeAsProp(_myRoom, data);
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
        ps_usedBurst.Emit(6);
		SetIsUsed(true);
        GameManagers.Instance.EventManager.OnPlayerUseBattery();
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
        if (isUsed) { return; } // Used? Ignore collision.
		if (LayerMask.LayerToName(col.gameObject.layer) != Layers.Player) { // Ignore anything that's NOT the Player.
			return;
		}
		Player player = col.GetComponent<Player>();
		// The player is thirsty!!
		if (player.MayUseBattery()) {
			PlayerUseMe(player);
		}
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	override public PropData ToData() {
        BatteryData data = new BatteryData {
            pos = pos,
            travelMind = new TravelMindData(travelMind),
        };
        return data;
	}


}

