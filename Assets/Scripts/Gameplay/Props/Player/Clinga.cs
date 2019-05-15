using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clinga : Player {
    [System.Flags]
    private enum Syde { // Like Side, but for Bitmask usage!
        none = 0,
        T = 1,
        R = 2,
        B = 4,
        L = 8,
    }
    private Syde GetSyde(int side) {
        switch (side) {
            case Sides.T: return Syde.T;
            case Sides.R: return Syde.R;
            case Sides.B: return Syde.B;
            case Sides.L: return Syde.L;
            default: return Syde.none;
        }
    }
    
    // Overrides
    override public PlayerTypes PlayerType() { return PlayerTypes.Clinga; }
    override protected bool MayWallSlide() { return false; } // I don't slide. I /cling/.
    override protected Vector2 Gravity { get {
            return IsClinging ? Vector2.zero : base.Gravity;
        }
    }
    //protected override float HorzMoveInputVelXDelta() {
    //    return IsClinging ? 0 : base.HorzMoveInputVelXDelta(); // Clinging? Do NOT accept our normal horz input.
    //}
    private const float ClingMoveInputScale = 0.1f;
    // Properties
    private Syde ClingSydes = Syde.none; // bitmasks!
    // References
    private ClingaBody myClingaBody;
    
    // Getters (Private)
    private bool IsClinging { get { return ClingSydes != Syde.none; } }
    public  bool IsClingSyde(int side) { return IsClingSyde(GetSyde(side)); }
    private bool IsClingSyde(Syde syde) { return (ClingSydes & syde) != Syde.none; }
    private bool IsClingHorz { get { return IsClingSyde(Syde.B) || IsClingSyde(Syde.T); } }
    private bool IsClingVert { get { return IsClingSyde(Syde.L) || IsClingSyde(Syde.R); } }
    private bool MayCling(Collider2D coll) {
        return true; // Currently, all Colliders are ok to cling to!
    }


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myClingaBody = myBody as ClingaBody;
        base.Start();
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    override protected void AcceptJoystickMoveInput() {
        // Clinging? Register 360 input!
        if (IsClinging) {
            ChangeVel(ClingMoveInputScale * LeftStick);
        }
        // NOT clinging. Do normal Player stuff.
        else {
            base.AcceptJoystickMoveInput();
        }
    }
    override protected void ApplyFriction() {
        // Clinging?
        if (IsClinging) {
            vel *= 0.7f; // flat clinging friction.
        }
        else {
            base.ApplyFriction();
        }
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
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void AddClingSyde(int side) { AddClingSyde(GetSyde(side)); }
    private void RemoveClingSyde(int side) { RemoveClingSyde(GetSyde(side)); }
    
    private void AddClingSyde(Syde syde) {
        ClingSydes |= syde;
        SetVel(Vector2.zero);
        myClingaBody.OnChangeClingSydes();
    }
    private void RemoveClingSyde(Syde syde) {
        ClingSydes &= ~syde;
        myClingaBody.OnChangeClingSydes();
    }


    // ----------------------------------------------------------------
    //  Physics Events
    // ----------------------------------------------------------------
    public override void OnWhiskersTouchCollider(int side, Collider2D col) {
        base.OnWhiskersTouchCollider(side, col);
        // NON-feet side touch collider...!
        if (side != Sides.B) {
            if (!IsClingSyde(side)) { // NOT already clinging here?...
                AddClingSyde(side);
            }
        }
    }
    public override void OnWhiskersLeaveCollider(int side, Collider2D col) {
        base.OnWhiskersLeaveCollider(side, col);
        // We WERE clinging here, but there's nothing left to cling to??
        if (IsClingSyde(side) && !myWhiskers.OnSurface(side)) {
            RemoveClingSyde(side);
        }
    }

}


    //protected override void AcceptButtonInput() {
        //base.AcceptButtonInput();
        //if (Input.GetKeyDown(KeyCode.LeftArrow)) { AddClingSyde(Syde.L); }
        //if (Input.GetKeyDown(KeyCode.RightArrow)) { AddClingSyde(Syde.R); }
        //if (Input.GetKeyDown(KeyCode.DownArrow)) { AddClingSyde(Syde.B); }
        //if (Input.GetKeyDown(KeyCode.UpArrow)) { AddClingSyde(Syde.T); }
        //if (Input.GetKeyDown(KeyCode.A)) { RemoveClingSyde(Syde.L); }
        //if (Input.GetKeyDown(KeyCode.D)) { RemoveClingSyde(Syde.R); }
        //if (Input.GetKeyDown(KeyCode.S)) { RemoveClingSyde(Syde.B); }
        //if (Input.GetKeyDown(KeyCode.W)) { RemoveClingSyde(Syde.T); }
    //}