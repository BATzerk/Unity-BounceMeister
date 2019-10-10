using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dweeb : Enemy {
    // Properties
    [SerializeField] private float speed = 0.06f;

    // Getters (Overrides)
    override protected float HorzMoveInputVelXDelta() {
        return speed;//dirMoving*MovementSpeedX;
    }


    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    override public void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);

        // A wall?? Reverse my horz direction!
        if (side==Sides.L || side==Sides.R) {
            speed *= -1;
            int dirMoving = MathUtils.Sign(speed);
            sr_body.transform.localScale = new Vector3(dirMoving, 1, 1);
        }
    }
    override public void OnPlayerFeetBounceOnMe(Player player) {
        if (IsInvincible) { return; } // Invincible? Do nothin'.
        TakeDamage(1);
    }
    

    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData ToData() {
        return new DweebData {
            pos = pos,
            speed = speed,
        };
    }
    
}


