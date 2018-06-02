﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour {
	// Components
	[SerializeField] private SpriteRenderer sr_body=null;
	[SerializeField] private SpriteLine sl_aimDir=null;
	[SerializeField] private ParticleSystem ps_dieBurst;
	// Properties
	private readonly Color bodyColor_neutral = new Color(25/255f, 175/255f, 181/255f);
	private Color bodyColor;
	private float alpha; // we modify this independently of bodyColor.
	// References
	[SerializeField] private Player myPlayer=null;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		alpha = 1;
		bodyColor = bodyColor_neutral;
		ApplyBodyColor();
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void SetSize(Vector2 _size) {
		GameUtils.SizeSpriteRenderer(sr_body, _size);
		sl_aimDir.SetColor(Color.white);
		sl_aimDir.SetThickness(_size.magnitude*0.12f);
		sl_aimDir.StartPos = Vector2.zero;
//		aimDirRadius = Mathf.Min(_size.x,_size.y) * 0.8f;
	}
	private void ApplyBodyColor() {
		sr_body.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, bodyColor.a*alpha);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnStartBouncing() {
		bodyColor = Color.green;
		ApplyBodyColor();
	}
	public void OnStopBouncing() {
		bodyColor = bodyColor_neutral;
		ApplyBodyColor();
	}

	public void OnSpendBounce() {
		bodyColor = Color.gray;
		ApplyBodyColor();
	}
	public void OnRechargeBounce() {
		bodyColor = bodyColor_neutral;
		ApplyBodyColor();
	}

	public void OnEndPostDamageImmunity() {
		alpha = 1f;
		ApplyBodyColor();
	}

	public void OnDie() {
		// Cheap way to get a particle burst: Just chuck my ParticleSystem onto my Player's parent transform the moment before we're destroyed!
		ps_dieBurst.gameObject.SetActive(true);
		ps_dieBurst.transform.SetParent(myPlayer.transform.parent);
		ps_dieBurst.Emit(40);
//		print("Enabled? " + ps_dieBurst.inheritVelocity.enabled);
		// Give all the particles the velocity of my Player!
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps_dieBurst.particleCount];
		ps_dieBurst.GetParticles(particles);
		for (int i=0; i<particles.Length; i++) {
			ParticleSystem.Particle particle = particles[i];
			float velScale = Random.Range(30f, 60f);
			particle.velocity += new Vector3(myPlayer.Vel.x, myPlayer.Vel.y, 0) * velScale;
			particles[i] = particle;
		}
		// Set the array back to the particleSystem
		ps_dieBurst.SetParticles(particles, particles.Length);
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update() {
		if (Time.timeScale == 0) { return; } // No time? No dice.

		if (myPlayer.IsPostDamageImmunity) {
			alpha = Random.Range(0.2f, 0.6f);
			ApplyBodyColor();
		}
	}




}

/*
	public void OnDash() {
//		Color color;
//		switch (myPlayer.NumDashesSinceGround) {
//			case 1: color = bodyColor_dashing1; break;
////			case 2: color = bodyColor_dashing2; break;
//			default: color = bodyColor_outOfDashes; break;
//		}
//		sr_body.color = color;
	}
	public void OnDashEnd() {
//		sr_body.color = bodyColor_neutral;
//		Color color;
//		switch (myPlayer.NumDashesSinceGround) {
//		case 0: color = bodyColor_neutral; break;
//		case Player.MaxDashes-2: color = bodyColor_dashing1; break;
//		case Player.MaxDashes-1: color = bodyColor_dashing1; break;
//		case Player.MaxDashes:   color = bodyColor_outOfDashes; break;
//		default: color = bodyColor_neutral; break;
//		}
//		sr_body.color = color;
	}
	public void OnRechargeDash() {
		bodyColor = bodyColor_neutral;
		ApplyBodyColor();
	}
//	private void UpdateAimDirLine() {
//		sl_aimDir.StartPos = Vector2.zero;
//		sl_aimDir.EndPos = AimDir * aimDirRadius;
//	}


//	private void FixedUpdate() {
//		UpdateBodyColor();
//	}
//	private void UpdateBodyColor() {
//		if (myPlayer.IsDashing) {
//			sr_body.color = Color.Lerp(sr_body.color, bodyColor_dashing, 0.3f); // Ease FAST into our dashing color.
//		}
//	}
	*/


