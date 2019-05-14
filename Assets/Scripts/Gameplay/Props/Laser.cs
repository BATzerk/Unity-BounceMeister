using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: This whole chestnut.
public class Laser : Prop {
    // Constants
    private const float BeamWidth = 0.45f; // in Unity units.
    // Components
    [SerializeField] private LayerMask lm_beamStops=new LayerMask();
    [SerializeField] private SpriteRenderer sr_beam=null;
    [SerializeField] private Transform tf_sourceBox=null;
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
        
        //tf_sourceBox.size = data.myRect.size;
        //sr_body.transform.localPosition = data.myRect.position;
        //sourceBoxRotation = data.rotation;
    }
    
    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        UpdateBeamLength();
    }
    private void UpdateBeamLength() {
        float beamLength = RaycastBeamLength();
        Rect beamRect = new Rect(-BeamWidth*0.5f,0, BeamWidth,beamLength);
        GameUtils.SizeSpriteRenderer(sr_beam, beamRect.size);
        sr_beam.transform.localPosition = beamRect.center;
        //bc_beam.size = beamRect.size;
        //bc_beam.offset = beamRect.center;
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

