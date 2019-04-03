using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snack : Edible, ISerializableData<SnackData> {
    // Components
    [SerializeField] private SpriteRenderer sr_aura=null;


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Level _myLevel, SnackData data, int myIndex) {
        base.BaseInitialize(_myLevel, data);
        this.myIndex = myIndex;

        // Load wasEverEaten!
        wasEverEaten = SaveStorage.GetBool(SaveKeys.DidEatSnack(myLevel, myIndex));

        //// Set wasEverEaten visuals.
        //if (wasEverEaten) {
        //    sr_body.color = new Color(0.2f,0.2f,0.2f, 0.25f);
        //}
        //sr_aura.enabled = !isEaten;
        // I've been eaten? Hide me entirely!
        this.gameObject.SetActive(!wasEverEaten);
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
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
    public SnackData SerializeAsData() {
        SnackData data = new SnackData {
            pos = pos
        };
        return data;
    }

}
