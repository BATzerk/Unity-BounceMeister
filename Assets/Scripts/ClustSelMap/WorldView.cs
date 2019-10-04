using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClustSelMapNamespace {
    public class WorldView : MonoBehaviour {
        // Components
        [SerializeField] private Image i_youAreHereIcon=null;
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private TextMeshProUGUI t_worldName=null;
        private ClustTile[] clustTiles;
        // Properties
        public int WorldIndex { get; private set; }
        // References
        private ClustSelController clustSelController;
        
        // Getters (Public)
        public float Width { get { return myRectTransform.rect.width; } }
        public float Height { get { return myRectTransform.rect.height; } }
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


        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        private void Update() {
            float alpha = MathUtils.SinRange(0.3f, 1.2f, Time.time*6f);
            GameUtils.SetUIGraphicAlpha(i_youAreHereIcon, alpha);
        }
        
        
    }
}
