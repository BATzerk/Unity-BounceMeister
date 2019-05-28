using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelingPlatform : Platform {
    // Components
    [SerializeField] private Transform tf_a=null;
    [SerializeField] private Transform tf_b=null;
    [SerializeField] private Transform tf_posContainer=null;
    // Properties
    [SerializeField] private float locOffset=0;
    [SerializeField] private float speed=1;
    private float oscLoc;

    // Getters / Setters
    private Vector2 posA {
        get { return tf_a.localPosition; }
        set { tf_a.localPosition = value; }
    }
    private Vector2 posB {
        get { return tf_b.localPosition; }
        set { tf_b.localPosition = value; }
    }



    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    override public void Initialize(Room _myRoom, PlatformData data) {
        base.Initialize(_myRoom, data);
        
        TravelingPlatformData tpd = data as TravelingPlatformData;
        locOffset = tpd.locOffset;
        speed = tpd.speed;
        posA = tpd.posA;
        posB = tpd.posB;
        oscLoc = locOffset; // start with my desired offset!
        
        UpdatePos();
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        Vector2 prevPos = pos;

        UpdatePos();

        SetVel(pos - prevPos);
        //float locNext = MathUtils.Sin01(oscLoc + Time.deltaTime * speed);
        //Vector2 bodyPosNext = Vector2.Lerp(tf_a.localPosition,tf_b.localPosition, locNext);
        //vel = bodyPosNext - pos;
    }
    private void UpdatePos() {
        oscLoc += Time.deltaTime * speed;
        float loc = MathUtils.Sin01(oscLoc);
        pos = Vector2.Lerp(tf_a.localPosition,tf_b.localPosition, loc);
        
        // Offset posContainer, so tf_posA and tf_posB are in the right relative spot.
        tf_posContainer.localPosition = -pos;
    }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
    	return new TravelingPlatformData {
            myRect = MyRect(),
            mayPlayerEat = MayPlayerEatHere,
            isPlayerRespawn = IsPlayerRespawn,
            canDropThru = CanDropThru,
            locOffset = locOffset,
            speed = speed,
            posA = posA,
            posB = posB,
        };
    }
}
