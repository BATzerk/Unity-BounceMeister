using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatlineBody : PlayerBody {
    // Components
    [SerializeField] private GameObject go_eyesNormal=null;
    [SerializeField] private GameObject go_eyesSuspended=null; // squinting!
    [SerializeField] private SpriteRenderer sr_highlight=null;
    // References
    private Flatline myFlatline;


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Start() {
        myFlatline = myBasePlayer as Flatline;
        base.Start();
    }
    protected override void SetVisualScale(Vector2 _scale) {
        base.SetVisualScale(_scale);
        GameUtils.SizeSpriteRenderer(sr_highlight, _scale*myBasePlayer.Size);
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnStartSuspension() {
        sr_highlight.enabled = true;
        go_eyesNormal.SetActive(false);
        go_eyesSuspended.SetActive(true);
    }
    public void OnStopSuspension() {
        sr_highlight.enabled = false;
        go_eyesNormal.SetActive(true);
        go_eyesSuspended.SetActive(false);
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        // Update highlight!
        if (myFlatline.IsSuspended) {
            float alpha = MathUtils.SinRange(0.2f,0.6f, Time.time*13f);
            GameUtils.SetSpriteAlpha(sr_highlight, alpha);
        }
    }
}
