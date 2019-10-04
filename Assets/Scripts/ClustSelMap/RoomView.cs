using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClustSelMapNamespace {
    public class RoomView : MonoBehaviour {
        // Components
        [SerializeField] private Image i_back=null;
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private RoomViewContents contents=null;
        // Properties
        public RoomData MyRoomData { get; private set; }
        
        
        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize(Transform tf_parent, RoomClusterData myClustData, RoomData myRoomData, float scale) {
            this.MyRoomData = myRoomData;
            
            // Parent jazz!
            GameUtils.ParentAndReset(this.gameObject, tf_parent);
            this.gameObject.name = "RoomView " + myRoomData.RoomKey;
            
            myRectTransform.sizeDelta = myRoomData.Size;// * scale;
            
            Vector2 pos = myRoomData.PosGlobal - myClustData.BoundsGlobal.center;
            pos += myRoomData.cameraBoundsData.pos; // hack-y! Just getting to work for now. Works around the rooms' local/global alignment mismatch.
            myRectTransform.anchoredPosition = pos;// * scale;
            
            contents.Initialize(this);
            
            // Secret and unvisited? Hide me!
            if (myRoomData.IsSecret && !myRoomData.HasPlayerBeenHere) {
                this.gameObject.SetActive(false);
            }
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        public void UpdateColor(Color roomColorVisited) {
            if (MyRoomData.HasPlayerBeenHere) {
                i_back.color = roomColorVisited;
            }
            else {
                float alpha = 0.5f;// MyRoomData.MyCluster.IsUnlocked ? 0.4f : 0.05f;
                i_back.color = new Color(0,0,0, alpha);
            }
        }
    }
}