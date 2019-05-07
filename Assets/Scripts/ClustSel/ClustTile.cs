using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClustSelNamespace {
    public class ClustTile : MonoBehaviour {
        // Components
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private Button myButton=null;
        [SerializeField] private GameObject go_snacksReq=null;
        [SerializeField] private Image i_back=null;
        [SerializeField] private TextMeshProUGUI t_snacksReq=null;
        [SerializeField] private Transform tf_rooms=null;
        // References
        private ClustSelController clustSelController;
        private RoomClusterData myClustData;
        private RoomView[] roomViews;
        // Properties
        private bool canUnlockMe;
        private float worldHue; // 0 to 1. The hue we use to color stuff in this WorldView.
        
        // Getters
        public Vector2 Size { get { return myRectTransform.rect.size; } }
        
        
        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize(ClustSelController clustSelController, WorldView myWorldView, RoomClusterData myClustData, Vector2 pos) {
            this.clustSelController = clustSelController;
            this.myClustData = myClustData;
            worldHue = ((150 + myClustData.WorldIndex*40)/360f) % 1;
            
            // Parent jazz!
            GameUtils.ParentAndReset(this.gameObject, myWorldView.transform);
            this.gameObject.name = "ClustTile " + myClustData.ClustIndex;
            myRectTransform.anchoredPosition = pos;
            
            // Make RoomViews!
            AddRoomViews();
            
            RefreshVisuals();
        }
        
        private void AddRoomViews() {
            // Scale rooms to fit!
            Vector2 availableSize = myRectTransform.rect.size;
            Vector2 clustBoundsSize = myClustData.BoundsGlobal.size;
            float scale = Mathf.Min(
                availableSize.x/clustBoundsSize.x,
                availableSize.y/clustBoundsSize.y);
            // Size myRectTransform!
            myRectTransform.sizeDelta = clustBoundsSize * scale;
            
            //scale *= 0.8f; // scale 'em down extra, for bloat purposes.
            scale = Mathf.Min(0.4f, scale); // Keep RoomViews small.
            tf_rooms.localScale = Vector3.one * scale * 0.72f; // scale 'em down extra, for bloat purposes.
            
            // Add views!
            int NumRooms = myClustData.rooms.Count;
            roomViews = new RoomView[NumRooms];
            for (int i=0; i<NumRooms; i++) {
                RoomView newObj = Instantiate(ResourcesHandler.Instance.ClustSelRoomView).GetComponent<RoomView>();
                newObj.Initialize(tf_rooms, myClustData, myClustData.rooms[i]);
                roomViews[i] = newObj;
            }
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        private void RefreshVisuals() {
            // Values.
            int numSnacksTotal = GameManagers.Instance.DataManager.SnackCountGame.eaten[PlayerTypes.Any];
            canUnlockMe = !myClustData.IsUnlocked && numSnacksTotal>=myClustData.NumSnacksReq;
            
            // Texts and back!
            myButton.interactable = myClustData.IsUnlocked;
            go_snacksReq.SetActive(!myClustData.IsUnlocked);
            t_snacksReq.text = myClustData.NumSnacksReq.ToString();
            i_back.color = myClustData.IsUnlocked || canUnlockMe ? new ColorHSB(worldHue,0.5f,1f, 0.4f).ToColor() : new Color(0,0,0, 0.08f);
            
            // RoomViews!
            Color roomColorVisited = new ColorHSB(worldHue, 0.5f, 0.95f).ToColor();
            foreach (RoomView roomView in roomViews) {
                roomView.UpdateColor(roomColorVisited);
            }
        }
        private void UnlockMe() {
            myClustData.SetIsUnlocked(true);
            RefreshVisuals();
        }
        
        
        
        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        public void OnClick() {
            // DEBUG
            if (Input.GetKey(KeyCode.LeftShift)) {
                Debug_ToggleIsUnlocked();
            }
            else {
                if (canUnlockMe) {
                    UnlockMe();
                }
                else if (myClustData.IsUnlocked) {
                    clustSelController.StartGameAtClust(myClustData);
                }
            }
        }
        
        
        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        private void Update() {
            if (canUnlockMe) {
                // Oscillate alpha!
                float alpha = MathUtils.SinRange(0.3f, 0.9f, Time.time*8f + myClustData.WorldIndex*1.2f-myClustData.ClustIndex);
                GameUtils.SetUIGraphicAlpha(i_back, alpha);
            }
        }
        
        
        // ----------------------------------------------------------------
        //  Debug
        // ----------------------------------------------------------------
        private void Debug_ToggleIsUnlocked() {
            myClustData.SetIsUnlocked(!myClustData.IsUnlocked);
            RefreshVisuals();
        }
        
        
    }
}