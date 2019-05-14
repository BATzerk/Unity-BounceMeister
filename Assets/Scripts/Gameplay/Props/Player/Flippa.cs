using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flippa : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Flippa; }
    //override protected Vector2 Gravity { get { return new Vector2(0, -0.032f); } }
    //override protected float WallSlideMinYVel { get { return -0.10f; } }
    //override protected Vector2 WallKickVel { get { return new Vector2(0.4f,0.42f); } }
    // References
    private FlippaBody myFlippaBody;

    // Getters
    private bool MayFlipGravity() {
        return true;
    }



    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myFlippaBody = myBody as FlippaBody;

        base.Start();
    }


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Press() {
        if (MayWallKick()) {
            WallKick();
        }
        else if (MayFlipGravity()) {
            FlipGravity();
        }
    }
    //override protected void OnButtonAction_Press() {
    //    if (CanStartPlunge()) {
    //        StartPlunge();
    //    }
    //}


    // ----------------------------------------------------------------
    //  Flipping!
    // ----------------------------------------------------------------
    private void FlipGravity() {
        
    }

}
