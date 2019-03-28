using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelingPlatform : BaseGround {//, ISerializableData<TravelingPlatformData> {
    // Components
    [SerializeField] private Transform tf_a;
    [SerializeField] private Transform tf_b;
    [SerializeField] private Transform tf_body;
    // Properties
    [SerializeField] private float oscOffset=0;
    [SerializeField] private float oscSpeed=1;

    // Getters / Setters
    private Vector2 bodyPos {
        get { return tf_body.localPosition; }
        set { tf_body.localPosition = value; }
    }



    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        Vector2 prevPos = bodyPos;

        UpdatePos();

        vel = bodyPos - prevPos;
    }
    private void UpdatePos() {
        float loc = MathUtils.Sin01(Time.time*oscSpeed);
        bodyPos = Vector2.Lerp(tf_a.localPosition, tf_b.localPosition, loc);
    }



    //// ----------------------------------------------------------------
    ////  Initialize
    //// ----------------------------------------------------------------
    //public void Initialize(Level _myLevel, PlatformData data) {
    //	base.BaseGroundInitialize(_myLevel, data);
    //}

    //// ----------------------------------------------------------------
    ////  Serializing
    //// ----------------------------------------------------------------
    //public PlatformData SerializeAsData() {
    //	PlatformData data = new PlatformData();
    //	data.myRect = MyRect;
    //	data.canEatGems = CanEatGems;
    //	return data;
    //}
}
