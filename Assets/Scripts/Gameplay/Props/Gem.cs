﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : Edible {
    // Properties
    [SerializeField] private int type = 0; // 0 is action, 1 is puzzle.


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, GemData data, int myIndex) {
		base.InitializeAsProp(_myRoom, data);
        this.myIndex = myIndex;
        this.type = data.type;

        // Load wasEverEaten!
        wasEverEaten = SaveStorage.GetBool(SaveKeys.DidEatGem(MyRoom, myIndex));

        // Set wasEverEaten visuals.
        if (wasEverEaten) {
            sr_body.color = new Color(0.2f,0.2f,0.2f, 0.25f);
        }

        // Apply type visuals.
        sr_body.sprite = ResourcesHandler.Instance.GetGemSprite(type);
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
	override public void GetEaten() {
        base.GetEaten();
        // Save the value!
        SaveStorage.SetBool(SaveKeys.DidEatGem(MyRoom, myIndex), true);
        // Particle bursttt
        ps_collectedBurst.Emit(16);
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

        float oscOffset = myIndex*1.5f; // if multiple Gems in a room, this offsets their floaty animation.
        Vector2 driftOffset = new Vector2(
			Mathf.Cos(oscOffset+Time.time*3f) * 0.2f,
			Mathf.Sin(oscOffset+Time.time*4f) * 0.3f);
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
    override public PropData ToData() {
        GemData data = new GemData {
            pos = pos,
            type = type,
            travelMind = new TravelMindData(travelMind),
        };
        return data;
	}

}
