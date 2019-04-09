using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LevelOpening {
    // Properties
    public float length;// { get; private set; }
    public int side;
    public Vector2 posCenter;
    public Vector2 posStart,posEnd;
    
    // Getters
    /// Returns a Rect that's a thicc version of me as an opening. So we can check for overlaps with other LevelOpenings.
    public Rect GetCollRectGlobal(Vector2 levelPosGlobal) {
        const float thickness = 2; // how many Unity units we bloat the Rect. Higher means we can have a bigger gap between levels.
        bool isHorz = side==Sides.B || side==Sides.T;
        Rect rect = new Rect {
            size = isHorz ? new Vector2(length, thickness) : new Vector2(thickness, length),
            center = posCenter + levelPosGlobal
        };
        return rect;
    }


    // Initialize
    public LevelOpening(int side, Vector2 posStart,Vector2 posEnd) {
        this.side = side;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.posCenter = Vector2.Lerp(posStart,posEnd, 0.5f);
        this.length = Vector2.Distance(posStart,posEnd);
    }
}
