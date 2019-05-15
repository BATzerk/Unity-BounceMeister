using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Buzzsaw))]
public class BuzzsawEditor : Editor {
    // References
    private Buzzsaw myBuzzsaw;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //DrawDefaultInspector();
        
        if (myBuzzsaw == null) {
            myBuzzsaw = (Buzzsaw)target;
        }
        
        if (GUILayout.Button("Set Pos A")) {
            myBuzzsaw.Debug_SetPosA();
        }
        if (GUILayout.Button("Set Pos B")) {
            myBuzzsaw.Debug_SetPosB();
        }
    }
}
