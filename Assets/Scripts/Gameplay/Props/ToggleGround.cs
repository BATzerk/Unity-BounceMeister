using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ToggleGround : BaseGround {
    // Components
    private TogGroundBehavior_Base togBehavior; // added in Initialize!
	// Properties
	[SerializeField] private bool startsOn=false;
    [SerializeField] private bool togFromAction;
    [SerializeField] private bool togFromContact;
	private bool isOn;
    private bool isPlayerInMe=false;
    private bool isWaitingToTurnOn; // set to TRUE if we wanna turn on, but a Player's in me! In this case, we'll turn on, but not apply it until the Player's left me.
	private Color bodyColorOn, bodyColorOff;


	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	override protected void Start () {
		base.Start();

		bodyColorOn = startsOn ? new Color(3/255f, 170/255f, 204/255f) : new Color(217/255f, 74/255f, 136/255f);
		bodyColorOff = new Color(bodyColorOn.r,bodyColorOn.g,bodyColorOn.b, bodyColorOn.a*0.14f);

		SetIsOn(startsOn);
	}
	public void Initialize(Room _myRoom, ToggleGroundData data) {
		base.BaseGroundInitialize(_myRoom, data);

		startsOn = data.startsOn;
        togFromAction = data.togFromAction;
        togFromContact = data.togFromContact;
        if (togFromContact) {
            togBehavior = gameObject.AddComponent<TogGroundBehavior_Contact>();
        }
        else if (togFromAction) {
            togBehavior = gameObject.AddComponent<TogGroundBehavior_Plunge>();
        }
        else {
            Debug.LogError("Whoa, not sure what TogGroundBehavior to add to ToggleGround!");
        }
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
    override public void OnCharacterLeaveMe(int charSide, PlatformCharacter character) {
        togBehavior.OnCharacterLeaveMe(charSide, character);
    }
    private void OnTriggerEnter2D(Collider2D col) {
        if (LayerMask.LayerToName(col.gameObject.layer) == Layers.Player) {
            isPlayerInMe = true;
        }
    }
    private void OnTriggerExit2D(Collider2D col) {
        if (LayerMask.LayerToName(col.gameObject.layer) == Layers.Player) {
            isPlayerInMe = false;
            if (isWaitingToTurnOn) { // I'm waiting to turn on? Do!
                ApplyIsOn();
            }
        }
    }



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
    public void ToggleIsOn() {
        SetIsOn (!isOn);
    }
	public void SetIsOn(bool _isOn) {
		isOn = _isOn;

        isWaitingToTurnOn = _isOn && isPlayerInMe;
        if (!isWaitingToTurnOn) { // I can turn on right away! So do it!
            ApplyIsOn();
        }
	}

	private void ApplyIsOn() {
		myCollider.isTrigger = !isOn;
		bodySprite.color = isOn ? bodyColorOn : bodyColorOff;

		// TEMP fragile solution: If I have any children, totally also enable/disable their colliders and sprites!
		if (this.transform.childCount > 0) {
			Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
			SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
			foreach (Collider2D col in childColliders) {
				col.enabled = isOn;
			}
			foreach (SpriteRenderer sprite in childSprites) {
				GameUtils.SetSpriteAlpha(sprite, isOn ? 1f : 0.14f);
			}
		}
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData ToData() {
        ToggleGroundData data = new ToggleGroundData {
            pos = pos,
            size = Size(),
            mayPlayerEat = MayPlayerEatHere,
            isPlayerRespawn = IsPlayerRespawn,
            startsOn = startsOn,
            togFromContact = togFromContact,//togBehavior is TogGroundBehavior_Contact;
            togFromAction = togFromAction,//togBehavior is TogGroundBehavior_Plunge;
            travelMind = new TravelMindData(travelMind),
        };
        return data;
	}

}


