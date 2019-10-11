using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warpa : Player {
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Warpa; }
    // References
    //private WarpaBody myWarpaBody;

    // Getters
    private bool MayWarp() {
        return true;
    }
    private Vector2 GetWarpPos() {
        Rect r = MyRoom.MyRoomData.BoundsLocalBL;
        return new Vector2(Random.Range(r.xMin,r.xMax), Random.Range(r.yMin,r.yMax));
    }



    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        //myWarpaBody = myBody as WarpaBody;

        base.Start();
    }


    // ----------------------------------------------------------------
    //  Input
    // ----------------------------------------------------------------
    override protected void OnButtonAction_Press() {
        if (MayWarp()) {
            Warp();
        }
    }


    // ----------------------------------------------------------------
    //  Flipping!
    // ----------------------------------------------------------------
    private void Warp() {
        pos = GetWarpPos();
    }

}
