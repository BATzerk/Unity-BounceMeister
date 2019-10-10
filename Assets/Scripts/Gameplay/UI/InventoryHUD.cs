using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHUD : MonoBehaviour {
    //// Enums
    //private enum VisibilityTypes { None, Faint, Full }
    // Components
    [SerializeField] private CanvasGroup myCanvasGroup=null;
    // Properties
    private float targetAlpha;
    private float alphaEaseSpeed; // HIGHER is FASTER. 1 is instant, 0 is never.
    private float timeSincePlayerInput=Mathf.Infinity; // in SECONDS. If there's no input, this is added to every frame.
    private float timeWhenInventoryChanged=Mathf.NegativeInfinity;
    //private VisibilityTypes visibility;
    // References
    //[SerializeField] private GameController gameController;
    
    
        
    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Start() {
        // Add event listeners!
        GameManagers.Instance.EventManager.CoinsCollectedChangedEvent += OnCoinsCollectedChanged;
        GameManagers.Instance.EventManager.SnackCountGameChangedEvent += OnSnackCountChanged;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.CoinsCollectedChangedEvent -= OnCoinsCollectedChanged;
        GameManagers.Instance.EventManager.SnackCountGameChangedEvent -= OnSnackCountChanged;
    }
    
    
        
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnCoinsCollectedChanged() {
        timeWhenInventoryChanged = Time.time;
    }
    private void OnSnackCountChanged() {
        timeWhenInventoryChanged = Time.time;
    }


        
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        // Update timeSincePlayerInput!
        {
            bool isPlayerInput = InputController.Instance.LeftStick.magnitude > 0.1f
                || InputController.Instance.IsJump_Press
                || InputController.Instance.IsAction_Press;
            if (isPlayerInput) {
                timeSincePlayerInput = 0;
            }
            else {
                timeSincePlayerInput += Time.deltaTime;
            }
        }
        
        // Update visibility type!
        {
            // Default to hidden
            targetAlpha = 0;
            alphaEaseSpeed = 0.09f;
            // Show, actually?
            if (timeSincePlayerInput > 2f) {
                targetAlpha = 1;
                alphaEaseSpeed = 0.015f;
            }
            else if (Time.time < timeWhenInventoryChanged+3) {
                targetAlpha = 1;
                alphaEaseSpeed = 0.4f;
            }
        }
        
        // Apply alpha!
        {
            float delta = MathUtils.Sign(targetAlpha-myCanvasGroup.alpha) * alphaEaseSpeed;
            //myCanvasGroup.alpha += (targetAlpha-myCanvasGroup.alpha) * alphaEaseSpeed;
            myCanvasGroup.alpha += delta;
        }
    }

}
