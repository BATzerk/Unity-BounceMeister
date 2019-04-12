using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour {
    // Components
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private RectTransform rt_tiles;
    private List<MiniMapLevelTile> tiles;
    // Properties
    private const float mapScale = 0.5f;//0.8f; // NOTE: Unity units to Screen units automatically makes levels way smaller (1 Unity unit is 1 pixel).
    private Vector2 mapPosTarget;
    // References
    private Level myLevel;
    
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
        GameManagers.Instance.EventManager.StartLevelEvent += OnStartLevel;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.StartLevelEvent -= OnStartLevel;
    }
    
    private void DestroyAllTiles() {
        if (tiles != null) {
            for (int i=0; i<tiles.Count; i++) {
                Destroy(tiles[i].gameObject);
            }
            tiles = null;
        }
    }
    private void MakeAllTiles() {
        DestroyAllTiles();
        
        // TODO: Just do clusters?
        tiles = new List<MiniMapLevelTile>();
        WorldData wd = myLevel.WorldDataRef;
        foreach (LevelData ld in wd.levelDatas.Values) {
            MiniMapLevelTile tile = Instantiate(ResourcesHandler.Instance.MiniMapLevelTile).GetComponent<MiniMapLevelTile>();
            tile.Initialize(rt_tiles, ld);
            tiles.Add(tile);
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
    //  Events
    // ----------------------------------------------------------------
    private void OnStartLevel(Level level) {
        // Update map position!
        mapPosTarget = level.PosGlobal*mapScale * -1;
        
        // We changed worlds??
        int prevWorldIndex = myLevel==null ? -1 : myLevel.WorldIndex;
        this.myLevel = level;
        if (prevWorldIndex != level.WorldIndex) {
            MakeAllTiles();
            MapPos = mapPosTarget; // start at right level.
        }
        
        // Update tile visuals!
        foreach (MiniMapLevelTile tile in tiles) { // TODO: This more efficiently?
            tile.UpdateVisuals(level.LevelDataRef);
        }
    }
    
    
    
}
