using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ClustSelNamespace {
    public class WorldView : MonoBehaviour {
        // Components
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private TextMeshProUGUI t_worldName=null;
        private ClustTile[] clustTiles;
        // Properties
        public int WorldIndex { get; private set; }
        // References
        //private ClustSelController clustSelController;
        
        // Getters
        public float Width { get { return myRectTransform.rect.width; } }
        public float Height { get { return myRectTransform.rect.height; } }
        
        
        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize(ClustSelController clustSelController, Transform tf_parent, int worldIndex, Vector2 myPos) {
            //this.clustSelController = clustSelController;
            this.WorldIndex = worldIndex;
            
            // Parent jazz!
            GameUtils.ParentAndReset(this.gameObject, tf_parent);
            this.gameObject.name = "World " + worldIndex;
            myRectTransform.anchoredPosition = myPos;
            
            // Look right!
            t_worldName.text = "World " + worldIndex;
            
            // Make my ClustTiles!
            WorldData myWorldData = GameManagers.Instance.DataManager.GetWorldData(worldIndex);
            int NumClusts = myWorldData.clusters.Count;
            
            float tempY = -60;
            clustTiles = new ClustTile[NumClusts];
            for (int i=0; i<NumClusts; i++) {
                RoomClusterData clustData = myWorldData.clusters[i];
                ClustTile newObj = Instantiate(ResourcesHandler.Instance.ClustSelClustTile).GetComponent<ClustTile>();
                Vector2 tilePos = new Vector2(0, tempY);
                newObj.Initialize(clustSelController, this, clustData, tilePos);
                clustTiles[i] = newObj;
                tempY -= newObj.Size.y + 20;
            }
        }
    }
}
