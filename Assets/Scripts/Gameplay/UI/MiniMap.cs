using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour {
    // Components
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private RectTransform rt_tiles;
    private Dictionary<string,MiniMapLevelTile> tiles; // levelKey is key.
    // Properties
    private const float mapScale = 0.5f;//0.8f; // NOTE: Unity units to Screen units automatically makes levels way smaller (1 Unity unit is 1 pixel).
    private Vector2 mapPosTarget;
    // References
    private Level currLevel;
    
    // Getters (Private)
    private LevelData currLevelData { get { return currLevel.LevelDataRef; } }
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
        GameManagers.Instance.EventManager.SnacksCollectedChangedEvent += OnSnacksCollectedChanged;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.StartLevelEvent -= OnStartLevel;
        GameManagers.Instance.EventManager.SnacksCollectedChangedEvent -= OnSnacksCollectedChanged;
    }
    
    private void DestroyAllTiles() {
        if (tiles != null) {
            foreach (MiniMapLevelTile tile in tiles.Values) {
                Destroy(tile.gameObject);
            }
            tiles = null;
        }
    }
    private void MakeAllTiles() {
        DestroyAllTiles();
        
        // TODO: Just do clusters?
        tiles = new Dictionary<string, MiniMapLevelTile>();
        WorldData wd = currLevel.WorldDataRef;
        foreach (LevelData ld in wd.levelDatas.Values) {
            MiniMapLevelTile tile = Instantiate(ResourcesHandler.Instance.MiniMapLevelTile).GetComponent<MiniMapLevelTile>();
            tile.Initialize(rt_tiles, ld);
            tiles.Add(ld.levelKey, tile);
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
        foreach (MiniMapLevelTile tile in tiles.Values) {
            tile.UpdateVisuals(currLevelData);
        }
    }
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnStartLevel(Level level) {
        // Update map position!
        mapPosTarget = level.PosGlobal*mapScale * -1;
        
        // We changed worlds??
        int prevWorldIndex = currLevel==null ? -1 : currLevel.WorldIndex;
        this.currLevel = level;
        if (prevWorldIndex != level.WorldIndex) {
            MakeAllTiles();
            MapPos = mapPosTarget; // start at right level.
        }
        
        // Update tile visuals!
        UpdateAllTilesVisuals();// TODO: This more efficiently?
    }
    private void OnSnacksCollectedChanged(int worldIndex) {
        tiles[currLevel.LevelKey].UpdateVisuals(currLevelData); // Update the visuals of the current LevelTile.
    }
    
    
    
}
