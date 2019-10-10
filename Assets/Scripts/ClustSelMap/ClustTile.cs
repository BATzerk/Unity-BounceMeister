using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClustSelMapNamespace {
    public class ClustTile : MonoBehaviour {
        // Components
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private Button myButton=null;
        [SerializeField] private GameObject go_snacksReq=null;
        //[SerializeField] private GameObject go_snacksLeft=null;
        [SerializeField] private Image i_back=null;
        [SerializeField] private Image i_checkmark=null;
        [SerializeField] private TextMeshProUGUI t_clustName=null;
        [SerializeField] private TextMeshProUGUI t_snacksReq=null;
        //[SerializeField] private TextMeshProUGUI t_snacksLeft=null;
        [SerializeField] private RectTransform rt_roomsRect=null; // the outer parent; represents actual bounds for what's rendered.
        [SerializeField] private RectTransform rt_roomsScaled=null; // this is inside rt_roomsRect; this is scaled; rooms go in THIS.
        // References
        private ClustSelController clustSelController;
        private RoomClusterData myClustData;
        private RoomView[] roomViews;
        // Properties
        private bool canUnlockMe;
        private float worldHue; // 0 to 1. The hue we use to color stuff in this WorldView.
        
        // Getters (Public)
        public Vector2 AnchoredPos { get { return myRectTransform.anchoredPosition; } }
        public Vector2 Size { get { return myRectTransform.rect.size; } }
        public RoomView GetRoomView(RoomData roomData) {
            foreach (RoomView view in roomViews) {
                if (view.MyRoomData == roomData) {
                    return view;
                }
            }
            return null;
        }
        
        
        // ----------------------------------------------------------------
        //  Initialize
        // ----------------------------------------------------------------
        public void Initialize(ClustSelController clustSelController, WorldView myWorldView, RoomClusterData myClustData, Vector2 pos) {
            this.clustSelController = clustSelController;
            this.myClustData = myClustData;
            worldHue = ((150 + myClustData.WorldIndex*40)/360f) % 1;
            
            // Parent jazz!
            GameUtils.ParentAndReset(this.gameObject, myWorldView.transform);
            //transform.SetAsFirstSibling(); // put behind other stuff.
            this.gameObject.name = "ClustTile " + myClustData.ClustIndex;
            myRectTransform.anchoredPosition = pos;
            
            t_clustName.text = (myClustData.ClustIndex+1).ToString();
            
            // Make RoomViews!
            AddRoomViews();
            
            // Add i_snacks!
            if (myClustData.IsUnlocked)
            {
                int numEaten = myClustData.SnackCount.Eaten_All;
                int numTotal = myClustData.SnackCount.Total_All;
                GameObject prefabGO = ResourcesHandler.Instance.ClustSelListClustRowSnack;
                float spacingX = 18;
                float snackIconsWidth = numTotal*spacingX;
                for (int i=0; i<numTotal; i++) {
                    float posX = -2 - snackIconsWidth + (i * spacingX);
                    Image img = Instantiate(prefabGO).GetComponent<Image>();
                    img.name = "Snack " + i;
                    GameUtils.ParentAndReset(img.gameObject, this.transform);
                    img.rectTransform.anchoredPosition = new Vector2(posX, 0);
                    // Not eaten? Darker img!
                    if (i >= numEaten) {
                        img.color = new Color(0,0,0, 0.8f);
                    }
                }
            }
            
            RefreshVisuals();
        }
        
        private void AddRoomViews() {
            // Scale rooms to fit!
            Vector2 availableSize = rt_roomsRect.rect.size;
            Vector2 clustBoundsSize = myClustData.BoundsGlobal.size;
            float scale = Mathf.Min(
                availableSize.x/clustBoundsSize.x,
                availableSize.y/clustBoundsSize.y);
            scale = Mathf.Min(0.24f, scale); // Keep RoomViews small.
            
            // Size myRectTransform!
            //Vector2 sizeDiff = myRectTransform.rect.size - availableSize;
            //myRectTransform.sizeDelta = clustBoundsSize*scale + sizeDiff;
            rt_roomsScaled.localScale = Vector3.one * scale;
            
            // Add views!
            int NumRooms = myClustData.rooms.Count;
            roomViews = new RoomView[NumRooms];
            for (int i=0; i<NumRooms; i++) {
                RoomView newObj = Instantiate(ResourcesHandler.Instance.ClustSelMapRoomView).GetComponent<RoomView>();
                newObj.Initialize(rt_roomsScaled, myClustData, myClustData.rooms[i], scale);
                roomViews[i] = newObj;
            }
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        private void RefreshVisuals() {
            // Values.
            int totalSnacksEaten = GameManagers.Instance.DataManager.SnackCountGame.Eaten_All;
            canUnlockMe = !myClustData.IsUnlocked && totalSnacksEaten>=myClustData.NumSnacksReq;
            
            // Texts and back!
            int numAdditionalSnacksReq = Mathf.Max(0, myClustData.NumSnacksReq - totalSnacksEaten);
            int numSnacksInClust = myClustData.SnackCount.Total_All;
            //myButton.interactable = myClustData.IsUnlocked;
            go_snacksReq.SetActive(!myClustData.IsUnlocked);
            //go_snacksLeft.SetActive(myClustData.IsUnlocked && numSnacksInClust>0);
            t_snacksReq.text = numAdditionalSnacksReq.ToString();
            //t_snacksLeft.text = myClustData.SnackCount.Eaten_All + " / " + numSnacksInClust; //numSnacksLeft.ToString();
            Color backColor = new ColorHSB(worldHue,0.5f,0.5f).ToColor();
            if (!myClustData.IsUnlocked && !canUnlockMe) { // locked? Make darker.
                backColor = Color.Lerp(backColor, Color.black, 0.5f);
            }
            i_back.color = backColor;
            
            // RoomViews!
            Color roomColorVisited = new ColorHSB(worldHue, 0.5f, 0.95f, 0.1f).ToColor();
            foreach (RoomView roomView in roomViews) {
                roomView.UpdateColor(roomColorVisited);
            }
            
            // Completion-ness!
            bool didCompleteClust = myClustData.IsUnlocked && !myClustData.SnackCount.AreUneatenSnacks(PlayerTypes.Any);// && myClustData.HasPlayerBeenInEveryRoom();
            i_checkmark.color = didCompleteClust ? new Color(0.3f,1f,0f) : new Color(0,0,0, 0.1f);
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