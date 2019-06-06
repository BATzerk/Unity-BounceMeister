using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniMapNamespace {
public class MiniMap : MonoBehaviour {
    // Components
    [SerializeField] private GameObject go_snackCount=null;
    [SerializeField] private TextMeshProUGUI t_snackCount=null;
    //[SerializeField] private RectTransform myRectTransform=null;
    [SerializeField] private RectTransform rt_tiles=null;
    private Dictionary<string,MiniMapRoomTile> tiles; // roomKey is key.
    // Properties
    private const float mapScale = 0.2f;//0.5f; // SMALLER is smaller tiles. // NOTE: Unity units to Screen units automatically makes rooms way smaller (1 Unity unit is 1 pixel).
    private Vector2 mapPosTarget;
    // References
    private Room currRoom;
    
    // Getters (Private)
    private PlayerTypes currPlayerType { get { return currRoom.Player.PlayerType(); } }
    private RoomData currRoomData { get { return currRoom.MyRoomData; } }
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
        GameManagers.Instance.EventManager.SnackCountGameChangedEvent += OnSnackCountGameChanged;
        GameManagers.Instance.EventManager.SwapPlayerTypeEvent += OnSwapPlayerType;
        GameManagers.Instance.EventManager.StartRoomEvent += OnStartRoom;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.SnackCountGameChangedEvent -= OnSnackCountGameChanged;
        GameManagers.Instance.EventManager.SwapPlayerTypeEvent -= OnSwapPlayerType;
        GameManagers.Instance.EventManager.StartRoomEvent -= OnStartRoom;
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
        
        // TO DO: #maybe Just do clusters?
        tiles = new Dictionary<string, MiniMapRoomTile>();
        WorldData wd = currRoom.MyWorldData;
        foreach (RoomData rd in wd.roomDatas.Values) {
            MiniMapRoomTile tile = Instantiate(ResourcesHandler.Instance.MiniMapRoomTile).GetComponent<MiniMapRoomTile>();
            tile.Initialize(rt_tiles, rd);
            tiles.Add(rd.RoomKey, tile);
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
            tile.UpdateVisuals(currRoomData, currPlayerType);
        }
    }
    private void UpdateSnackCountText() {
        if (currRoomData.MyCluster == null || currRoomData.MyCluster.SnackCount.Total_All==0) {
            go_snackCount.SetActive(false); // No cluster ('cause dev), OR no Snacks? Hide snackCount.
        }
        else {
            go_snackCount.SetActive(true);
            SnackCount sc = currRoomData.MyCluster.SnackCount;
            t_snackCount.text = sc.Eaten(currPlayerType) + " / " + sc.Total(currPlayerType);
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
        UpdateAllTilesVisuals();
        UpdateSnackCountText();
    }
    private void OnSwapPlayerType() {
        UpdateAllTilesVisuals();
        UpdateSnackCountText();
    }
    private void OnSnackCountGameChanged() {
        tiles[currRoom.RoomKey].UpdateVisuals(currRoomData, currPlayerType); // Update the visuals of the current RoomTile.
        UpdateSnackCountText();
    }
    
    
    
}
}
