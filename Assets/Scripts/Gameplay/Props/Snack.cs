using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snack : Edible {
    // Components
    [SerializeField] private SpriteRenderer sr_aura=null;
    // Properties
    [SerializeField] private PlayerTypes playerType=PlayerTypes.Undefined;
    private float driftAmp; // body-drifting amplitude. Higher is bigger "floatiness".
    private float rotScale; // 1 is rotate normal speed. 0.3 will rotate much slower.


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    private void Awake() {
        // Add event listeners!
        GameManagers.Instance.EventManager.PlayerInitEvent += OnPlayerInit;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.PlayerInitEvent -= OnPlayerInit;
    }
    public void Initialize(Room _myRoom, SnackData data, int myIndex) {
        base.BaseInitialize(_myRoom, data);
        this.myIndex = myIndex;
        this.playerType = PlayerTypeHelper.TypeFromString(data.playerType);

        // Load wasEverEaten!
        wasEverEaten = SaveStorage.GetBool(SaveKeys.DidEatSnack(myRoom, myIndex));
        isEaten = wasEverEaten;
        
        UpdatePresence();
    }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdatePresence() {
        Player currPlayer = myRoom.Player;
        bool isMyType = currPlayer!=null && currPlayer.PlayerType()==playerType;
        
        // Update my color by my PlayerType.
        Color playerColor = PlayerBody.GetBodyColorNeutral(playerType);
        
        sr_aura.enabled = !wasEverEaten;
        driftAmp = 1f; // default these visual values.
        rotScale = 1f;
        
        // Matching type!
        if (isMyType) {
            sr_body.enabled = !wasEverEaten;
            myCollider.enabled = !wasEverEaten;
            sr_aura.color = playerColor;
            sr_body.color = playerColor;
        }
        // Different type.
        else {
            myCollider.enabled = false;
            sr_body.enabled = !wasEverEaten;
            // I'm unlocked! Color me like the PlayerType.
            if (GameManagers.Instance.DataManager.IsPlayerTypeUnlocked(playerType)) {
                GameUtils.SetSpriteColor(sr_aura, playerColor, 0.14f);
                GameUtils.SetSpriteColor(sr_body, playerColor, 0.3f);
            }
            // We HAVEN'T unlocked this PlayerType. Make me gray until we do.
            else {
                driftAmp = 0.15f;
                rotScale = 0.1f;
                sr_aura.color = new Color(0,0,0, 0.1f);
                sr_body.color = new Color(0,0,0, 0.2f);
            }
        }
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnPlayerInit(Player player) {
        UpdatePresence();
    }
    override public void GetEaten() {
        base.GetEaten();
        // Save the value!
        SaveStorage.SetBool(SaveKeys.DidEatSnack(myRoom, myIndex), true);
        // Particle bursttt and visuals
        sr_aura.enabled = false;
        ps_collectedBurst.Emit(4);
        // Tell my WorldData!
        myRoom.WorldDataRef.OnPlayerEatSnack();
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        UpdateBodyPosRot();
    }
    private void UpdateBodyPosRot() {
        if (isEaten) { return; } // If I'm toast, don't do any position updating.
        
        float rotOffset = myIndex*1.3f;
        float rotDelta = -0.8f - Mathf.Sin(rotOffset+Time.time*1.5f) * 0.45f;
        rotDelta *= rotScale;
        bodyRotation += rotDelta;

        float oscOffset = myIndex*1.5f; // if multiple Snacks in a room, this offsets their floaty animation.
        Vector2 driftOffset = new Vector2(
            Mathf.Cos(oscOffset+Time.time*2f) * driftAmp*0.06f,
            Mathf.Sin(oscOffset+Time.time*3.6f) * driftAmp*0.12f);
        Vector2 targetPos;
        if (playerHoldingMe != null) {
            targetPos = playerHoldingMe.PosLocal + new Vector2(0, 3.3f);
        }
        else {
            targetPos = this.pos;
        }
        targetPos += driftOffset;

        // Make it relative.
        targetPos -= this.pos;
        bodyPos += (targetPos - bodyPos) / 6f;
    }



    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        SnackData data = new SnackData {
            pos = pos,
            playerType = playerType.ToString(),
        };
        return data;
    }

}
