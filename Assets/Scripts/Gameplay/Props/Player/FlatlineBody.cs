using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatlineBody : PlayerBody {
    // Enums
    private enum EyeTypes { Undefined, Normal, Squint }
    // Constants
    [SerializeField] private Color c_hoverlight=Color.white;
    [SerializeField] private Color c_hoverlightEnding=Color.white;
    [SerializeField] private Color c_hoverlightEmpty=Color.white;
    // Components
    [SerializeField] private GameObject go_eyesNormal=null; // wide open. I can see the world.
    [SerializeField] private GameObject go_eyesSquint=null; // squinting in earnest consternation.
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
    //  Doers
    // ----------------------------------------------------------------
    private void SetEyes(EyeTypes eyeType) {
        go_eyesNormal.SetActive(false);
        go_eyesSquint.SetActive(false);
        switch (eyeType) {
            case EyeTypes.Normal: go_eyesNormal.SetActive(true); break;
            case EyeTypes.Squint: go_eyesSquint.SetActive(true); break;
            default: Debug.LogWarning("FlatlineBody EyeType not recognized: " + eyeType); break;
        }
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnStartHover() {
        sr_highlight.enabled = true;
        SetEyes(EyeTypes.Squint);
    }
    public void OnStopHover() {
        // Depleted?
        if (myFlatline.IsHoverEmpty) {
            GameUtils.SetSpriteColor(sr_highlight, c_hoverlightEmpty, 0.7f);
        }
        // NOT depleted? Open my eyes!
        else {
            SetEyes(EyeTypes.Normal);
            sr_highlight.enabled = false;
        }
    }
    public void OnRechargeHover() {
        if (!myFlatline.IsHovering) { // If I'm no longer hovering...
            sr_highlight.enabled = false;
            SetEyes(EyeTypes.Normal);
        }
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        UpdateHoverHighlight();
    }
    private void UpdateHoverHighlight() {
        if (myFlatline.IsHovering) {
            float alpha;
            float hovTimeLeft = myFlatline.HoverTimeLeft;
            if (hovTimeLeft > 0.45f) { // Oscillate normally.
                sr_highlight.color = c_hoverlight;
                alpha = MathUtils.SinRange(0.2f,0.6f, hovTimeLeft*13f);
            }
            else { // Almost out? Oscillate FAST!
                sr_highlight.color = c_hoverlightEnding;
                alpha = MathUtils.SinRange(0.2f,0.7f, hovTimeLeft*40f);
            }
            GameUtils.SetSpriteAlpha(sr_highlight, alpha);
        }
    }
}
