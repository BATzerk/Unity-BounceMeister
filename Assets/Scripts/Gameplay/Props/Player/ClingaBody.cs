using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClingaBody : PlayerBody {
    // Components
    [SerializeField] private SpriteRenderer[] sr_clingHands=null; // T, R, B, L.
    // References
    private Clinga myClinga;
    
    // Setters
    //private float visualRotation {
    //    get { return this.transform.localEulerAngles.z; }
    //    set { this.transform.localEulerAngles = new Vector3(0,0, value); }
    //}
    
    
    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    override protected void Awake() {
        base.Awake();
        myClinga = myBasePlayer as Clinga;
        
        OnChangeClingSydes(); // refresh cling visuals.
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnChangeClingSydes() {
        for (int i=0; i<sr_clingHands.Length; i++) {
            sr_clingHands[i].enabled = myClinga.IsClingSyde(i);
        }
        //if (myClinga.IsClingSyde(Sides.T)) {
        //    visualRotation = 180;
        //}
        //else if (myClinga.IsClingSyde(Sides.L)) {
        //    visualRotation = -90;
        //}
        //else if (myClinga.IsClingSyde(Sides.R)) {
        //    visualRotation =  90;
        //}
        //else {
        //    visualRotation = 0;
        //}
    }
}
