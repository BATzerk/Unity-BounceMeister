using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ClustSelMapNamespace {
    public class ClustSelController : MonoBehaviour {
        // Components
        [SerializeField] private RectTransform rt_worldViews=null;
        [SerializeField] private TextMeshProUGUI t_snacksCollected=null;
        private Dictionary<int,WorldView> worldViews;
        // Properties
        private int framesAlive=0;
        private int selectedWorldIndex;


        // ----------------------------------------------------------------
        //  Start
        // ----------------------------------------------------------------
        private void Start() {
            Time.timeScale = 1;
            
            // Update text.
            t_snacksCollected.text = GameManagers.Instance.DataManager.SnackCountGame.Eaten_All.ToString();
            
            // Make worldViews!
            worldViews = new Dictionary<int, WorldView>();
            
            float maxHeight = 0;
            float tempX = 800; // start with World 1 centered in screen.
            for (int worldIndex=GameProperties.FirstWorld; worldIndex<=GameProperties.LastWorld; worldIndex++) {
                WorldView newObj = Instantiate(ResourcesHandler.Instance.ClustSelMapWorldView).GetComponent<WorldView>();
                Vector2 pos = new Vector2(tempX, 0);
                newObj.Initialize(this, rt_worldViews, worldIndex, pos);
                worldViews[worldIndex] = newObj;
                tempX += 800;//newObj.Width + 80;
                maxHeight = Mathf.Max(maxHeight, newObj.Height);
            }
            // Size rt_worldViews.
            rt_worldViews.sizeDelta = new Vector2(tempX+300, maxHeight+100);
            rt_worldViews.anchoredPosition = new Vector2(rt_worldViews.anchoredPosition.x, 0);
            
            // Start with last world played!
            int lastWorldPlayed = GameManagers.Instance.DataManager.LastPlayedRoomAddress().world;
            SetWorldSelected(lastWorldPlayed);
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        public void StartGameAtClust(RoomClusterData clust) {
            SceneHelper.OpenGameplayScene(clust);
        }
        private void ChangeWorldSelected(int indexDelta) {
            SetWorldSelected(selectedWorldIndex + indexDelta);
        }
        private void SetWorldSelected(int val) {
            selectedWorldIndex = Mathf.Clamp(val, GameProperties.FirstWorld,GameProperties.LastWorld);
            // Tell the boys!
            foreach (WorldView wv in worldViews.Values) {
                wv.OnSetWorldSelected(selectedWorldIndex);
            }
            // Show selected one!
            Vector2 wvsPos = worldViews[selectedWorldIndex].AnchoredPos;
            float targetPosX = -wvsPos.x + 400;
            LeanTween.value(this.gameObject, SetWorldViewsAnchoredPosX, rt_worldViews.anchoredPosition.x,targetPosX, 0.3f).setEaseOutQuint();
        }
        private void SetWorldViewsAnchoredPosX(float val) {
            rt_worldViews.anchoredPosition = new Vector2(val, rt_worldViews.anchoredPosition.y);
        }
        
        
        public void OnSetSelectedClustTile(ClustTile clustTile) {
            // Set scroll-layer pos!
            float currScrollPosY = rt_worldViews.anchoredPosition.y;
            float scrollPosY = -clustTile.AnchoredPos.y;
            scrollPosY = Mathf.Clamp(currScrollPosY, scrollPosY-400, scrollPosY);
            rt_worldViews.anchoredPosition = new Vector2(rt_worldViews.anchoredPosition.x, scrollPosY);
        }
        
        
        
        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        private void Update () {
            RegisterButtonInput();
            framesAlive ++;
        }
        
        private void RegisterButtonInput () {
            if (framesAlive < 2) { return; } // Ignore first few frames.
            
            //// Canvas has a selected element? Ignore ALL button input.
            //if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
            //    return;
            //}
    
            bool isKey_alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            bool isKey_control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isKey_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            // SHIFT + ___
            if (isKey_shift) {
            }
            // CONTROL + ___
            if (isKey_control) {
                // CONTROL + DELETE = Clear all save data!
                if (Input.GetKeyDown(KeyCode.Delete)) {
                    GameManagers.Instance.DataManager.ClearAllSaveData();
                    SceneHelper.ReloadScene();
                    return;
                }
            }
            
            // NOTHING + _____
            if (!isKey_alt && !isKey_shift && !isKey_control) {
                if (false) {}
                else if (Input.GetKeyDown(KeyCode.LeftArrow)) { ChangeWorldSelected(-1); }
                else if (Input.GetKeyDown(KeyCode.RightArrow)) { ChangeWorldSelected( 1); }
                // Scene Changing
                else if (Input.GetKeyDown(KeyCode.R)) { SceneHelper.ReloadScene(); return; }
                else if (Input.GetKeyDown(KeyCode.G)) { SceneHelper.OpenScene(SceneNames.Gameplay); return; }
                else if (Input.GetKeyDown(KeyCode.J)) { SceneHelper.OpenScene(SceneNames.RoomJump); return; }
                else if (Input.GetKeyDown(KeyCode.M)) { SceneHelper.OpenScene(SceneNames.MapEditor); return; }
            }
        }
        
        
    }
}