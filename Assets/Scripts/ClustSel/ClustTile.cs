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
        [SerializeField] private GameObject go_snacksLeft=null;
        [SerializeField] private Image i_back=null;
        [SerializeField] private Image i_checkmark=null;
        [SerializeField] private TextMeshProUGUI t_clustName=null;
        [SerializeField] private TextMeshProUGUI t_snacksReq=null;
        [SerializeField] private TextMeshProUGUI t_snacksLeft=null;
        [SerializeField] private RectTransform rt_rooms=null;
        // References
        private ClustSelController clustSelController;
        private RoomClusterData myClustData;
        private RoomView[] roomViews;
        // Properties
        private bool canUnlockMe;
        private float worldHue; // 0 to 1. The hue we use to color stuff in this WorldView.
        
        // Getters (Public)
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
            transform.SetAsFirstSibling(); // put behind other stuff.
            this.gameObject.name = "ClustTile " + myClustData.ClustIndex;
            myRectTransform.anchoredPosition = pos;
            
            t_clustName.text = (myClustData.ClustIndex+1).ToString();
            
            // Make RoomViews!
            AddRoomViews();
            
            RefreshVisuals();
        }
        
        private void AddRoomViews() {
            // Scale rooms to fit!
            Vector2 availableSize = rt_rooms.rect.size;
            Vector2 clustBoundsSize = myClustData.BoundsGlobal.size;
            float scale = Mathf.Min(
                availableSize.x/clustBoundsSize.x,
                availableSize.y/clustBoundsSize.y);
            
            //scale = Mathf.Min(0.4f, scale); // Keep RoomViews small.
            // Size myRectTransform!
            Vector2 sizeDiff = myRectTransform.rect.size - rt_rooms.rect.size;
            myRectTransform.sizeDelta = clustBoundsSize*scale + sizeDiff;
            
            // Add views!
            int NumRooms = myClustData.rooms.Count;
            roomViews = new RoomView[NumRooms];
            for (int i=0; i<NumRooms; i++) {
                RoomView newObj = Instantiate(ResourcesHandler.Instance.ClustSelRoomView).GetComponent<RoomView>();
                newObj.Initialize(rt_rooms, myClustData, myClustData.rooms[i], scale);
                roomViews[i] = newObj;
            }
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        private void RefreshVisuals() {
            //PlayerTypes tempPlayerType=PlayerTypes.Any; // TEMP HACK!!
            //if (myClustData.WorldIndex==1) { tempPlayerType = PlayerTypes.Plunga; }
            //else if (myClustData.WorldIndex==2) { tempPlayerType = PlayerTypes.Flatline; }
        
            // Values.
            int totalSnacksEaten = GameManagers.Instance.DataManager.SnackCountGame.Eaten_All;
            canUnlockMe = !myClustData.IsUnlocked && totalSnacksEaten>=myClustData.NumSnacksReq;
            
            // Texts and back!
            int numAdditionalSnacksReq = Mathf.Max(0, myClustData.NumSnacksReq - totalSnacksEaten);
            int numSnacksInClust = myClustData.SnackCount.Total(PlayerTypes.Any);
            //int numSnacksLeft = myClustData.SnackCount.Uneaten(tempPlayerType);
            myButton.interactable = myClustData.IsUnlocked;
            go_snacksReq.SetActive(!myClustData.IsUnlocked);
            //go_snacksLeft.SetActive(myClustData.IsUnlocked && numSnacksLeft>0);
            go_snacksLeft.SetActive(myClustData.IsUnlocked && numSnacksInClust>0);
            t_snacksReq.text = numAdditionalSnacksReq.ToString();
            t_snacksLeft.text = myClustData.SnackCount.Eaten_All + " / " + numSnacksInClust; //numSnacksLeft.ToString();
            i_back.color = myClustData.IsUnlocked || canUnlockMe ? new ColorHSB(worldHue,0.5f,1f, 0.4f).ToColor() : new Color(0,0,0, 0.08f);
            
            // RoomViews!
            Color roomColorVisited = new ColorHSB(worldHue, 0.5f, 0.95f).ToColor();
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