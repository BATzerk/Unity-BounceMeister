using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatlineBody : PlayerBody {
    // Constants
    [SerializeField] private Color c_hoverlight=Color.white;
    [SerializeField] private Color c_hoverlightEnding=Color.white;
    [SerializeField] private Color c_hoverlightEmpty=Color.white;
    // Components
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
    //protected override void SetVisualScale(Vector2 _scale) {
    //    base.SetVisualScale(_scale);
    //    GameUtils.SizeSpriteRenderer(sr_highlight, _scale*myBasePlayer.Size);
    //}


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnStartHover() {
        sr_highlight.enabled = true;
        UpdateHoverHighlight();
        eyes.Set(EyeTypes.Squint);
    }
    public void OnStopHover() {
        // Depleted?
        if (myFlatline.IsHoverEmpty) {
            GameUtils.SetSpriteColor(sr_highlight, c_hoverlightEmpty, 0.7f);
        }
        // NOT depleted?
        else {
            eyes.Set(EyeTypes.Normal);
            // We still can't control, though?? Keep some highlight.
            if (myFlatline.HasHoveredWithoutTouchCollider) {
                sr_highlight.enabled = true;
                GameUtils.SetSpriteAlpha(sr_highlight, 0.44f);
            }
            // We CAN control again!
            else {
                sr_highlight.enabled = false;
            }
        }
    }
    public void OnRechargeHover() {
        if (!myFlatline.IsHovering) { // If I'm no longer hovering...
            sr_highlight.enabled = false;
            eyes.Set(EyeTypes.Normal);
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
