using System.Collections;
using System.Collections.Generic;
using UnityEngine;


abstract public class TogGroundBehavior_Base : MonoBehaviour {
    // Components
    protected ToggleGround myTogGround; // Set in Awake.
    
    // Awake!
    private void Awake() {
        myTogGround = GetComponent<ToggleGround>();
    }
    // Abstract Functions
    virtual public void OnCharacterLeaveMe(int charSide, PlatformCharacter character) {}
    virtual protected void OnTriggerExit2D(Collider2D col) {}
    // Doers
    protected void SetIsOn(bool isOn) { myTogGround.SetIsOn(isOn); }
    protected void ToggleIsOn() { myTogGround.ToggleIsOn(); }
}




public class TogGroundBehavior_Contact : TogGroundBehavior_Base {
    override public void OnCharacterLeaveMe(int charSide, PlatformCharacter character) {
        if (character is Player) {
            SetIsOn(false);
        }
    }
    override protected void OnTriggerExit2D(Collider2D col) {
        PlatformCharacter character = col.gameObject.GetComponent<PlatformCharacter>();
        if (character != null) {
            SetIsOn(true);
        }
    }
}


public class TogGroundBehavior_Plunge : TogGroundBehavior_Base {
    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Start() {
        // Add event listeners!
        GameManagers.Instance.EventManager.PlayerStartHoverEvent += OnPlayerAction;
        GameManagers.Instance.EventManager.PlayerStartJetEvent += OnPlayerAction;
        GameManagers.Instance.EventManager.PlayerStartPlungeEvent += OnPlayerAction;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.PlayerStartHoverEvent -= OnPlayerAction;
        GameManagers.Instance.EventManager.PlayerStartJetEvent -= OnPlayerAction;
        GameManagers.Instance.EventManager.PlayerStartPlungeEvent -= OnPlayerAction;
    }

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnPlayerAction(Player player) {
        ToggleIsOn();
    }
}

