using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClustSelMapNamespace {
    public class WorldView : MonoBehaviour {
        // Components
        [SerializeField] private CanvasGroup myCanvasGroup=null;
        [SerializeField] private Image i_youAreHereIcon=null;
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private TextMeshProUGUI t_worldName=null;
        private ClustTile[] clustTiles;
        // Properties
        public int WorldIndex { get; private set; }
        private bool isCurrWorld; // true if ClustSelController has us selected!
        // References
        private ClustSelController clustSelController;
        private ClustTile selectedClustTile; // set from save in Start. Updated when we change what's selected.
        
        // Getters (Public)
        public float Width { get { return myRectTransform.rect.width; } }
        public float Height { get { return myRectTransform.rect.height; } }
        public Vector2 AnchoredPos { get { return myRectTransform.anchoredPosition; } }
        // Getters (Private)
        private ClustTile GetClustTile(int clustIndex) {
            if (clustIndex<0 || clustIndex>=clustTiles.Length) { return null; }
            return clustTiles[clustIndex];
        }
        private RoomView GetRoomView(RoomData roomData) {
            if (roomData == null || roomData.MyCluster==null) { return null; } // Safety check.
            ClustTile clustTile = GetClustTile(roomData.MyCluster.ClustIndex);
            if (clustTile == null) { return null; } // Safety check.
            return clustTile.GetRoomView(roomData);
        }
        
        
        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize(ClustSelController clustSelController, Transform tf_parent, int worldIndex, Vector2 myPos) {
            this.clustSelController = clustSelController;
            this.WorldIndex = worldIndex;
            
            // Parent jazz!
            GameUtils.ParentAndReset(this.gameObject, tf_parent);
            this.gameObject.name = "World " + worldIndex;
            myRectTransform.anchoredPosition = myPos;
            
            // Look right!
            t_worldName.text = "World " + worldIndex;
            
            MakeClustTiles();
            UpdateYouAreHereIconPos();
            
            // Set lastPlayedClustTile from save!
            SetSelectedClustTile(clustTiles[GameManagers.Instance.DataManager.LastPlayedClustIndex(worldIndex)]);
        }
        private void MakeClustTiles() {
            WorldData myWorldData = GameManagers.Instance.DataManager.GetWorldData(WorldIndex);
            int NumClusts = myWorldData.clusters.Count;
            
            float tempY = 0;
            const float gapY = 6;
            clustTiles = new ClustTile[NumClusts];
            for (int i=0; i<NumClusts; i++) {
                RoomClusterData clustData = myWorldData.clusters[i];
                ClustTile newObj = Instantiate(ResourcesHandler.Instance.ClustSelMapClustTile).GetComponent<ClustTile>();
                Vector2 tilePos = new Vector2(0, tempY);
                newObj.Initialize(clustSelController, this, clustData, tilePos);
                clustTiles[i] = newObj;
                tempY -= newObj.Size.y + gapY;
            }
            // Size meee
            myRectTransform.sizeDelta = new Vector2(myRectTransform.rect.size.x, Mathf.Abs(tempY)+0);
        }
        private void UpdateYouAreHereIconPos() {
            RoomData roomData = GameManagers.Instance.DataManager.LastPlayedRoomData(WorldIndex);//GameManagers.Instance.DataManager.currRoomData;
            bool doShowIcon = roomData!=null && roomData.MyCluster!=null && roomData.MyCluster.IsUnlocked;
            i_youAreHereIcon.enabled = doShowIcon;
            if (doShowIcon) {
                RoomView roomView = GetRoomView(roomData);
                i_youAreHereIcon.transform.position = roomView.transform.position;
            }
        }
        
        
        
        public void OnSetWorldSelected(int selectedWorldIndex) {
            isCurrWorld = WorldIndex == selectedWorldIndex;
            // Enable/disable my elements!
            myCanvasGroup.interactable = isCurrWorld;
            // I'm selected? Select my last-played ClustTile!
            if (isCurrWorld) {
                //GameObject clustTileGO = lastSelectedClustGO==null ? null : lastSelectedClustGO.gameObject; // safety check.
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(selectedClustTile.gameObject);
            }
        }
        
        private void SetSelectedClustTile(ClustTile clustTile) {
            selectedClustTile = clustTile;
            clustSelController.OnSetSelectedClustTile(clustTile);
        }


        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        private void Update() {
            // YouAreHere icon!
            float alpha = MathUtils.SinRange(0.3f, 1.2f, Time.time*6f);
            GameUtils.SetUIGraphicAlpha(i_youAreHereIcon, alpha);
            // Update selection.
            if (isCurrWorld) {
                GameObject currSelGO = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                // Nothing selected? Auto-select our last clustTile, yo.
                if (currSelGO == null) {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(selectedClustTile.gameObject);
                }
                if (currSelGO!=null && selectedClustTile.gameObject != currSelGO) { // Selection changed!
                    ClustTile clustTile = currSelGO.GetComponent<ClustTile>();
                    if (clustTile != null) {
                        SetSelectedClustTile(clustTile);
                    }
                }
            }
        }
        
        
    }
}
