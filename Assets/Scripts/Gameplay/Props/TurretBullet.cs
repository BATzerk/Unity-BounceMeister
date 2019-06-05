﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** TurretBullets come out of Turrets. */
public class TurretBullet : MonoBehaviour {
    // Constants
    const float MaxLifetime = 20; // in SECONDS.
    // Properties
    private bool isDead = false;
    private Vector2 vel;
    private float timeBorn; // in SECONDS.
    private Rect dieBounds; // we die if we EXIT these bounds.

    // Getters / Setters
	protected Vector2 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room myRoom, Turret myTurret) {
        // Parent jazz!
        GameUtils.ParentAndReset(this.gameObject, myRoom.transform);

        this.pos = myTurret.PosLocal;
        this.vel = MathUtils.GetVectorFromDeg(myTurret.rotation) * myTurret.Speed;
        //this.GetComponent<Rigidbody2D>().velocity = this.vel * 30;//QQQ hacks
        timeBorn = Time.time;
        // Make our dieBounds a bit outside of the room.
        dieBounds = MathUtils.BloatRect(myRoom.GetCameraBoundsLocal(), 6);
    }

    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        if (isDead) { return; } // Safety check.
        ApplyVel();
        MaybeDie();
    }
    private void ApplyVel() {
        pos += vel * GameTimeController.RoomScale;
    }
    private void MaybeDie() {
        if (Time.time > timeBorn+MaxLifetime // outta time?
        || !dieBounds.Contains(pos)) { // outta bounds?
            Die();
        }
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void Die() {
        if (isDead) { return; } // Safety check.
        isDead = true;
        // Add a particle burst!
        GameObject burst = Instantiate(ResourcesHandler.Instance.TurretBulletBurst);
        GameUtils.ParentAndReset(burst.gameObject, this.transform.parent);
        burst.transform.localPosition = pos;
        Destroy(burst, 3); // destroy it when it's done.
        // Destroy meee.
        Destroy(this.gameObject);
    }


    // ----------------------------------------------------------------
    //  Physics Events
    // ----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collider) {
        // IGNORE all collsions for first moment after birth.
        if (Time.time < timeBorn+0.1f) { return; }
        // Ground?? Die!
        if (LayerUtils.IsLayer(collider.gameObject, Layers.Ground)) {
            Die();
        }
    }


}