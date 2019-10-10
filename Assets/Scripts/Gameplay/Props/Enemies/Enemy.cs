using System.Collections;
using System.Collections.Generic;
using UnityEngine;


abstract public class Enemy : PlatformCharacter {
	// Constants
    virtual protected int NumCoinsInMe { get { return 3; } }
    override protected int StartingHealth { get { return 1; } }
	override protected float FrictionAir { get { return 0.6f; } }
	override protected float FrictionGround { get { return 0.6f; } }
	override protected Vector2 Gravity { get { return new Vector2(0, -0.05f); } }
    // Components
    [SerializeField] protected SpriteRenderer sr_body=null;
    // References
    [SerializeField] private Sprite s_bodyDead=null;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
    public void Initialize(Room _myRoom, PropData data) {
        base.BaseInitialize(_myRoom, data);
    }


	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate () {
        if (!DoUpdate()) { return; }
        if (GameTimeController.IsRoomFrozen) { return; } // Time's frozen? Register NOTHING! Raor!
        if (IsDead) { return; } // Dead? Do nothin'.
        
        Vector2 ppos = pos;

        ApplyVelFromSurfaces();
        ApplyFriction();
		ApplyGravity();
		AcceptDirectionalMoveInput();
        ApplyTerminalVel();
        ApplyLiftForces();
		myWhiskers.UpdateSurfaces(); // update these dependently now, so we guarantee most up-to-date info.
		ApplyVel();

		// Update vel to be the distance we ended up moving this frame.
		SetVel(pos - ppos);
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
    override protected void Die() {
        // Spit out COINS!
        for (int i=0; i<NumCoinsInMe; i++) {
            SpawnCoinInMe();
        }
        base.Die();
        StartCoroutine(Coroutine_CorpseFall());
    }

    private void SpawnCoinInMe() {
        Coin newCoin = Instantiate(ResourcesHandler.Instance.Coin).GetComponent<Coin>();
        newCoin.Initialize(MyRoom, this.transform.localPosition);
    }
    
    
    private IEnumerator Coroutine_CorpseFall() {
        // TEMP alpha me out hackily.
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) {
            GameUtils.SetSpriteAlpha(sr, sr.color.a * 0.7f);
        }
        // Look ouch.
        if (s_bodyDead != null) {
            sr_body.sprite = s_bodyDead;
        }
        // Fall! Fall!
        Vector2 bodyVel = new Vector2(Random.Range(-6f,6f), 9f);
        float angleVel = MathUtils.RandomDir() * 18f;
        float timeWhenDisappear = Time.time + 10f;
        while (Time.time < timeWhenDisappear) {
            sr_body.transform.localEulerAngles += new Vector3(0, 0, angleVel) * Time.deltaTime;
            sr_body.transform.localPosition += new Vector3(bodyVel.x,bodyVel.y) * Time.deltaTime;
            bodyVel += new Vector2(0, -40f) * Time.deltaTime;
            yield return null;
        }
        this.gameObject.SetActive(false); // oh, just disable my GO for now.
    }
    
    




}

