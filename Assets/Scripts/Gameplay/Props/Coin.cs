using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Collidable {
	// Components
	[SerializeField] private Collider2D myCollider=null;
	[SerializeField] private SpriteRenderer sr_body=null;
	[SerializeField] private ParticleSystem ps_collectedBurst=null;
	// Properties
	[SerializeField] private int value = 1; // how much I'm worth.

	// Getters
	public int Value { get { return value; } }
    
    
    public override PropData ToData() {
        throw new System.NotImplementedException();
    }
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _room, Vector2 _sourcePos) {
        GameUtils.ParentAndReset(this.gameObject, _room.transform);
        transform.localPosition = _sourcePos;
        transform.localPosition += new Vector3( // Put it randomly somewhere inside my box area (instead of putting them all exactly in the center).
            Random.Range(-2, 2),
            Random.Range(-2, 2));
            
        Rigidbody2D myRB = this.GetComponent<Rigidbody2D>();
        myRB.angularVelocity = Random.Range(-5, 5);
        myRB.velocity = new Vector2(
            Random.Range(-4, 4),
            Random.Range(-2, 8));
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public override void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
        base.OnCharacterTouchMe(charSide, character);
        if (character is Player) {
            //if (charSide == Sides.B) {
                GetCollected();
            //}
        }
    }
 //   private void OnCollisionEnter2D(Collision2D col) {
	//	Player player = col.gameObject.GetComponent<Player>();
	//	if (player != null) {
 //           GetCollected();
 //           // Bounce the player, yo! TEST TEMP HACK
 //           float playerNewYVel = Mathf.Max(0, -player.vel.y);
 //           player.SetVel(player.vel.x, playerNewYVel);
	//	}
	//}
    

	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GetCollected() {
		// Disable my collider and sprite!
		myCollider.enabled = false;
		sr_body.enabled = false;
		// Particle burst!
		ps_collectedBurst.Emit(6);
		// Pump up our funds, yo!
		GameManagers.Instance.DataManager.ChangeCoinsCollected(value);
	}
}
