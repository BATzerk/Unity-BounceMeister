using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class Spikes : Collidable {
	// Components
	[SerializeField] private SpriteRenderer bodySprite=null;

	// Getters
	private Rect MyRect {
		get {
			Vector2 center = bodySprite.transform.localPosition;
			Vector2 size = bodySprite.size;
			return new Rect(center, size);
		}
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, SpikesData data) {
		base.BaseInitialize(_myRoom, data);

		bodySprite.size = data.myRect.size;
		bodySprite.transform.localPosition = data.myRect.position;
        bodySprite.color = Colors.Spikes(WorldIndex);
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
    //  Debug
    // ----------------------------------------------------------------
    public void Debug_Rotate(float delta) {
        rotation = Mathf.Round(rotation + delta);
    }


	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        SpikesData data = new SpikesData {
            myRect = MyRect,
            rotation = rotation
        };
        return data;
	}
}
