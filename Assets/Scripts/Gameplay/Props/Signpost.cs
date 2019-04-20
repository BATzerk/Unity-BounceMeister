using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Signpost : Prop {
    // Components
    //[SerializeField] private SpriteRenderer sr_body=null;
    // Properties
    [SerializeField] private string myText;
    
    // Getters (Public)
    public string MyText { get { return myText; } }


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, SignpostData data) {
        base.BaseInitialize(_myRoom, data);

        myText = data.myText;
    }


    // ----------------------------------------------------------------
    //  Physics Events
    // ----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D otherCol) {
        Player player = otherCol.GetComponent<Player>();
        if (player != null) {
            OnPlayerTouchEnter();
        }
    }
    private void OnTriggerExit2D(Collider2D otherCol) {
        Player player = otherCol.GetComponent<Player>();
        if (player != null) {
            OnPlayerTouchExit();
        }
    }
    private void OnPlayerTouchEnter() {
        GameManagers.Instance.EventManager.OnPlayerTouchEnterSignpost(this);
    }
    private void OnPlayerTouchExit() {
        GameManagers.Instance.EventManager.OnPlayerTouchExitSignpost(this);
    }
    

    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        SignpostData data = new SignpostData {
            pos = pos,
            myText = myText,
        };
        return data;
    }

}
