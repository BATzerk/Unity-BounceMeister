using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharUnlockOrb : Prop {
    // Components
    [SerializeField] private Collider2D myCollider=null;
    [SerializeField] private SpriteRenderer sr_body=null;
    // Properties
    [SerializeField] private string myCharName = "Plunga";
    public PlayerTypes MyPlayerType { get; private set; } // set from myCharName.
    //private int myIndex; // which CharUnlockOrb I am in Room.
    
    // Getters (Private)
    private CharLineup lineup { get { return GameManagers.Instance.DataManager.CharLineup; } }
    
    // Serializing
    override public PropData SerializeAsData() {
        CharUnlockOrbData data = new CharUnlockOrbData {
            pos = PosLocal,
            myCharName = myCharName,
        };
        return data;
    }

    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, CharUnlockOrbData data) {
        base.BaseInitialize(_myRoom, data);

        this.myCharName = data.myCharName;
        MyPlayerType = PlayerTypeHelper.TypeFromString(myCharName);

        // Color my body!
        Color color = PlayerBody.GetBodyColorNeutral(MyPlayerType);
        sr_body.color = Color.Lerp(color,Color.black, 0.2f); // darken it slightly.
        
        UpdateUnlockedVisuals();
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void UpdateUnlockedVisuals() {
        bool didUnlockType = lineup.Lineup.Contains(MyPlayerType);
        myCollider.enabled = !didUnlockType;
        if (didUnlockType) {
            GameUtils.SetSpriteAlpha(sr_body, 0.1f);
        }
    }
    //private void UnlockMyChar() {
    //    lineup.AddPlayerType(myPlayerType);
    //    UpdateUnlockedVisuals();
    //}


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D col) {
        Player player = col.gameObject.GetComponent<Player>();
        if (player != null) {
            MyRoom.GameController.OnPlayerTouchCharUnlockOrb(this);
        }
    }



}


