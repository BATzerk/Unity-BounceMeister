﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Prop {
    // Constants
    private const float BeamWidth = 0.3f; // in Unity units.
    // Components
    [SerializeField] private BoxCollider2D bc_beam=null;
    [SerializeField] private LayerMask lm_beamStops=new LayerMask();
    [SerializeField] private SpriteRenderer sr_beam=null;
    [SerializeField] private SpriteRenderer sr_beamGlow=null;
    [SerializeField] private Transform tf_sourceBox=null;
    private LaserOnOffer onOffer; // added in Initialize.
    // Properties
    public bool IsOn { get; private set; }
    // References
    private RaycastHit2D hit; // cached for optimization.

    // Getters
    private float angle { get { return transform.localEulerAngles.z * Mathf.Deg2Rad; } }
    private PlatformCharacter Character(Collider2D col) {
        return col.gameObject.GetComponent<PlatformCharacter>();
    }
    private float RaycastBeamLength() {
        Vector2 dir = MathUtils.GetVectorFromAngleDeg(-rotation);
        hit = Physics2D.Raycast(PosGlobal, dir, 999, lm_beamStops);//beamOriginOffset
        if (hit.collider != null) {
            return hit.distance;
        }
        return 999; // Beam goes on fo'eva.
    }

    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, LaserData data) {
        base.BaseInitialize(_myRoom, data);
        
        onOffer = this.gameObject.AddComponent<LaserOnOffer>();
        onOffer.Initialize(this);
        
        //tf_sourceBox.size = data.myRect.size;
        //sr_body.transform.localPosition = data.myRect.position;
        //sourceBoxRotation = data.rotation;
    }
    
    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void Update() {
        UpdateGlowAlpha();
    }
    private void UpdateGlowAlpha() {
        float alpha = MathUtils.SinRange(0.05f, 0.12f, Time.time*60);
        GameUtils.SetSpriteAlpha(sr_beamGlow, alpha);
    }
    
    private void FixedUpdate() {
        UpdateBeamLength();
    }
    private void UpdateBeamLength() {
        float beamLength = RaycastBeamLength();
        Rect beamRect = new Rect(-BeamWidth*0.5f,0, BeamWidth,beamLength);
        GameUtils.SizeSpriteRenderer(sr_beam, beamRect.size);
        GameUtils.SizeSpriteRenderer(sr_beamGlow, beamRect.size + new Vector2(0.5f, 0));
        sr_beam.transform.localPosition = beamRect.center;
        sr_beamGlow.transform.localPosition = beamRect.center;// + new Vector2(-1.5f, 0);
        //bc_beam.size = beamRect.size;
        //bc_beam.offset = beamRect.center;
    }
    
    public void UpdateAlmostOn(float timeUntilOn) {
        float alpha = MathUtils.SinRange(0.03f, 0.10f, timeUntilOn*40);
        GameUtils.SetSpriteAlpha(sr_beam, alpha);
    }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void SetIsOn(bool _isOn) {
        IsOn = _isOn;
        bc_beam.enabled = IsOn;
        GameUtils.SetSpriteAlpha(sr_beam, IsOn ? 0.7f : 0);
        sr_beamGlow.enabled = IsOn;
    }
    


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        return new LaserData {
            pos = pos,
            rotation = rotation,
        };
    }


}
