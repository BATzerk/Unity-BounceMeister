using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserOnOffer : MonoBehaviour {
    // Properties
    [SerializeField] public float DurOn = 1;
    [SerializeField] public float DurOff = 1;
    private float timeUntilToggle;
    // References
    private Laser myLaser; // assigned in Initialize.
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Laser myLaser) {
        this.myLaser = myLaser;
        
        timeUntilToggle = DurOn;
        myLaser.SetIsOn(true);
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
