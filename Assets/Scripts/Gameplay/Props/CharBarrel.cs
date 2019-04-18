using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharBarrel : Prop {
    // Components
    [SerializeField] private SpriteRenderer sr_body=null;
    // Properties
    [SerializeField] private string otherCharName = "Slippa"; // this value DOESN'T change. It's the DEFAULT other guy I've got.
	private int myIndex; // which CharBarrel I am in Room.
    private float timeWhenCanSensePlayer; // in SCALED seconds. So we don't keep swapping between two Player types.
	public PlayerTypes CharTypeInMe { get; private set; }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        CharBarrelData data = new CharBarrelData {
            pos = PosLocal,
            otherCharName = otherCharName
        };
        return data;
    }

	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, CharBarrelData data, int myIndex) {
		base.BaseInitialize(_myRoom, data);

        timeWhenCanSensePlayer = Time.time;

		this.otherCharName = data.otherCharName;
        this.myIndex = myIndex;

        // Load what character's in me!
        string savedCharType = SaveStorage.GetString(SaveKeys.CharBarrelTypeInMe(myRoom.RoomDataRef, myIndex), otherCharName);
        SetCharTypeInMe(PlayerTypeHelper.TypeFromString(savedCharType));
	}



    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetCharTypeInMe(PlayerTypes _type) {
        CharTypeInMe = _type;
        // Color my body!
        Color playerColor = PlayerBody.GetBodyColorNeutral(CharTypeInMe);
        sr_body.color = Color.Lerp(playerColor,Color.black, 0.2f); // darken it slightly.
    }
	private void SwapPlayerType(Player player) {
        PlayerTypes playerNewType = CharTypeInMe;
        PlayerTypes myNewType = player.PlayerType();
        // Set Player's type!
        myRoom.SwapPlayerType(playerNewType);
        // Set/save my type!
        SetCharTypeInMe(myNewType);
        SaveStorage.SetString(SaveKeys.CharBarrelTypeInMe(myRoom.RoomDataRef, myIndex), myNewType.ToString());
        // Reset timeWhenCanSensePlayer!
        timeWhenCanSensePlayer = Time.time + 0.1f;
	}




	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D col) {
        if (Time.time < timeWhenCanSensePlayer) { return; } // I was like JUST born?? Do nothin'.
        
        bool isPlayer = LayerMask.LayerToName(col.gameObject.layer) == Layers.Player;
        if (isPlayer) {
            Player player = col.gameObject.GetComponent<Player>();
            SwapPlayerType(player);
        }
    }



}


