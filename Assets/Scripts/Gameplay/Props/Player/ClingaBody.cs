using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClingaBody : PlayerBody {
    // Components
    [SerializeField] private SpriteRenderer[] sr_clingHands=null; // T, R, B, L.
    //[SerializeField] private SpriteRenderer sr_highlight=null;
    // References
    private Clinga myClinga;
    
    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Awake() {
        base.Awake();
        myClinga = myBasePlayer as Clinga;
        
        OnChangeClingSydes(); // refresh cling visuals.
    }
    //protected override void SetVisualScale(Vector2 _scale) {
    //    base.SetVisualScale(_scale);
    //    GameUtils.SizeSpriteRenderer(sr_highlight, _scale*myBasePlayer.Size);
    //}
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnChangeClingSydes() {
        for (int i=0; i<sr_clingHands.Length; i++) {
            sr_clingHands[i].enabled = myClinga.IsClingSyde(i);
        }
    }
}
