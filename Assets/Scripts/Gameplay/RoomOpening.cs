using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOpening {
    // Properties
    public float length { get; private set; }
    public int side { get; private set; }
    public Vector2 posCenter { get; private set; }
    public Vector2 posStart { get; private set; }
    public Vector2 posEnd { get; private set; }
    // References
    public RoomData RoomFrom { get; private set; }
    public RoomData RoomTo { get; private set; }
    //public RoomOpening OpeningTo { get; private set; }
    
    // Getters
    public bool IsRoomTo { get { return RoomTo != null; } } // Some Openings DON'T have neighbors, while we're moving around/editing rooms.
    /// Returns a Rect that's a thicc version of me as an opening. So we can check for overlaps with other RoomOpenings.
    public Rect GetCollRectGlobal(Vector2 roomPosGlobal) {
        const float thickness = 2; // how many Unity units we bloat the Rect. Higher means we can have a bigger gap between rooms.
        bool isHorz = side==Sides.B || side==Sides.T;
        Rect rect = new Rect {
            size = isHorz ? new Vector2(length, thickness) : new Vector2(thickness, length),
            center = posCenter + roomPosGlobal
        };
        return rect;
    }
    
    // Setters
    public void SetRoomTo(RoomData _room) { RoomTo = _room; }


    // Initialize
    public RoomOpening(RoomData RoomFrom, int side, Vector2 posStart,Vector2 posEnd) {
        this.RoomFrom = RoomFrom;
        this.side = side;
        this.posStart = posStart;
        this.posEnd = posEnd;
        this.posCenter = Vector2.Lerp(posStart,posEnd, 0.5f);
        this.length = Vector2.Distance(posStart,posEnd);
    }
}
