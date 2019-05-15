using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserOnOffer : MonoBehaviour {
    // Properties
    [SerializeField] public float DurOn = 1;
    [SerializeField] public float DurOff = 1;
    [SerializeField] public float StartOffset = 0;
    private float timeUntilToggle;
    // References
    private Laser myLaser; // assigned in Initialize.
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Laser myLaser, LaserData data) {
        this.myLaser = myLaser;
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
            myLaser.SetIsOn(true);
        }
        else { // start OFF with this much time left.
            timeUntilToggle += DurOff;
            myLaser.SetIsOn(false);
        }
    }
    
    
    
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
        timeUntilToggle -= Time.deltaTime;
        // ALMOST back ON?
        if (!myLaser.IsOn && timeUntilToggle < 0.4f) {
            myLaser.UpdateAlmostOn(timeUntilToggle);
        }
        // Time to toggle?
        if (timeUntilToggle <= 0) {
            if (myLaser.IsOn) { TurnOff(); }
            else { TurnOn(); }
        }
    }
    private void TurnOn() {
        timeUntilToggle += DurOn;
        myLaser.SetIsOn(true);
    }
    private void TurnOff() {
        timeUntilToggle += DurOff;
        myLaser.SetIsOn(false);
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
