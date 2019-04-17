using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snack : Edible {
    // Components
    [SerializeField] private SpriteRenderer sr_aura=null;
    // Properties
    [SerializeField] private PlayerTypes playerType=PlayerTypes.Undefined;


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
    public void Initialize(Level _myLevel, SnackData data, int myIndex) {
        base.BaseInitialize(_myLevel, data);
        this.myIndex = myIndex;
        this.playerType = PlayerTypeHelper.TypeFromString(data.playerType);

        // Load wasEverEaten!
        wasEverEaten = SaveStorage.GetBool(SaveKeys.DidEatSnack(myLevel, myIndex));
        isEaten = wasEverEaten;
        
        
        UpdatePresence();
    }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdatePresence() {
        Player currPlayer = myLevel.Player;
        bool isMyType = currPlayer!=null && currPlayer.PlayerType()==playerType;
        
        // Update my color by my PlayerType.
        Color playerColor = PlayerBody.GetBodyColorNeutral(playerType);
        sr_aura.color = playerColor;
        sr_body.color = playerColor;
        
        sr_aura.enabled = !wasEverEaten;
        
        if (isMyType) {
            sr_body.enabled = !wasEverEaten;
            myCollider.enabled = !wasEverEaten;
        }
        else {
            myCollider.enabled = false;
            sr_body.enabled = !wasEverEaten;
            //sr_body.color = Color.Lerp(playerColor, new Color(0,0,0, 0.15f), 0.7f);
            //sr_aura.color = new Color(0,0,0, 0.07f);
            GameUtils.SetSpriteAlpha(sr_aura, 0.1f);
            GameUtils.SetSpriteAlpha(sr_body, 0.3f);
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
        SaveStorage.SetBool(SaveKeys.DidEatSnack(myLevel, myIndex), true);
        // Particle bursttt and visuals
        sr_aura.enabled = false;
        ps_collectedBurst.Emit(4);
        // Tell my WorldData!
        myLevel.WorldDataRef.OnPlayerEatSnack();
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
        bodyRotation -= 0.8f + Mathf.Sin(rotOffset+Time.time*1.5f) * 0.45f;

        float oscOffset = myIndex*1.5f; // if multiple Snacks in a level, this offsets their floaty animation.
        Vector2 driftOffset = new Vector2(
            Mathf.Cos(oscOffset+Time.time*2f) * 0.06f,
            Mathf.Sin(oscOffset+Time.time*3.6f) * 0.12f);
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
