using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClustSelListNamespace {
    public class WorldView : MonoBehaviour {
        // Components
        [SerializeField] private Image i_youAreHereIcon=null;
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private TextMeshProUGUI t_worldName=null;
        private ClustRow[] clustRows;
        // Properties
        public int WorldIndex { get; private set; }
        // References
        private ClustSelController clustSelController;
        
        // Getters (Public)
        public float Width { get { return myRectTransform.rect.width; } }
        public float Height { get { return myRectTransform.rect.height; } }
        // Getters (Private)
        private ClustRow GetClustRow(int clustIndex) {
            if (clustIndex<0 || clustIndex>=clustRows.Length) { return null; }
            return clustRows[clustIndex];
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
            
            MakeClustRows();
            UpdateYouAreHereIconPos();
            
            // Start with last-played cluster selected.
            {
                int clustIndex = GameManagers.Instance.DataManager.LastPlayedClustIndex(worldIndex);
                ClustRow clustRow = GetClustRow(clustIndex);
                if (clustRow != null) {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(clustRow.gameObject);
                }
            }
        }
        private void MakeClustRows() {
            WorldData myWorldData = GameManagers.Instance.DataManager.GetWorldData(WorldIndex);
            int NumClusts = myWorldData.clusters.Count;
            
            float tempY = 0;
            const float gapY = 6;
            clustRows = new ClustRow[NumClusts];
            for (int i=0; i<NumClusts; i++) {
                RoomClusterData clustData = myWorldData.clusters[i];
                ClustRow newObj = Instantiate(ResourcesHandler.Instance.ClustSelListClustRow).GetComponent<ClustRow>();
                Vector2 objPos = new Vector2(0, tempY);
                newObj.Initialize(clustSelController, this, clustData, objPos);
                clustRows[i] = newObj;
                tempY -= newObj.Size.y + gapY;
            }
            // Size meee
            myRectTransform.sizeDelta = new Vector2(myRectTransform.rect.size.x, Mathf.Abs(tempY)+0);
        }
        private void UpdateYouAreHereIconPos() {
            int clustIndex = GameManagers.Instance.DataManager.LastPlayedClustIndex(WorldIndex);
            ClustRow clustRow = GetClustRow(clustIndex);
            if (clustRow != null) {
                i_youAreHereIcon.transform.position = clustRow.transform.position + new Vector3(-25,-29); // hacky hardcoded offset.
            }
            
            //RoomData roomData = GameManagers.Instance.DataManager.LastPlayedRoomData(WorldIndex);//GameManagers.Instance.DataManager.currRoomData;
            //bool doShowIcon = roomData!=null && roomData.MyCluster!=null && roomData.MyCluster.IsUnlocked;
            //i_youAreHereIcon.enabled = doShowIcon;
            //if (doShowIcon) {
            //    i_youAreHereIcon.transform.position = clustRows[roomData.MyCluster.ClustIndex].transform.position + new Vector3(-25,-29); // hacky hardcoded offset.
            //}
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
