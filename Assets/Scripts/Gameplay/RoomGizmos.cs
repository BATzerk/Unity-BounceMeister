using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGizmos : MonoBehaviour {
    // References
    private Room MyRoom; // set in Awake.
    private List<RoomOpening> NeighborOpenings; // openings that lead TO me!
    
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _MyRoom) {
        // NOT in edit mode?? Destroy me.
        if (!Application.isEditor) {
            Destroy(this);
            return;
        }
        
        this.MyRoom = _MyRoom;
        
        // Make NeighborOpenings!
        NeighborOpenings = new List<RoomOpening>();
        // For each of my neighboring Rooms...
        foreach (RoomData neighborRD in MyRoom.MyRoomData.NeighborRooms) {
            // For each of this OTHER Room's openings...
            foreach (RoomOpening otherOpening in neighborRD.Openings) {
                // If I'M the ROOM it connects to...!
                if (otherOpening.RoomTo == MyRoom.MyRoomData) {
                    NeighborOpenings.Add(otherOpening);
                }
            }
        }
    }


    // ----------------------------------------------------------------
    //  OnDrawGizmos
    // ----------------------------------------------------------------
    private void OnDrawGizmos() {
        if (NeighborOpenings == null) { return; }
        
        Gizmos.color = new Color(0.7f, 0.95f, 0f);
        foreach (RoomOpening ro in NeighborOpenings) {
            Vector2 offset = ro.RoomFrom.posGlobal;
            offset -= MathUtils.GetDir(ro.side) * 0.25f; // offset the Gizmos line TOWARDS other Room so we can see it better.
            Gizmos.DrawLine(offset+ro.posStart, offset+ro.posEnd);
        }
    }
}
