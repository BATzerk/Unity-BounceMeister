using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[ExecuteInEditMode]
public class DamageableGroundTiler : MonoBehaviour {
    // Components
    [SerializeField] private DamageableGround myGround=null;

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
