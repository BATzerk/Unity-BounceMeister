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
    public Vector2 Size() {
        if (bodySprite.drawMode == SpriteDrawMode.Simple) { // Simple draw mode? Ok, use my SCALE.
            return bodySprite.transform.localScale;
        }
        else { // Tiled draw mode? Ok, use the SPRITE SIZE.
            return bodySprite.size;
        }
    }
    protected void SetSize(Vector2 _size) {
        if (bodySprite.drawMode == SpriteDrawMode.Simple) { // Simple draw mode? Ok, use my SCALE.
            bodySprite.transform.localScale = _size;
        }
        else { // Tiled draw mode? Ok, use the SPRITE SIZE.
            bodySprite.size = _size;
        }
    }
    
    public Rect GetMyRect() { return new Rect(pos, Size()); }
    /// Returns bottom-left aligned Rect.
    public Rect GetMyRectBL() {
        Rect r = GetMyRect();
        r.center = r.position;
        return r;
    }
    //protected void SetMyRect(Rect r) {
    //    bodySprite.transform.localPosition = r.position;
    //}


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	//override protected void Start() {
 //       base.Start();
	//	// TEMP! For room transitioning.
	//	if (bodySprite==null) {
	//		bodySprite = GetComponent<SpriteRenderer>();
	//	}
	//}
	protected void BaseGroundInitialize(Room _myRoom, BaseGroundData data) {
		base.BaseInitialize(_myRoom, data);

		mayPlayerEat = data.mayPlayerEat;
        isPlayerRespawn = data.isPlayerRespawn;
        SetSize(data.size);
	}




}