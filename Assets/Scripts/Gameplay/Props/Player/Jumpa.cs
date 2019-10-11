using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumpa : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Jumpa; }
    protected override void InitMyPhysicsValues() {
        base.InitMyPhysicsValues();
        GravityNeutral = new Vector2(0, -0.042f);
    }
    // Constants
    private int MaxJumps = 2;
	// Properties
    private int numJumpsLeft;
    private int numJumpsSinceGround;
	//private bool isPlunging = false;
	//private bool isPlungeRecharged = true;
	// References
	private JumpaBody myJumpaBody;


	// Getters (Public)
    protected override bool MayJump() {
        return IsJumpRecharged;
    }
    public bool IsJumpRecharged { get { return numJumpsLeft > 0; } }
	override public bool MayUseBattery() { return numJumpsLeft < MaxJumps; }
    //public bool IsPlungeRecharged { get { return isPlungeRecharged; } }



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
		myJumpaBody = myBody as JumpaBody;

		base.Start();
	}


    // ----------------------------------------------------------------
    //  Events (Physics)
    // ----------------------------------------------------------------
    protected override void Jump() {
        base.Jump();
        numJumpsLeft --;
        numJumpsSinceGround ++;
    }
    //protected override void OnFeetTouchCollidable(Collidable collidable) {
    //    RechargeJumps();
    //    numJumpsSinceGround = 0;
    //    base.OnFeetTouchCollidable(collidable);
    //}
    override protected void LandOnCollidable(Collidable collidable) {
		// Landing on a surface stops the plunge.
		numJumpsSinceGround = 0;
        // Is this collidable refreshing? Recharge my plunge!
        if (collidable==null || collidable.DoRechargePlayer) {
            RechargeJumps();
		}
		// Base call.
		base.LandOnCollidable(collidable);
	}
    private void RechargeJumps() {
        if (numJumpsLeft < MaxJumps) {
            numJumpsLeft = MaxJumps;
            myJumpaBody.OnRechargeJumps();
        }
    }


	override public void OnUseBattery() {
        base.OnUseBattery();
		numJumpsLeft = MaxJumps;
	}

}
