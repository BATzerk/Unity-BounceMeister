using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableGround : BaseGround, ISerializableData<DamageableGroundData> {
    // Constants
    const float RegenTime = 2.2f; // how long it takes for me to reappear after I've disappeared.
    const float BreakVel = 0.6f; // how hard Player must hit me for me to break.
    // Components
    [SerializeField] public BoxCollider2D MyCollider=null;
    [SerializeField] private SpriteRenderer sr_stroke=null;
    [SerializeField] private DamageableGroundTiler tiler=null; // this guy correctly updates my sprite and collider tilings!
    // Properties
	[SerializeField] private bool doRegen = true; // if TRUE, I'll come back after a moment!
    [SerializeField] private bool dieFromBounce = false;
    [SerializeField] private bool dieFromPlayerLeave = true;
    [SerializeField] private bool dieFromVel = true; // NOTE: Not used in gameplay much.
    private Color bodyColor; // depends on my properties, ya hear?
	// References
	//[SerializeField] private Sprite s_strokeSolid;
	//[SerializeField] private Sprite s_strokeDashed;
	private Player playerTouchingMe;

    // Getters (Public)
    public SpriteRenderer BodySprite { get { return bodySprite; } }
    public SpriteRenderer sr_Stroke { get { return sr_stroke; } }


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, DamageableGroundData data) {
		base.BaseGroundInitialize(_myLevel, data);

		doRegen = data.doRegen;
		dieFromBounce = data.dieFromBounce;
		dieFromPlayerLeave = data.dieFromPlayerLeave;
        dieFromVel = data.dieFromVel;

        // Color me impressed!
        if (dieFromBounce) { bodyColor = ColorUtils.HexToColor("468EBA"); }
        else if (dieFromPlayerLeave) { bodyColor = ColorUtils.HexToColor("8F6BA4"); }
        else if (dieFromVel) { bodyColor = ColorUtils.HexToColor("886611"); }
        bodySprite.color = bodyColor;
        sr_stroke.enabled = doRegen;
        sr_stroke.color = Color.Lerp(bodyColor, Color.black, 0.7f);

        // Init my tiler.
        tiler.Initialize();
    }

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    override public void OnPlayerBounceOnMe(Player player) {
		if (dieFromBounce) {
			TurnOff();
		}
	}

	override public void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
        //if (charSide != Sides.B && MyRect.size.y==1) { return; } // Currently, we ONLY care about FEET if we're thin. Kinda a test.
        if (character is Player) {
            playerTouchingMe = character as Player;
            if (dieFromVel) {
                // Left or Right sides
                if (charSide==Sides.L || charSide==Sides.R) {
                    if (Mathf.Abs(character.Vel.x) > BreakVel) {
                        TurnOff();
                    }
                }
                // Top or Bottom sides
                else if (charSide==Sides.B || charSide==Sides.T) {
                    if (Mathf.Abs(character.Vel.y) > BreakVel) {
                        TurnOff();
                    }
                }
            }
        }
    }
	override public void OnCharacterLeaveMe(int charSide, PlatformCharacter character) {
		//if (charSide != Sides.B && MyRect.size.y==1) { return; } // Currently, we ONLY care about FEET if we're thin. Kinda a test.
		if (dieFromPlayerLeave && character is Player) {
			TurnOff();
			playerTouchingMe = null;
		}
	}

	// Kinda hacked in for now.
	private void TurnOff() {
		SetIsOn(false);
		if (doRegen) {
			Invoke("TurnOn", RegenTime);
		}
	}
	private void TurnOn() {
		SetIsOn(true);
	}
	private void SetIsOn(bool _isOn) {
		GameUtils.SetSpriteAlpha (bodySprite, 1); // Always reset my alpha here.
		if (myCollider!=null) { myCollider.enabled = _isOn; }
		if (bodySprite!=null) {
			bodySprite.enabled = _isOn;
//			if (doRegen) {
////				GameUtils.SetSpriteAlpha (bodySprite, _isOn ? 1f : 0.15f);
//			}
//			else {
//				bodySprite.enabled = _isOn;
//			}
		}
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update() {
		if (dieFromPlayerLeave && playerTouchingMe != null) {
			float alpha = 0.6f + Mathf.Sin(Time.time*20f)*0.3f;
			GameUtils.SetSpriteAlpha (bodySprite, alpha);
		}
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public DamageableGroundData SerializeAsData() {
		DamageableGroundData data = new DamageableGroundData();
		data.myRect = MyRect;
		data.canEatGems = CanEatGems;
		data.doRegen = doRegen;
		data.dieFromBounce = dieFromBounce;
        data.dieFromPlayerLeave = dieFromPlayerLeave;
        data.dieFromVel = dieFromVel;
        return data;
	}



}
