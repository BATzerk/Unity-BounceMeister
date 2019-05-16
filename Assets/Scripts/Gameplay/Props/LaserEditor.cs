using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Laser))]
public class LaserEditor : Editor {
    // References
    private Laser myLaser;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        if (myLaser == null) {
            myLaser = (Laser)target;
        }
        
        if (!myLaser.HasOnOffer()) {
            if (GUILayout.Button("Add OnOffer")) {
                myLaser.AddOnOffer(new OnOfferData(0.3f, 1.7f, 0f));
            }
        }
        else {
            if (GUILayout.Button("Remove OnOffer")) {
                myLaser.RemoveOnOffer();
            }
        }
    }
}
