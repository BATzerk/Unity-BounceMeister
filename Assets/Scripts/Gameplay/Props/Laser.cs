using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Prop, IOnOffable {
    // Constants
    private const float BeamWidth = 0.3f; // in Unity units.
    // Components
    [SerializeField] private BoxCollider2D bc_beam=null;
    [SerializeField] private LayerMask lm_beamStops=new LayerMask();
    [SerializeField] private SpriteRenderer sr_beam=null;
    [SerializeField] private SpriteRenderer sr_beamGlow=null;
    //[SerializeField] private Transform tf_sourceBox=null;
    // References
    private RaycastHit2D hit; // cached for optimization.

    // Getters
    private float angle { get { return transform.localEulerAngles.z * Mathf.Deg2Rad; } }
    private PlatformCharacter Character(Collider2D col) {
        return col.gameObject.GetComponent<PlatformCharacter>();
    }
    private float RaycastBeamLength() {
        bool pqueriesHitTriggers = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = false;
        
        Vector2 dir = MathUtils.GetVectorFromDeg(rotation);
        Vector2 sourcePos = PosGlobal + dir*0.52f; // HARDCODED 0.52f. just beyond my tip, so I can be slightly in some Ground and not detect it.
        hit = Physics2D.Raycast(sourcePos, dir, 999, lm_beamStops);//beamOriginOffset
        
        Physics2D.queriesHitTriggers = pqueriesHitTriggers; // Put the toilet seat back down.
        if (hit.collider != null) {
            return hit.distance + 0.52f + 0.3f; // HACK go a liitle farther in case Player's moving away from us. // HARDCODED 0.52f
        }
        return 999; // Beam goes on fo'eva.
    }
    
    // OnOffer Stuff
    private bool isOn;
    private PropOnOffer onOffer; // added in Initialize.
    public OnOfferData onOfferData { get { return new OnOfferData(onOffer); } }
    public bool IsOn() { return isOn; }
    public bool HasOnOffer() { return onOffer != null; }
    public void AddOnOffer(OnOfferData data) {
        if (onOffer != null) { return; } // Safety check.
        onOffer = gameObject.AddComponent<PropOnOffer>();
        onOffer.Initialize(this, data);
    }
    public void RemoveOnOffer() {
        if (onOffer == null) { return; } // Safety check.
        Destroy(onOffer);
        onOffer = null;
        SetIsOn(true);
    }
    public void UpdateAlmostOn(float timeUntilOn) {
        float alpha = MathUtils.SinRange(0.08f, 0.14f, timeUntilOn*40);
        GameUtils.SetSpriteAlpha(sr_beam, alpha);
    }
    public void SetIsOn(bool _isOn) {
        isOn = _isOn;
        bc_beam.enabled = isOn;
        LeanTween.cancel(sr_beam.gameObject);
        //GameUtils.SetSpriteAlpha(sr_beam, IsOn ? 0.7f : 0);
        if (isOn) {
            GameUtils.SetSpriteAlpha(sr_beam, 0.7f);
        }
        else {
            LeanTween.alpha(sr_beam.gameObject, 0.02f, 0.1f).setEaseOutQuad(); // fade out quickly.
        }
        sr_beamGlow.enabled = isOn;
    }
    override protected void OnCreatedInEditor() {
        base.OnCreatedInEditor();
        if (onOffer == null) { onOffer = GetComponent<PropOnOffer>(); } // Safety check for duplicating objects.
    }
    

    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, LaserData data) {
        base.BaseInitialize(_myRoom, data);
        
        if (data.onOfferData.durOff > 0) { AddOnOffer(data.onOfferData); }
        
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
    

    // ----------------------------------------------------------------
    //  Editing
    // ----------------------------------------------------------------
    public override void FlipVert() {
        pos = new Vector2(pos.x, -pos.y);
        rotation += 180;
    }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        //if (onOffer == null) { onOffer = GetComponent<PropOnOffer>(); } // Safety check for duplicating objects.
        return new LaserData {
            pos = pos,
            rotation = rotation,
            onOfferData = new OnOfferData(onOffer),
        };
    }


}

