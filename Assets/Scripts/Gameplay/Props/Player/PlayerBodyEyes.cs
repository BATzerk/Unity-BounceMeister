using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enums
public enum EyeTypes { Undefined, Normal, Happy, Squint }


public class PlayerBodyEyes : MonoBehaviour {
    // Components
    [SerializeField] private GameObject go_eyesHappy=null; // little rainbow arcs.
    [SerializeField] private GameObject go_eyesNormal=null; // wide open. I can see the world.
    [SerializeField] private GameObject go_eyesSquint=null; // squinting in earnest consternation.
    // Properties
    //private float happy;
    //private float squint;
    //private EyeTypes currEyes;


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void Set(EyeTypes eyeType) {
        //this.currEyes = eyeType;
        go_eyesNormal.SetActive(false);
        go_eyesHappy.SetActive(false);
        go_eyesSquint.SetActive(false);
        switch (eyeType) {
            case EyeTypes.Normal: go_eyesNormal.SetActive(true); break;
            case EyeTypes.Happy: go_eyesHappy.SetActive(true); break;
            case EyeTypes.Squint: go_eyesSquint.SetActive(true); break;
            default: Debug.LogWarning("FlatlineBody EyeType not recognized: " + eyeType); break;
        }
    }
    private void TEMP_SetEyesNormal() { Set(EyeTypes.Normal); }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnEatEdiblesHolding() {
        //happy = 1;
        Set(EyeTypes.Happy);
        Invoke("TEMP_SetEyesNormal", 1.2f);
    }
    
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    //private void Update() {
        
    //}
    
    
}
