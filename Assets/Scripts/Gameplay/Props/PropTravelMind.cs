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
    private Vector2 myRoomPos; // global pos of my Room. For Gizmos.
    // References
    private ITravelable myTravelable; // assigned in Initialize.
    
    // Serializing
    public TravelMindData ToData() {
        return new TravelMindData(this);
    }
    
    // Getters
    private Vector2 Pos { get { return myTravelable.GetPos(); } }
    private Vector2 GetCurrPos() {
        float loc = MathUtils.Sin01(oscLoc);
        return Vector2.Lerp(PosA,PosB, loc);
    }
    
    public void Debug_ShiftPosesFromEditorMovement() {
        Vector2 delta = Pos - GetCurrPos(); // how far are we from where we're SUPPOSED to be?
        PosA = MathUtils.HalfRound(PosA + delta);
        PosB = MathUtils.HalfRound(PosB + delta);
        ApplyPos();
    }


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(TravelMindData data) {
        PosA = data.posA;
        PosB = data.posB;
        Speed = data.speed;
        LocOffset = data.locOffset;
        oscLoc = LocOffset; // start with my desired offset!
        
        // Set refs.
        myTravelable = GetComponent<ITravelable>();
        Prop myProp = GetComponent<Prop>();
        if (myProp != null) {
            myRoomPos = myProp.MyRoom.PosGlobal;
            oscLoc += myProp.MyRoom.RoomTime * Speed; // also start us at the right spot, based on the Room's age (in case we're made DURING being in the Room [i.e. in editor]).
        }
        
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
        Vector2 prevPos = Pos;

        StepPos();
        ApplyPos();

        myTravelable.SetVel(Pos - prevPos);
    }
    private void StepPos() {
        oscLoc += GameTimeController.RoomDeltaTime * Speed;
    }
    private void ApplyPos() {
        myTravelable.SetPos(GetCurrPos());
    }
    
    
    
    // ----------------------------------------------------------------
    //  Editing
    // ----------------------------------------------------------------
    public void Debug_SetPosA() {
        PosA = MathUtils.HalfRound(myTravelable.GetPos());
    }
    public void Debug_SetPosB() {
        PosB = MathUtils.HalfRound(myTravelable.GetPos());
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

    // ----------------------------------------------------------------
    //  Gizmos
    // ----------------------------------------------------------------
    private void OnDrawGizmos() {
        Gizmos.color = Color.gray;
        Gizmos.DrawLine(PosA+myRoomPos, PosB+myRoomPos);
    }

}
