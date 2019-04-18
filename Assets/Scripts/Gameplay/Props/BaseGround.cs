using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
abstract public class BaseGround : Collidable {
	// Properties
	[SerializeField] private bool canEatGems = true; // teechnically, it's "Can Player eat Gems while on me?" If false, Player WON'T collect the Gem they're holding when they land on me!
    [SerializeField] private bool isPlayerRespawn = false; // if TRUE, then Player will set GroundedRespawnPos when they leave me! When Player dies, they'll respawn at that pos.
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!
	// Components
	[SerializeField] protected SpriteRenderer bodySprite=null;
	[SerializeField] protected BoxCollider2D myCollider=null;

    // Getters (Public)
    public bool CanEatEdibles { get { return canEatGems; } }//TODO: Rename to MayPlayerEat! In saves and stuff!
    public bool IsPlayerRespawn { get { return isPlayerRespawn; } }
    // Getters (Private)
    public Rect MyRect() {
		Vector2 center = bodySprite.transform.localPosition;
		Vector2 size;
		if (bodySprite.drawMode == SpriteDrawMode.Simple) { // Simple draw mode? Ok, use my SCALE.
			size = bodySprite.transform.localScale;
		}
		else { // Tiled draw mode? Ok, use the SPRITE SIZE.
			size = bodySprite.size;
		}
		return new Rect(center, size);
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	virtual protected void Start() {
		// TEMP! For room transitioning.
		if (bodySprite==null) {
			bodySprite = GetComponent<SpriteRenderer>();
		}
	}
	protected void BaseGroundInitialize(Room _myRoom, BaseGroundData data) {
		base.BaseInitialize(_myRoom, data);

		canEatGems = data.canEatGems;
        isPlayerRespawn = data.isPlayerRespawn;

        if (bodySprite.drawMode == SpriteDrawMode.Simple) { // Simple draw mode? Ok, use my SCALE.
			bodySprite.transform.localScale = data.myRect.size;
		}
		else { // Tiled draw mode? Ok, use the SPRITE SIZE.
			bodySprite.size = data.myRect.size;
		}
		bodySprite.transform.localPosition = data.myRect.position;
	}




}