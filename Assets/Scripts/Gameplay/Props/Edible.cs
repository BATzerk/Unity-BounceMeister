using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Edible : Prop {
    // Components
    [SerializeField] protected BoxCollider2D myCollider=null;
    [SerializeField] protected ParticleSystem ps_collectedBurst=null;
    [SerializeField] protected SpriteRenderer sr_body=null;
    [SerializeField] protected GameObject go_body=null;
    // References
    protected Player playerHoldingMe=null; // Player's gotta land on safe ground before they can eat me.
    // Properties
    protected bool isEaten=false;
    protected bool wasEverEaten=false; // Snacks that've been eaten in the past show up as ghosts.
    protected int myIndex; // used to save/load who was eaten.

    // Getters (Private)
    protected float bodyRotation {
        get { return go_body.transform.localEulerAngles.z; }
        set { go_body.transform.localEulerAngles = new Vector3(0, 0, value); }
    }
    protected Vector2 bodyPos {
        get { return go_body.transform.localPosition; }
        set { go_body.transform.localPosition = value; }
    }
    

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D otherCol) {
        // Player??
        Player player = otherCol.GetComponent<Player>();
        if (player != null) {//LayerMask.LayerToName(otherCol.gameObject.layer) == Layers.Player) {
            player.OnTouchEdible(this);
        }
    }
    public void OnPlayerPickMeUp(Player player) {
        playerHoldingMe = player;
        //// Oh, disable my gridSnap script so it doesn't interfere with our positioning.
        //GridSnapPos snapScript = GetComponent<GridSnapPos>();
        //snapScript.enabled = false;
    }
    
    
    
    virtual public void GetEaten() {
        // Update values!
        isEaten = true;
        wasEverEaten = true;
        // Visuals!
        bodyRotation = 0;
        myCollider.enabled = false;
        sr_body.enabled = false;
        playerHoldingMe = null;
    }
    
    
    
    
}
