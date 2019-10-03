using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClustSelListNamespace {
    public class ClustRow : MonoBehaviour {
        // Components
        [SerializeField] private RectTransform myRectTransform=null;
        [SerializeField] private Button myButton=null;
        [SerializeField] private Image i_back=null;
        [SerializeField] private TextMeshProUGUI t_clustName=null;
        [SerializeField] private Image[] i_snacks=null;
        // References
        private ClustSelController clustSelController;
        private RoomClusterData myClustData;
        // Properties
        private float worldHue; // 0 to 1. The hue we use to color stuff in this WorldView.
        
        // Getters (Public)
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
            transform.SetAsFirstSibling(); // put behind other stuff.
            this.gameObject.name = "ClustRow " + myClustData.ClustIndex;
            myRectTransform.anchoredPosition = pos;
            
            t_clustName.text = (myClustData.ClustIndex+1).ToString();
            
            // Add i_snacks!
            {
                int numEaten = myClustData.SnackCount.Eaten_All;
                int numTotal = myClustData.SnackCount.Total_All;
                i_snacks = new Image[numTotal];
                GameObject prefabGO = ResourcesHandler.Instance.ClustSelListClustRowSnack;
                float spacingX = 26;
                float snackIconsWidth = i_snacks.Length*spacingX;
                for (int i=0; i<i_snacks.Length; i++) {
                    float posX = -2 - snackIconsWidth + (i * 26);
                    Image img = Instantiate(prefabGO).GetComponent<Image>();
                    img.name = "Snack " + i;
                    GameUtils.ParentAndReset(img.gameObject, this.transform);
                    img.rectTransform.anchoredPosition = new Vector2(posX, 0);
                    // Not eaten? Darker img!
                    if (i >= numEaten) {
                        img.color = new Color(0,0,0, 0.8f);
                    }
                    i_snacks[i] = img;
                }
            }
            
            RefreshVisuals();
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        private void RefreshVisuals() {
            myButton.interactable = myClustData.IsUnlocked;
            i_back.color = myClustData.IsUnlocked ? new ColorHSB(worldHue,0.5f,1f, 0.1f).ToColor() : new Color(0,0,0, 0.08f);
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
                if (myClustData.IsUnlocked) {
                    clustSelController.StartGameAtClust(myClustData);
                }
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