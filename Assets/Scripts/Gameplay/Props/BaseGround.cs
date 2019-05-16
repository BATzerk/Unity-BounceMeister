using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
abstract public class BaseGround : Collidable {
	// Properties
	[SerializeField] private bool mayPlayerEat = true; // teechnically, it's "May Player eat Edibles while on me?" If false, Player WON'T collect the Edible they're holding when they land on me!
    [SerializeField] private bool isPlayerRespawn = false; // if TRUE, then Player will set GroundedRespawnPos when they leave me! When Player dies, they'll respawn at that pos.
//	[SerializeField] private bool doDisappearAfterBounces = false;
//	[SerializeField] private int numBouncesLeft = -1; // exhaustable!
	// Components
	[SerializeField] protected SpriteRenderer bodySprite=null;
	[SerializeField] protected BoxCollider2D myCollider=null;

    // Getters (Public)
    public bool MayPlayerEatHere { get { return mayPlayerEat; } }
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
    /// Returns bottom-left aligned Rect.
    protected Rect MyRectBL() {
        Rect r = MyRect();
        r.center = r.position;
        return r;
    }
    protected void SetMyRect(Rect r) {
        if (bodySprite.drawMode == SpriteDrawMode.Simple) { // Simple draw mode? Ok, use my SCALE.
            bodySprite.transform.localScale = r.size;
        }
        else { // Tiled draw mode? Ok, use the SPRITE SIZE.
            bodySprite.size = r.size;
        }
        bodySprite.transform.localPosition = r.position;
    }


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	override protected void Start() {
        base.Start();
		// TEMP! For room transitioning.
		if (bodySprite==null) {
			bodySprite = GetComponent<SpriteRenderer>();
		}
	}
	protected void BaseGroundInitialize(Room _myRoom, BaseGroundData data) {
		base.BaseInitialize(_myRoom, data);

		mayPlayerEat = data.mayPlayerEat;
        isPlayerRespawn = data.isPlayerRespawn;
        
        SetMyRect(data.myRect);
	}




}