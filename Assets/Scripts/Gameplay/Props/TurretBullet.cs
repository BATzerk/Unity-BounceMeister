using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** TurretBullets come out of Turrets. */
public class TurretBullet : MonoBehaviour {
    // Constants
    const float MaxLifetime = 20; // in SECONDS.
    // Properties
    private bool isDead = false;
    private Vector2 vel;
    private float timeUntilDie; // in SECONDS.
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
        timeUntilDie = MaxLifetime;
        // Make our dieBounds a bit outside of the room.
        dieBounds = MathUtils.BloatRect(myRoom.GetCameraBoundsLocal(), 6);
    }

    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        if (isDead) { return; } // Safety check.
        ApplyVel();
        timeUntilDie -= GameTimeController.RoomDeltaTime;
        MaybeDie();
    }
    private void ApplyVel() {
        pos += vel * GameTimeController.RoomScale;
    }
    private void MaybeDie() {
        if (timeUntilDie <= 0 // outta time?
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
    private void OnTriggerEnter2D(Collider2D coll) {
        // IGNORE all collsions for first moment after birth.
        if (timeUntilDie>MaxLifetime-0.1f) { return; }
        if (coll.isTrigger) { return; } // Ignore if THEY're a trigger too (i.e. DispGround and ToggleGround).
        // Ground?? Die!
        if (LayerUtils.IsLayer(coll.gameObject, Layers.Ground)) {
            Die();
        }
    }


}
