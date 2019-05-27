using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(Spikes))]
public class SpikesEditor : UnityEditor.Editor {
    // References
    private Spikes mySpikes;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //DrawDefaultInspector();
        
        if (mySpikes == null) {
            mySpikes = (Spikes)target;
        }
        
        if (GUILayout.Button("Rotate 90°")) { mySpikes.Debug_Rotate(-90); }
        
        if (!mySpikes.HasOnOffer()) {
            if (GUILayout.Button("Add OnOffer")) {
                mySpikes.AddOnOffer(new OnOfferData(0.3f, 1.7f, 0f));
            }
        }
        else {
            if (GUILayout.Button("Remove OnOffer")) {
                mySpikes.RemoveOnOffer();
            }
        }
    }
}
