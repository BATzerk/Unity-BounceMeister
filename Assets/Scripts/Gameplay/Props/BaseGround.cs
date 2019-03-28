using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
abstract public class BaseGround : Collidable {
	// Properties
	[SerializeField] private bool canEatGems = true; // teechnically, it's "Can Player eat Gems while on me?" If false, Player WON'T collect the Gem they're holding when they land on me!
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!
	// Components
	[SerializeField] protected SpriteRenderer bodySprite=null;
	[SerializeField] protected BoxCollider2D myCollider=null;

	// Getters (Public)
	public bool CanEatGems { get { return canEatGems; } }
	// Getters (Private)
	protected Rect MyRect {
		get {
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
	}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	virtual protected void Start() {
		// TEMP! For level transitioning.
		if (bodySprite==null) {
			bodySprite = GetComponent<SpriteRenderer>();
		}
	}
	protected void BaseGroundInitialize(Level _myLevel, BaseGroundData data) {
		base.BaseInitialize(_myLevel, data);

		canEatGems = data.canEatGems;

		if (bodySprite.drawMode == SpriteDrawMode.Simple) { // Simple draw mode? Ok, use my SCALE.
			bodySprite.transform.localScale = data.myRect.size;
		}
		else { // Tiled draw mode? Ok, use the SPRITE SIZE.
			bodySprite.size = data.myRect.size;
		}
		bodySprite.transform.localPosition = data.myRect.position;
	}




}