using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ClustSelNamespace {
    public class ClustSelController : MonoBehaviour {
        // Components
        [SerializeField] private RectTransform rt_worldViews=null;
        [SerializeField] private TextMeshProUGUI t_snacksCollected=null;
        private Dictionary<int,WorldView> worldViews;


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
            float tempX = MainCanvas.Width*0.5f; // start with World 1 centered in screen.
            tempX += 20; // ok, shift it to the right a bit actually.
            for (int worldIndex=GameProperties.FirstWorld; worldIndex<=GameProperties.LastWorld; worldIndex++) {
                WorldView newObj = Instantiate(ResourcesHandler.Instance.ClustSelWorldView).GetComponent<WorldView>();
                Vector2 pos = new Vector2(tempX, -20);
                newObj.Initialize(this, rt_worldViews, worldIndex, pos);
                worldViews[worldIndex] = newObj;
                tempX += newObj.Width + 80;
                maxHeight = Mathf.Max(maxHeight, newObj.Height);
            }
            // Size rt_worldViews.
            rt_worldViews.sizeDelta = new Vector2(tempX+300, maxHeight+100);
            rt_worldViews.anchoredPosition = new Vector2(rt_worldViews.anchoredPosition.x, 0);
        }
        
        
        // ----------------------------------------------------------------
        //  Doers
        // ----------------------------------------------------------------
        public void StartGameAtClust(RoomClusterData clust) {
            SceneHelper.OpenGameplayScene(clust);
        }
        
        
        
        // ----------------------------------------------------------------
        //  Update
        // ----------------------------------------------------------------
        private void Update () {
            RegisterButtonInput();
        }
        
        private void RegisterButtonInput () {
            // Canvas has a selected element? Ignore ALL button input.
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
                return;
            }
    
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
                // Scene Changing
                if (Input.GetKeyDown(KeyCode.Return)) { SceneHelper.ReloadScene(); return; }
                else if (Input.GetKeyDown(KeyCode.J)) { SceneHelper.OpenScene(SceneNames.RoomJump); return; }
                else if (Input.GetKeyDown(KeyCode.M)) { SceneHelper.OpenScene(SceneNames.MapEditor); return; }
                else if (Input.GetKeyDown(KeyCode.G)) { SceneHelper.OpenScene(SceneNames.Gameplay); return; }
            }
        }
        
        

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Debug
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    #if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            if (UnityEditor.EditorApplication.isPlaying) {
                SceneHelper.ReloadScene();
            }
        }
    #endif
        
    }
}