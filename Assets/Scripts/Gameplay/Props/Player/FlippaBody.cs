using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippaBody : PlayerBody {
    // Constants
    [SerializeField] private Color c_bodyFlipSpent=Color.white;
    // Properties
    private int flipDir; // matches what's in my Flippa.
    
    
    override protected void ApplyVisualScale() {
        this.transform.localScale = new Vector3(visualScale.x*myBasePlayer.DirFacing, visualScale.y*flipDir, 1);
    }
    
    
    public void OnSetFlipDir(int flipDir) {
        this.flipDir = flipDir;
        ApplyVisualScale();
        //transform.localScale = new Vector3(transform.localScale.x, flipDir, transform.localScale.z);    
    }
    public void OnFlipGravity() {
        SetBodyColor(c_bodyFlipSpent);
    }
    public void OnRechargeFlip() {
        SetBodyColor(c_bodyNeutral);
    }
    
}
