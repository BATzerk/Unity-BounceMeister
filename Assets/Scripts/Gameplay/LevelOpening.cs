using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LevelOpening {
    // Properties
    public float length;// { get; private set; }
    public int side;
    public Vector2 posCenter;
    public Vector2 posStart,posEnd;


    // Initialize
    public LevelOpening(int side, Vector2 posStart,Vector2 posEnd) {
        this.side = side;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.posCenter = Vector2.Lerp(posStart,posEnd, 0.5f);
        this.length = Vector2.Distance(posStart,posEnd);
    }
}
