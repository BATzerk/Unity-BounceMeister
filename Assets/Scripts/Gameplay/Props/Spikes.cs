﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class Spikes : Collidable, IOnOffable {
	// Components
    [SerializeField] private Collider2D myCollider=null;
    [SerializeField] private SpriteRenderer bodySprite=null;

	// Getters
	//private Rect MyRect {
	//	get {
	//		Vector2 center = bodySprite.transform.localPosition;
	//		Vector2 size = bodySprite.size;
	//		return new Rect(center, size);
	//	}
	//}
    private Vector2 Size {
        get { return bodySprite.size; }
        set { bodySprite.size = value; }
    }
    
    // OnOffer Stuff
    private bool isOn;
    private PropOnOffer onOffer; // added in Initialize.
    public bool IsOn() { return isOn; }
    public bool HasOnOffer() { return onOffer != null; }
    public void AddOnOffer(OnOfferData data) {
        if (onOffer != null) { return; } // Safety check.
        onOffer = gameObject.AddComponent<PropOnOffer>();
        onOffer.Initialize(this, data);
    }
    public void RemoveOnOffer() {
        if (onOffer == null) { return; } // Safety check.
        Destroy(onOffer);
        onOffer = null;
        SetIsOn(true);
    }
    public void UpdateAlmostOn(float timeUntilOn) {
        //float alpha = MathUtils.SinRange(0.08f, 0.14f, timeUntilOn*40);
        float alpha = 0.3f;
        GameUtils.SetSpriteAlpha(bodySprite, alpha);
    }
    public void SetIsOn(bool _isOn) {
        isOn = _isOn;
        myCollider.enabled = isOn;
        LeanTween.cancel(bodySprite.gameObject);
        if (isOn) {
            GameUtils.SetSpriteAlpha(bodySprite, 1);
        }
        else {
            LeanTween.alpha(bodySprite.gameObject, 0.1f, 0.14f).setEaseOutQuad(); // fade out quickly.
        }
    }
    override protected void OnCreatedInEditor() {
        base.OnCreatedInEditor();
        if (onOffer == null) { onOffer = GetComponent<PropOnOffer>(); } // Safety check for duplicating objects.
    }


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, SpikesData data) {
		base.InitializeAsProp(_myRoom, data);
        Size = data.size;
        bodySprite.color = Colors.Spikes(WorldIndex);
        
        if (data.onOffer.durOff > 0) { AddOnOffer(data.onOffer); }
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
    // TODO: Clean this up! Clean up how player touching harm is handled.
	override public void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
		Player player = character as Player;
		if (player != null) {
			player.OnTouchHarm();
		}
	}
    

    // ----------------------------------------------------------------
    //  Editing
    // ----------------------------------------------------------------
    public override void FlipVert() {
        pos = new Vector2(pos.x, -pos.y);
        rotation += 180;
    }


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData ToData() {
        SpikesData data = new SpikesData {
            pos = pos,
            size = Size,
            rotation = rotation,
            onOffer = new OnOfferData(onOffer),
            travelMind = new TravelMindData(travelMind),
        };
        return data;
	}
}
