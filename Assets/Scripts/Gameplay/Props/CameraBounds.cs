using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class CameraBounds : Prop {
    // Overrides
    public override bool DoSaveInRoomFile() { return false; } // Note: CameraBounds are saved differently from other Props, as there's always only one.
    // Components
    [SerializeField] private SpriteRenderer bodySprite=null;

	// Getters (Private)
    public Vector2 Size {
        get { return bodySprite.size; }
        private set { bodySprite.size = value; }
    }
	//public Rect RectLocal { // Note that we save/load our Rect locally, not globally. (Otherwise we'd be wonked when rooms are moved.)
	//	get {
	//		Rect newRect = new Rect();
	//		newRect.size = bodySprite.size;
	//		newRect.center = bodySprite.transform.localPosition;
	//		return newRect;
	//	}
	//}



	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
        base.Start();
		// Of course, hide the sprite! It's just for the editor.
		bodySprite.enabled = false;
	}
	public void Initialize(Room _myRoom, CameraBoundsData data) {
		base.BaseInitialize(_myRoom, data);
		bodySprite.size = data.size;
	}


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	override public PropData ToData() {
        CameraBoundsData data = new CameraBoundsData {
            pos = pos,
            size = Size,
        };
        return data;
	}


}
