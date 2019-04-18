using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNeighborData {
    // References
    public RoomData RoomTo { get; private set; }
    public RoomOpening OpeningFrom { get; private set; }
    // Getters
    public bool IsRoomTo { get { return RoomTo != null; } } // Some NeighborDatas DON'T have neighbors, while we're moving around/editing rooms.

    // Initialize
    public RoomNeighborData(RoomData roomTo, RoomOpening openingFrom) {
        this.RoomTo = roomTo;
        this.OpeningFrom = openingFrom;
    }
}
