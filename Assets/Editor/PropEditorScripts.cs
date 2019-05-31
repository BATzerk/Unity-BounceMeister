using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//public class BasePropEditor : Editor {
//    private void OnSceneGUI() {
//        if (Event.current.modifiers == EventModifiers.Control) {
//            Debug.Log("Control");
//        }
//    }
//}

[CustomEditor(typeof(Buzzsaw))]
public class BuzzsawEditor : Editor {
    // References
    private Buzzsaw myProp;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (myProp == null) { myProp = (Buzzsaw)target; }
        if (!myProp.HasTravelMind()) {
            if (GUILayout.Button("Add TravelMind")) {
                myProp.AddTravelMind(TravelMindData.Default);
            }
        }
        else {
            if (GUILayout.Button("Remove TravelMind")) {
                myProp.RemoveTravelMind();
            }
        }
    }
}


[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor {
    // References
    private Ground myProp;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (myProp == null) { myProp = (Ground)target; }
        if (!myProp.HasTravelMind()) {
            if (GUILayout.Button("Add TravelMind")) {
                myProp.AddTravelMind(TravelMindData.Default);
            }
        }
        else {
            if (GUILayout.Button("Remove TravelMind")) {
                myProp.RemoveTravelMind();
            }
        }
    }
}


[CustomEditor(typeof(Laser))]
public class LaserEditor : Editor {
    // References
    private Laser myLaser;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (myLaser == null) { myLaser = (Laser)target; }
        //if (GUILayout.Button("Rotate 90°")) { myLaser.Debug_RotateCW(); }
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


[CustomEditor(typeof(PropTravelMind))]
public class PropTravelMindEditor : Editor {
    // References
    private PropTravelMind myTravelMind;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (myTravelMind == null) { myTravelMind = (PropTravelMind)target; }
        if (GUILayout.Button("Set Pos A")) {
            myTravelMind.Debug_SetPosA();
        }
        if (GUILayout.Button("Set Pos B")) {
            myTravelMind.Debug_SetPosB();
        }
    }
}


[CustomEditor(typeof(Spikes))]
public class SpikesEditor : Editor {
    // References
    private Spikes mySpikes;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (mySpikes == null) { mySpikes = (Spikes)target; }
        
        //if (GUILayout.Button("Rotate 90°")) { mySpikes.Debug_RotateCW(); }
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


//[CustomEditor(typeof(Turret))]
//public class TurretEditor : Editor {
//    // References
//    private Turret myProp;
//    public override void OnInspectorGUI() {
//        base.OnInspectorGUI();
//        if (myProp == null) { myProp = (Turret)target; }
//        if (GUILayout.Button("Rotate 90°")) { myProp.Debug_RotateCW(); }
//    }
//}
