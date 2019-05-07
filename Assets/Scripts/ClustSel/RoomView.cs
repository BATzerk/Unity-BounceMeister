using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClustSelNamespace {
    public class RoomView : MonoBehaviour {
        // Components
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private Image i_back=null;
        // Properties
        private RoomData myRoomData;
        
        
        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize(Transform tf_parent, RoomClusterData myClustData, RoomData myRoomData) {
            this.myRoomData = myRoomData;
            
            // Parent jazz!
            GameUtils.ParentAndReset(this.gameObject, tf_parent);
            this.gameObject.name = "RoomView " + myRoomData.RoomKey;
            
            myRectTransform.sizeDelta = myRoomData.BoundsLocal.size;
            
            Vector2 pos = myRoomData.PosGlobal - myClustData.BoundsGlobal.center;
            pos += myRoomData.cameraBoundsData.myRect.center; // hack-y! Just getting to work for now. Works around the rooms' local/global alignment mismatch.
            myRectTransform.anchoredPosition = pos;
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        public void UpdateColor(Color roomColorVisited) {
            if (myRoomData.HasPlayerBeenHere) {
                i_back.color = roomColorVisited;
            }
            else {
                float alpha = myRoomData.MyCluster.IsUnlocked ? 0.4f : 0.05f;
                i_back.color = new Color(0,0,0, alpha);
            }
        }
    }
}