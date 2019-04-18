using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableGround : BaseGround {
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
    private bool isOn;
    private Color bodyColor; // depends on my properties, ya hear?
	// References
    private Coroutine c_planTurnOn; // if I regen, this is the coroutine that'll make me turn on again.
	private Player playerTouchingMe;

    // Getters (Public)
    public SpriteRenderer BodySprite { get { return bodySprite; } }
    public SpriteRenderer sr_Stroke { get { return sr_stroke; } }
    static public Color GetBodyColor(DamageableGroundData data) {
        if (data.dieFromBounce) { return ColorUtils.HexToColor("468EBA"); }
        else if (data.dieFromPlayerLeave) { return ColorUtils.HexToColor("8F6BA4"); }
        else if (data.dieFromVel) { return ColorUtils.HexToColor("886611"); }
        return Color.magenta; // Hmm.
    }


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, DamageableGroundData data) {
		base.BaseGroundInitialize(_myRoom, data);

		doRegen = data.doRegen;
		dieFromBounce = data.dieFromBounce;
		dieFromPlayerLeave = data.dieFromPlayerLeave;
        dieFromVel = data.dieFromVel;

        // Color me impressed!
        bodyColor = GetBodyColor(data);
        bodySprite.color = bodyColor;
        sr_stroke.enabled = doRegen;
        sr_stroke.color = Color.Lerp(bodyColor, Color.black, 0.7f);

        // Init my tiler.
        tiler.Initialize();

        // Start on.
        SetIsOn(true);
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
                    if (Mathf.Abs(character.vel.x) > BreakVel) {
                        TurnOff();
                    }
                }
                // Top or Bottom sides
                else if (charSide==Sides.B || charSide==Sides.T) {
                    if (Mathf.Abs(character.vel.y) > BreakVel) {
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


    private PlatformCharacter charInMyTrigger; // ONLY not null when we're off and about to regen.
    private void OnTriggerEnter2D(Collider2D col) {
        PlatformCharacter character = col.gameObject.GetComponent<PlatformCharacter>();
        if (character != null) {
            charInMyTrigger = character;
        }
    }
    private void OnTriggerExit2D(Collider2D col) {
        PlatformCharacter character = col.gameObject.GetComponent<PlatformCharacter>();
        if (character != null) {
            charInMyTrigger = null;
        }
    }




    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void TurnOff() {
        SetIsOn(false);
		if (doRegen) {
            PlanTurnOn();
		}
	}
    private void PlanTurnOn() {
        CancelPlanTurnOn();
        c_planTurnOn = StartCoroutine(Coroutine_PlanTurnOn());
    }

    private void CancelPlanTurnOn() {
        if (c_planTurnOn != null) { StopCoroutine(c_planTurnOn); }
    }
    private IEnumerator Coroutine_PlanTurnOn() {
        yield return new WaitForSeconds(RegenTime);

        // There's a character touching me??...
        if (charInMyTrigger != null) {
            // Wait for them to leave.
            while (charInMyTrigger != null) {
                // Oscillate alpha.
                float alpha = MathUtils.SinRange(0.4f, 0.45f, Time.time*16f);
                GameUtils.SetSpriteAlpha(bodySprite,alpha);
                yield return null;
            }
        }

        // Ok, we're good to turn on! Do!
		SetIsOn(true);
        yield return null;
    }

	private void SetIsOn(bool _isOn) {
        // Stop any coroutine if it's going.
        CancelPlanTurnOn();

        isOn = _isOn;
        // Update collider.
		if (myCollider!=null) { myCollider.isTrigger = !isOn; }
        // Update body alpha.
        float bodyAlpha;
        if (isOn) { bodyAlpha = 1; }
        else if (doRegen) { bodyAlpha = 0.09f; }
        else { bodyAlpha = 0; }
        GameUtils.SetSpriteAlpha(bodySprite,bodyAlpha);
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update() {
		if (isOn) {
            if (dieFromPlayerLeave && playerTouchingMe != null) {
			    float alpha = MathUtils.SinRange(0.45f,0.8f, Time.time*10f);
			    GameUtils.SetSpriteAlpha (bodySprite, alpha);
		    }
        }
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        DamageableGroundData data = new DamageableGroundData {
            myRect = MyRect(),
            mayPlayerEat = MayPlayerEatHere,
            isPlayerRespawn = IsPlayerRespawn,
            doRegen = doRegen,
            dieFromBounce = dieFromBounce,
            dieFromPlayerLeave = dieFromPlayerLeave,
            dieFromVel = dieFromVel
        };
        return data;
	}



}
