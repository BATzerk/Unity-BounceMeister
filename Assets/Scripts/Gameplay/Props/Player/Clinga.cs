using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clinga : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Clinga; }
    // References
    private ClingaBody myClingaBody;

    // Getters
    private bool MayWarp() {
        return true;
    }
    private Vector2 GetWarpPos() {
        Rect r = MyRoom.MyRoomData.BoundsLocal;
        return new Vector2(Random.Range(r.xMin,r.xMax), Random.Range(r.yMin,r.yMax));
    }



    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myClingaBody = myBody as ClingaBody;

        base.Start();
    }


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonJump_Press() {
        if (MayWallKick()) {
            WallKick();
        }
        else if (MayJump()) {
            Jump();
        }
        else {
            ScheduleDelayedJump();
        }
    }

}
