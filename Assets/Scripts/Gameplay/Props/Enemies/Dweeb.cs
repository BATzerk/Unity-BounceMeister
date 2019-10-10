using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dweeb : Enemy {
    // Properties
    [SerializeField] private float speed = 0.06f;

    // Getters (Overrides)
    override protected int NumCoinsInMe { get { return 0; } }
    override protected float HorzMoveInputVelXDelta() {
        return speed;//dirMoving*MovementSpeedX;
    }

    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    override public void Initialize(Room _myRoom, PropData data) {
        base.Initialize(_myRoom, data);
        DweebData myData = data as DweebData;
        speed = myData.speed;
        UpdateBodyDirFacing();
    }

    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    override public void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);

        // A wall?? Reverse my horz direction!
        if (side==Sides.L || side==Sides.R) {
            FlipDirection();
        }
    }
    override public void OnPlayerFeetBounceOnMe(Player player) {
        if (IsInvincible) { return; } // Invincible? Do nothin'.
        TakeDamage(1);
    }
    override protected void OnNoticeWalkingOffLedge() {
        FlipDirection();
    }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateBodyDirFacing() {
        int dirMoving = MathUtils.Sign(speed, false);
        sr_body.transform.localScale = new Vector3(dirMoving, 1, 1);
    }
    private void FlipDirection() {
        speed *= -1;
        UpdateBodyDirFacing();
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


