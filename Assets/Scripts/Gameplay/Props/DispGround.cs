﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispGround : BaseGround {
    // Constants
    public const float RegenTimeDefault = 2.2f;
    const float BreakVel = 0.6f; // how hard Player must hit me for me to break.
    // Components
    [SerializeField] public BoxCollider2D MyCollider=null;
    [SerializeField] private SpriteRenderer sr_stroke=null;
    [SerializeField] private DispGroundTiler tiler=null; // this guy correctly updates my sprite and collider tilings!
    // Properties
	[SerializeField] private bool doRegen = true; // if TRUE, I'll come back after a moment!
    [SerializeField] private bool dieFromBounce = false;
    [SerializeField] private bool dieFromPlayerLeave = true;
    [SerializeField] private bool dieFromVel = true; // NOTE: Not used in gameplay much.
    [SerializeField] private float regenTime = RegenTimeDefault; // how long it takes for me to regen after I've disappeared.
    private bool isOn;
    private Color bodyColor; // depends on my properties, ya hear?
	// References
    [SerializeField] private Sprite s_strokeDashed=null;
    [SerializeField] private Sprite s_strokeSolid=null;
    private Coroutine c_planTurnOn; // if I regen, this is the coroutine that'll make me turn on again.
	private Player playerTouchingMe;

    // Getters (Public)
    public bool DieFromPlayerLeave { get { return dieFromPlayerLeave; } }
    public SpriteRenderer BodySprite { get { return bodySprite; } }
    public SpriteRenderer sr_Stroke { get { return sr_stroke; } }
    static public Color GetBodyColor(DispGroundData data) {
        if (data.dieFromBounce) { return ColorUtils.HexToColor("468EBA"); }
        else if (data.dieFromPlayerLeave) {
            if (data.doRegen) { return ColorUtils.HexToColor("8F6BA4"); }
            else { return ColorUtils.HexToColor("7884BA"); }
        }
        else if (data.dieFromVel) { return ColorUtils.HexToColor("886611"); }
        return Color.magenta; // Hmm.
    }


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, DispGroundData data) {
		base.BaseGroundInitialize(_myRoom, data);

		doRegen = data.doRegen;
        regenTime = data.regenTime;
		dieFromBounce = data.dieFromBounce;
		dieFromPlayerLeave = data.dieFromPlayerLeave;
        dieFromVel = data.dieFromVel;

        // Color me impressed!
        bodyColor = GetBodyColor(data);
        bodySprite.color = bodyColor;
        sr_stroke.sprite = data.doRegen ? s_strokeDashed : s_strokeSolid;
        sr_stroke.color = Color.Lerp(bodyColor, Color.black, 0.7f);

        // Init my tiler.
        tiler.Initialize();

        // Start on.
        SetIsOn(true);
    }
    protected override void OnCreatedInEditor() {
        base.OnCreatedInEditor();
        // Do standard paperwork so I can look good right away.
        DispGroundData data = ToData() as DispGroundData;
        Initialize(MyRoom, data);
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    override public void OnPlayerFeetBounceOnMe(Player player) {
		if (dieFromBounce) {
			TurnOff();
		}
	}
	override public void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
        if (GameTimeController.IsRoomFrozen) { return; } // Room's frozen? Ignore character touch!
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
        // Room's frozen, AND there was no Player registered touching me? Do nothin'!
        if (GameTimeController.IsRoomFrozen && playerTouchingMe==null) {
            return;
        }
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
        float timeUntilReady = regenTime;
        
        while (timeUntilReady > 0) {
            // Countdown.
            timeUntilReady -= GameTimeController.RoomDeltaTime;
            // Flash stroke for last half-second!
            if (timeUntilReady < 0.5f) {
                float strokeAlpha = MathUtils.SinRange(1,0.35f, timeUntilReady*40);
                GameUtils.SetSpriteAlpha(sr_stroke, strokeAlpha);
            }
            yield return null;
        }
        
        // ... We're ready to turn on now!
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
        // Update body/stroke alphas.
        float bodyAlpha;
        float strokeAlpha;
        if (doRegen) { // I DO regen.
            if (isOn) {
                bodyAlpha = 1;
                strokeAlpha = 1;
            }
            else {
                bodyAlpha = 0;
                strokeAlpha = 1f;
            }
        }
        else { // I DON'T regen.
            if (isOn) {
                bodyAlpha = 1;
                strokeAlpha = 0.5f;
            }
            else {
                bodyAlpha = 0;
                strokeAlpha = 0.05f;
            }
        }
        GameUtils.SetSpriteAlpha(bodySprite, bodyAlpha);
        GameUtils.SetSpriteAlpha(sr_stroke, strokeAlpha);
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update() {
		if (isOn) {
            if (dieFromPlayerLeave && playerTouchingMe != null) {
			    float alpha = MathUtils.SinRange(0.45f,0.8f, MyRoom.RoomTime*10f);
			    GameUtils.SetSpriteAlpha (bodySprite, alpha);
		    }
        }
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData ToData() {
        DispGroundData data = new DispGroundData {
            pos = pos,
            size = Size(),
            mayPlayerEat = MayPlayerEatHere,
            isPlayerRespawn = IsPlayerRespawn,
            doRegen = doRegen,
            regenTime = regenTime,
            dieFromBounce = dieFromBounce,
            dieFromPlayerLeave = dieFromPlayerLeave,
            dieFromVel = dieFromVel,
            travelMind = new TravelMindData(travelMind),
        };
        return data;
	}



}
