using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DispGroundTiler : MonoBehaviour {
    // Components
    [SerializeField] private DispGround myGround=null;

    public void Initialize() {
        UpdateTiling();
    }

    private void UpdateTiling() {
        Vector2 size = myGround.BodySprite.size;
        myGround.sr_Stroke.size = size;
        myGround.MyCollider.size = size;
    }

#if UNITY_EDITOR
    void Update() {
        UpdateTiling();
    }
#endif
}
