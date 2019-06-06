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
    
    // Getters
    private Vector2 Pos { get { return myProp.GetPos(); } }
    
    
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
        
        // Move this component just under my Script, for easiness.
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
        
        Vector2 prevPos = Pos;

        ApplyPos();

        myProp.SetVel(Pos - prevPos);
    }
    private void ApplyPos() {
        oscLoc += GameTimeController.RoomDeltaTime * Speed;
        float loc = MathUtils.Sin01(oscLoc);
        myProp.SetPos(Vector2.Lerp(PosA,PosB, loc));
    }
    
    
    
    // ----------------------------------------------------------------
    //  Editing
    // ----------------------------------------------------------------
    public void Debug_SetPosA() {
        PosA = MathUtils.HalfRound(myProp.GetPos());
    }
    public void Debug_SetPosB() {
        PosB = MathUtils.HalfRound(myProp.GetPos());
    }
    public void FlipHorz() {
        PosA = new Vector2(-PosA.x, PosA.y);
        PosB = new Vector2(-PosB.x, PosB.y);
    }
    public void FlipVert() {
        PosA = new Vector2(PosA.x, -PosA.y);
        PosB = new Vector2(PosB.x, -PosB.y);
    }
    public void Move(Vector2 delta) {
        PosA += delta;
        PosB += delta;
    }
    
    
}
