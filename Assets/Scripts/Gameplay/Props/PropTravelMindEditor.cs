using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PropTravelMind))]
public class PropTravelMindEditor : Editor {
    // References
    private PropTravelMind myTravelMind;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //DrawDefaultInspector();
        
        if (myTravelMind == null) {
            myTravelMind = (PropTravelMind)target;
        }
        
        if (GUILayout.Button("Set Pos A")) {
            myTravelMind.Debug_SetPosA();
        }
        if (GUILayout.Button("Set Pos B")) {
            myTravelMind.Debug_SetPosB();
        }
    }
}
