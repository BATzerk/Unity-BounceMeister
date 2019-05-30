﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditModePropHelper : MonoBehaviour {
#if UNITY_EDITOR
    // Properties
    private bool doAutoRotateSpikes = true;
    private Vector3 spikesSelPos; // when this changes, we update spikesSel rotation!
    // References
    private GameController gameController; // set in Awake.
    private List<GroundSideRect> groundSides;
    private Spikes spikesSel;
    
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
        // We DO wanna auto-rotate?? Do our updates!
        if (doAutoRotateSpikes) {
            UpdateSpikesSelRef();
            CheckSpikesSelPosChanged();
        }
        RegisterKeyInput();
    }
    private void RegisterKeyInput() {
        if (InputController.IsKeyDown_control) { ToggleDoAutoRotateSpikes(); }
    }
    private void UpdateSpikesSelRef() {
        Spikes _spikes = null;
        if (Selection.activeGameObject != null) {
            _spikes = Selection.activeGameObject.GetComponent<Spikes>();
        }
        // It's changed??
        if (spikesSel != _spikes) {
            if (_spikes == null) { NullifySpikesSel(); }
            else { SetSpikesSel(_spikes); }
        }
    }
    private void CheckSpikesSelPosChanged() {
        if (spikesSel!=null && spikesSel.transform.localPosition!=spikesSelPos) {
            OnSpikesSelPosChanged();
        }
    }

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSpikesSelPosChanged() {
        spikesSelPos = spikesSel.transform.localPosition;
        AutoRotateSpikesSel();
    }
    

    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void ToggleDoAutoRotateSpikes() {
        doAutoRotateSpikes = !doAutoRotateSpikes;
        Debug.Log("Auto-rotate Spikes " + (doAutoRotateSpikes?"ENABLED":"DISabled") + ".");
    }
    private void NullifySpikesSel() {
        spikesSel = null;
    }
    private void SetSpikesSel(Spikes _spikes) {
        spikesSel = _spikes;
        spikesSelPos = spikesSel.transform.localPosition; // also reset this, so we don't rotate until we move it.
        RefreshGroundSides();
    }
    private void RefreshGroundSides() {
        groundSides = new List<GroundSideRect>();
        Ground[] grounds = currRoom.GetComponentsInChildren<Ground>();
        foreach (Ground g in grounds) {
            for (int side=0; side<4; side++) {
                groundSides.Add(new GroundSideRect(side, g.MyRectBL()));
            }
        }
    }
    private void AutoRotateSpikesSel() {
        if (spikesSel == null) { return; } // Not selected? No gazpacho.
        // What rect are we in??
        GroundSideRect gsrIn = GetGroundSideRectAt(spikesSel.PosLocal);
        if (!GroundSideRect.IsUndefined(gsrIn)) { // We're touching one!
            float rotation = -gsrIn.side * 90;
            spikesSel.Debug_SetRotation(rotation);
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
