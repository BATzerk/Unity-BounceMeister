using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class GridSnapScale : BaseGridSnap {
    // Components
    private SpriteRenderer spriteRenderer;
    // Properties
    private bool doSnapSpriteSize; // true if I have a SpriteRenderer that's sliced or tiled! (In this scenario, our scale will probably stay at 1, and it's the SPRITE that's scaled.)

    // Getters
    private Vector3 scale {
        get {
            if (doSnapSpriteSize) { return spriteRenderer.size; }
            else { return this.transform.localScale; }
        }
        set {
            if (doSnapSpriteSize) { spriteRenderer.size = value; }
            else { this.transform.localScale = value; }
        }
    }


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    private void Start () {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        doSnapSpriteSize = spriteRenderer!=null && spriteRenderer.drawMode!=SpriteDrawMode.Simple;
    }


    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private Vector3 pscale;
    private void Update () {
        // If scale or pos has changed, snap me!
        if (scale != pscale) {
            SnapScale();
            pscale = scale;
        }
    }
    private void SnapScale() {
        float us = GameProperties.UnitSize;
        // Snap scale.
        scale = new Vector3(Mathf.Round(scale.x/us)*us, Mathf.Round(scale.y/us)*us, scale.z);
        scale = new Vector3(Mathf.Max(1, scale.x), Mathf.Max(1, scale.y)); // Don't let things get weird.
        
//      // Snap sprite scale!
//      if (doSnapSpriteSize) {
//          spriteRenderer.size = new Vector3(Mathf.Round(spriteRenderer.size.x/us)*us, Mathf.Round(spriteRenderer.size.y/us)*us, scale.z);
//          spriteRenderer.size = new Vector3(Mathf.Max(1, spriteRenderer.size.x), Mathf.Max(1, spriteRenderer.size.y)); // Don't let things get weird.
//      }
    }


}



