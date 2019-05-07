using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditorNamespace {
    /** How to use this class: It's added to MapEditor automatically! */
    public class MapEditorGizmos : MonoBehaviour {
        // Properties
        private List<Line> roomLinkLines;
        // References
        private MapEditor mapEditor; // set in Start.
        
        // Getters (Private)
        WorldData CurrWorldData { get { return mapEditor==null ? null : mapEditor.CurrWorldData; } }
        
        
        // ----------------------------------------------------------------
        //  Awake / Destroy
        // ----------------------------------------------------------------
        private void Awake() {
            mapEditor = GetComponent<MapEditor>();
            // Add event listeners.
            GameManagers.Instance.EventManager.MapEditorSetCurrWorldEvent += OnSetCurrWorld;
        }
        private void OnDestroy() {
            // Remove event listeners.
            GameManagers.Instance.EventManager.MapEditorSetCurrWorldEvent -= OnSetCurrWorld;
        }
        
        
        
        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        private void OnSetCurrWorld(int worldIndex) {
            // Remake roomLinkLines!
            roomLinkLines = new List<Line>();
            foreach (RoomData roomFrom in CurrWorldData.roomDatas.Values) {
                if (!roomFrom.IsInCluster) { continue; } // Not in a Cluster? Skip it.
                // For each RoomDoor...
                foreach (RoomDoorData doorFrom in roomFrom.roomDoorDatas) {
                    RoomData roomTo = CurrWorldData.GetRoomData(doorFrom.roomToKey);
                    if (roomTo == null) { continue; } // No room? No line. TO DO: Show like an X or something?
                    Vector2 doorToPos = roomTo.GetRoomDoorPos(doorFrom.roomToDoorID, false);
                    Line line = new Line {
                        start = roomFrom.PosGlobal + doorFrom.pos,
                        end = roomTo.PosGlobal + doorToPos
                    };
                    roomLinkLines.Add(line);
                }
            }
        }


        // ----------------------------------------------------------------
        //  Gizmos!
        // ----------------------------------------------------------------
        private void OnDrawGizmos () {
            // World Bounds
            if (CurrWorldData != null) {
                Gizmos.color = new Color (0.1f, 0.1f, 0.1f);
                Rect boundsRectAllRooms = CurrWorldData.BoundsRectAllRooms;
                Gizmos.DrawWireCube (new Vector3 (boundsRectAllRooms.center.x, boundsRectAllRooms.center.y, 0), new Vector3 (boundsRectAllRooms.size.x, boundsRectAllRooms.size.y, 10));
            }
            // RoomLinkLines!
            if (roomLinkLines != null) {
                Gizmos.color = Color.blue;
                foreach (Line line in roomLinkLines) {
                    Gizmos.DrawLine(line.start, line.end);
                }
            }
            // Clusters!
            if (CurrWorldData != null) {
                Gizmos.color = new Color (0.3f, 0.2f, 0.1f);
                foreach (RoomClusterData clustData in CurrWorldData.clusters) {
                    Rect r = clustData.BoundsGlobal;
                    Gizmos.DrawWireCube(new Vector3(r.center.x,r.center.y, 0), new Vector3(r.size.x,r.size.y, 10));
                }
            }
        }
        
    }
}