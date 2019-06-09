using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(GameController))]
public class EditModePropHelper : MonoBehaviour {
#if UNITY_EDITOR
    // Properties
    private bool doAutoRotateSpikes; // updated every frame, based on keyboard input.
    private Vector3 propSelPos; // so we know when it changes!
    // References
    private GameController gameController; // set in Awake.
    private List<GroundSideRect> groundSides;
    private Prop propSel;
    
    // Getters (Private)
    private Room currRoom { get { return gameController.CurrRoom; } }
    private GroundSideRect GetGroundSideRectAt(Vector2 pos) {
        for (int i=0; i<groundSides.Count; i++) {
            if (groundSides[i].rect.Contains(pos)) {
                return groundSides[i];
            }
        }
        return GroundSideRect.undefined;
    }
    

    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake() {
        gameController = GetComponent<GameController>();
        if (gameController == null) { // Safety check.
            Debug.LogError("Oops! We're supposed to attach EditModePropHelper to the GameController GameObject!");
            this.enabled = false;
            return;
        }
    }


    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        doAutoRotateSpikes = !InputController.IsEditorKey_Control;
        
        UpdatePropSelRef();
        CheckPropSelPosChanged();
        
        //// Control + G = Add a Ground
        //if (InputController.IsEditorKey_Control && 
    }
    private void UpdatePropSelRef() {
        Prop prop = null;
        if (Selection.activeGameObject != null) {
            prop = Selection.activeGameObject.GetComponent<Prop>();
        }
        // It's changed??
        if (propSel != prop) {
            if (prop == null) { NullifyPropSel(); }
            else { SetPropSel(prop); }
        }
    }
    private void CheckPropSelPosChanged() {
        if (propSel!=null && propSel.transform.localPosition!=propSelPos) {
            OnPropSelPosChanged();
        }
    }

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnPropSelPosChanged() {
        // Yes auto-rotate Spikes, and only ONE Prop selected?
        if (doAutoRotateSpikes && Selection.gameObjects.Length==1) {
            AutoRotateSpikes(propSel as Spikes);
        }
        // Loop through ALL Props selected...
        foreach (GameObject go in Selection.gameObjects) {
            Prop prop = go.GetComponent<Prop>();
            if (prop != null) {
                // TravelMind?
                if (prop.HasTravelMind()) {
                    prop.Debug_ShiftPosesFromEditorMovement();
                }
            }
        }
        propSelPos = propSel.transform.localPosition;
    }
    

    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void NullifyPropSel() {
        propSel = null;
    }
    private void SetPropSel(Prop _prop) {
        propSel = _prop;
        propSelPos = propSel.transform.localPosition;
        if (propSel is Spikes) {
            RefreshGroundSides();
        }
    }
    private void RefreshGroundSides() {
        groundSides = new List<GroundSideRect>();
        Ground[] grounds = currRoom.GetComponentsInChildren<Ground>();
        foreach (Ground g in grounds) {
            for (int side=0; side<4; side++) {
                groundSides.Add(new GroundSideRect(side, g.GetMyRectBL()));
            }
        }
    }
    private void AutoRotateSpikes(Spikes spikes) {
        if (spikes == null) { return; } // Not spikes? No gazpacho.
        // What rect are we in??
        GroundSideRect gsrIn = GetGroundSideRectAt(spikes.PosLocal);
        if (!GroundSideRect.IsUndefined(gsrIn)) { // We're touching one!
            float rotation = -gsrIn.side * 90;
            spikes.Debug_SetRotation(rotation);
        }
    }
    
    
    //private void OnDrawGizmos() {
    //    if (currRoom == null || groundSides==null) { return; } // Safety check.
    //    Gizmos.color = Color.blue;
    //    Vector2 po = currRoom.PosGlobal;
    //    foreach (GroundSideRect gsr in groundSides) {
    //        Gizmos.DrawCube(po+gsr.rect.center, gsr.rect.size);
    //    }
    //}

    
    
    
    // ================================================================
    //  GroundSide
    // ================================================================
    private struct GroundSideRect {
        static public readonly GroundSideRect undefined = new GroundSideRect(-1, new Rect());
        public Rect rect; // MY rect, which is just beyond this side of the Ground
        public int side; // e.g. if this is Top side, I'm above my ground.
        public GroundSideRect(int side, Rect groundRectBL) {
            this.side = side;
            this.rect = GetRectFromGroundRect(side, groundRectBL);
        }
        private static Rect GetRectFromGroundRect(int side, Rect gr) {
            const float thk = 1; // thickness!
            switch (side) {
                case Sides.B: return new Rect(gr.xMin,gr.yMin-thk, gr.width,thk*2);
                case Sides.T: return new Rect(gr.xMin,gr.yMax-thk, gr.width,thk*2);
                case Sides.L: return new Rect(gr.xMin-thk,gr.yMin, thk*2,gr.height);
                case Sides.R: return new Rect(gr.xMax-thk,gr.yMin, thk*2,gr.height);
                default: return Rect.zero; // Hmm.
            }
        }

        public static bool IsUndefined(GroundSideRect gsr) { return gsr.side < 0; }
        public override bool Equals(object o) { return base.Equals(o); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).
        public override int GetHashCode() { return base.GetHashCode(); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).
    }


#endif
}

