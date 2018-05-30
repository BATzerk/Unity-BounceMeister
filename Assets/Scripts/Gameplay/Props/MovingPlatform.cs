using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {
	// Properties
	[SerializeField] private float oscillationOffset;
	private const float yPosA = -12f;
	private const float yPosB = -1f;
	private float xPosNeutral;
	private Vector3 posOffset, posOffsetVel;

	// Getters / Setters
	private Vector3 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}

	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		xPosNeutral = this.transform.localPosition.x;
		posOffset = posOffsetVel = Vector3.zero;
	}


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate () {
		UpdateAndApplyPosOffsetVel();
		UpdatePos();
	}

	private void UpdateAndApplyPosOffsetVel() {
		float elasticForce = 9f;
		posOffsetVel += new Vector3((0-posOffset.x)/elasticForce, (0-posOffset.y)/elasticForce);
		posOffsetVel *= 0.89f;
		posOffset += posOffsetVel;
	}

	private void UpdatePos() {
		float xPosLoc = 0.5f + Mathf.Cos(Time.time*0.2f+oscillationOffset)*0.5f;
		float xPos = Mathf.Lerp(xPosNeutral, xPosNeutral+6, xPosLoc);
		float yPosLoc = 0.5f + Mathf.Sin(Time.time*0.2f+oscillationOffset)*0.5f;
		float yPos = Mathf.Lerp(yPosA, yPosB, yPosLoc);
		pos = posOffset + new Vector3(xPos, yPos, pos.z);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnCollisionEnter2D(Collision2D collision) {
		GameObject collisionGO = collision.collider.gameObject;
		if (LayerMask.LayerToName(collisionGO.layer) == LayerNames.Player) {
			if (collision.relativeVelocity.y<0) { // Player's moving downward? Then it's landed on me!
				OnPlayerLandOnMe(collision);
			}
		}
	}
	private void OnPlayerLandOnMe(Collision2D collision) {
		// Push mee!
		posOffsetVel += new Vector3(-collision.relativeVelocity.x*0.005f, collision.relativeVelocity.y*0.016f);
	}

}
