using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spikes))]
public class SpikesEditor : Editor {
    // References
    private Spikes mySpikes;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //DrawDefaultInspector();
        
        if (mySpikes == null) {
            mySpikes = (Spikes)target;
        }
        
        if (GUILayout.Button("Rotate 90°")) {
            mySpikes.Debug_Rotate(-90);
        }
    }
}
