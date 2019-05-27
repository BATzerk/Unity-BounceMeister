using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropTravelMind : MonoBehaviour {
    // Properties
    [SerializeField] public Vector2 PosA = new Vector2(0, 0);
    [SerializeField] public Vector2 PosB = new Vector2(5, 0);
    [SerializeField] public float Speed = 1;
    [SerializeField] public float LocOffset = 0;
    private float oscLoc;
    // References
    private ITravelable myProp; // assigned in Initialize.
    
    public TravelMindData ToData() {
        return new TravelMindData(this);
    }
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(ITravelable myProp, TravelMindData data) {
        this.myProp = myProp;
        PosA = data.posA;
        PosB = data.posB;
        Speed = data.speed;
        LocOffset = data.locOffset;
        
        oscLoc = LocOffset; // start with my desired offset!
        
        ApplyPos();
        
        // Move OnOffer component just under my Script, for easiness.
        #if UNITY_EDITOR
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
        #endif
    }
    
    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        // Safety check for edit-mode.
        if (myProp == null) { myProp = GetComponent<ITravelable>(); }
        
        //Vector2 prevPos = Pos;TODO: This, vel-setting.

        ApplyPos();

        //vel = Pos - prevPos;
    }
    private void ApplyPos() {
        oscLoc += Time.deltaTime * Speed;
        float loc = MathUtils.Sin01(oscLoc);
        myProp.SetPos(Vector2.Lerp(PosA,PosB, loc));
    }
    
    
    
    // ----------------------------------------------------------------
    //  Editing
    // ----------------------------------------------------------------
    public void Debug_SetPosA() {
        PosA = MathUtils.Round(myProp.GetPos());
    }
    public void Debug_SetPosB() {
        PosB = MathUtils.Round(myProp.GetPos());
    }
    
    
}
