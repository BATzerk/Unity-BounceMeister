using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropOnOffer : MonoBehaviour {
    // Properties
    [SerializeField] public float DurOn = 0.3f;
    [SerializeField] public float DurOff = 1.7f;
    [SerializeField] public float StartOffset = 0;
    private float timeUntilToggle;
    // References
    private IOnOffable myProp; // assigned in Initialize.
    
    public OnOfferData ToData() {
        return new OnOfferData(this);
    }
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(IOnOffable myProp, OnOfferData data) {
        this.myProp = myProp;
        DurOn = data.durOn;
        DurOff = data.durOff;
        StartOffset = data.startOffset;
        
        // Make appliedOffset, which is StartOffset between 0 and total-dur.
        float durTotal = DurOn + DurOff;
        float appliedOffset = StartOffset;
        if (appliedOffset < 0) { appliedOffset += durTotal; }
        appliedOffset = appliedOffset % (durTotal);
        
        // Use StartOffset!
        timeUntilToggle = DurOn;
        timeUntilToggle -= appliedOffset;
        if (timeUntilToggle >= 0) { // start ON with this much time left.
            myProp.SetIsOn(true);
        }
        else { // start OFF with this much time left.
            timeUntilToggle += DurOff;
            myProp.SetIsOn(false);
        }
        
        // Move OnOffer component just under my Script, for easiness.
        #if UNITY_EDITOR
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
        #endif
    }
    
    
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        // Safety check for edit-mode.
        if (myProp == null) { myProp = GetComponent<IOnOffable>(); }
        
        timeUntilToggle -= GameTimeController.RoomDeltaTime;
        // ALMOST back ON?
        if (!myProp.IsOn() && timeUntilToggle < 0.25f) {
            myProp.UpdateAlmostOn(timeUntilToggle);
        }
        // Time to toggle?
        if (timeUntilToggle <= 0) {
            if (myProp.IsOn()) { TurnOff(); }
            else { TurnOn(); }
        }
    }
    private void TurnOn() {
        timeUntilToggle += DurOn;
        myProp.SetIsOn(true);
    }
    private void TurnOff() {
        timeUntilToggle += DurOff;
        myProp.SetIsOn(false);
    }
    
    
    
    // ----------------------------------------------------------------
    //  Coroutine_OnOffing
    // ----------------------------------------------------------------
    //private IEnumerator Coroutine_OnOffing() {
    //    while (true) {
    //        myLaser.SetIsOn(true);
    //        yield return new WaitForSeconds(DurOn);
            
    //        myLaser.SetIsOn(false);
    //        yield return new WaitForSeconds(DurOff);
    //    }
    //}
    
    
}
