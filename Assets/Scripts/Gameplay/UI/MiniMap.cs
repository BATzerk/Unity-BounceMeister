using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour {
    // Components
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private RectTransform rt_tiles;
    private Dictionary<string,MiniMapRoomTile> tiles; // roomKey is key.
    // Properties
    private const float mapScale = 0.5f;//0.8f; // NOTE: Unity units to Screen units automatically makes rooms way smaller (1 Unity unit is 1 pixel).
    private Vector2 mapPosTarget;
    // References
    private Room currRoom;
    
    // Getters (Private)
    private RoomData currRoomData { get { return currRoom.RoomDataRef; } }
    // Setters
    private Vector2 MapPos {
        get { return rt_tiles.anchoredPosition; }
        set { rt_tiles.anchoredPosition = value; }
    }
    

    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Awake() {
        // Scale da map!
        rt_tiles.localScale = Vector3.one * mapScale;
        
        // Add event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent += OnStartRoom;
        GameManagers.Instance.EventManager.SnacksCollectedChangedEvent += OnSnacksCollectedChanged;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent -= OnStartRoom;
        GameManagers.Instance.EventManager.SnacksCollectedChangedEvent -= OnSnacksCollectedChanged;
    }
    
    private void DestroyAllTiles() {
        if (tiles != null) {
            foreach (MiniMapRoomTile tile in tiles.Values) {
                Destroy(tile.gameObject);
            }
            tiles = null;
        }
    }
    private void MakeAllTiles() {
        DestroyAllTiles();
        
        // TODO: Just do clusters?
        tiles = new Dictionary<string, MiniMapRoomTile>();
        WorldData wd = currRoom.WorldDataRef;
        foreach (RoomData rd in wd.roomDatas.Values) {
            MiniMapRoomTile tile = Instantiate(ResourcesHandler.Instance.MiniMapRoomTile).GetComponent<MiniMapRoomTile>();
            tile.Initialize(rt_tiles, rd);
            tiles.Add(rd.roomKey, tile);
        }
    }
    
    
    
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        // Ease pos to target!
        if (MapPos != mapPosTarget) {
            MapPos += (mapPosTarget-MapPos) * 0.1f;
            if (Vector2.Distance(MapPos, mapPosTarget) < 0.1f) { // Almost there? Get it, Rainn!
                MapPos = mapPosTarget;
            }
        }
    }
    
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateAllTilesVisuals() {
        foreach (MiniMapRoomTile tile in tiles.Values) {
            tile.UpdateVisuals(currRoomData);
        }
    }
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnStartRoom(Room room) {
        // Update map position!
        mapPosTarget = room.PosGlobal*mapScale * -1;
        
        // We changed worlds??
        int prevWorldIndex = currRoom==null ? -1 : currRoom.WorldIndex;
        this.currRoom = room;
        if (prevWorldIndex != room.WorldIndex) {
            MakeAllTiles();
            MapPos = mapPosTarget; // start at right room.
        }
        
        // Update tile visuals!
        UpdateAllTilesVisuals();// TODO: This more efficiently?
    }
    private void OnSnacksCollectedChanged(int worldIndex) {
        tiles[currRoom.RoomKey].UpdateVisuals(currRoomData); // Update the visuals of the current RoomTile.
    }
    
    
    
}
