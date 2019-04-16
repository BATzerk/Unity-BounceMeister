using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDoor : Prop {
	// Components
	//[SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	[SerializeField] private string myID;
    [SerializeField] private int worldToIndex=-1; // if this is -1, we'll stay in THIS world.
	[SerializeField] private string levelToKey;
	[SerializeField] private string levelToDoorID; // NOTE: UNUSED right now!
	//private bool isTouchingPlayer;
    private float timeWhenBorn; // in SCALED seconds. Ignore Player touching me for first second of my existence, so we don't flip-flop between two levels in a loop.

	// Getters
	public string MyID { get { return myID; } }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        LevelDoorData data = new LevelDoorData {
            pos = PosLocal,
            myID = myID,
            worldToIndex = worldToIndex,
            levelToKey = levelToKey,
            levelToDoorID = levelToDoorID
        };
        return data;
    }

	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, LevelDoorData data) {
		base.BaseInitialize(_myLevel, data);

		myID = data.myID;
        worldToIndex = data.worldToIndex;
		levelToKey = data.levelToKey;
		levelToDoorID = data.levelToDoorID;
        timeWhenBorn = Time.time;
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GoToMyLevel() {
        if (myLevel==null) { return; } // Safety check.
		// Set the door we're gonna start at!
		GameManagers.Instance.DataManager.levelToDoorID = levelToDoorID;
		// Load the level!
        int _worldIndex = worldToIndex==-1 ? myLevel.WorldIndex : worldToIndex; // Haven't defined worldToIndex? Stay in my world.
        LevelData ldTo = GameManagers.Instance.DataManager.GetLevelData(_worldIndex, levelToKey, false);
        if (ldTo == null) { // Safety check.
            Debug.LogWarning("LevelDoor can't go to LevelData; doesn't exist. World: " + _worldIndex + ", LevelKey: " + levelToKey);
        }
        else { // There IS a level to go to! Go!
            SceneHelper.OpenGameplayScene(ldTo);
        }
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	//private void Update() {
	//	if (isTouchingPlayer) {
	//		if (Input.GetKeyDown(KeyCode.DownArrow)) {
	//			GoToMyLevel();
	//		}
	//	}
	//}

	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	//private void FixedUpdate() {
	//	float alpha = isTouchingPlayer ? 1f : 0.4f;
	//	sr_body.color = new Color(sr_body.color.r,sr_body.color.g,sr_body.color.b, alpha);

	//	isTouchingPlayer = false; // Reset this here, yo.
	//}
    //private void OnTriggerStay2D(Collider2D col) {
    //    bool isPlayer = LayerMask.LayerToName(col.gameObject.layer) == Layers.Player;
    //    if (isPlayer) {
    //        isTouchingPlayer = true;
    //    }
    //}
    private void OnTriggerEnter2D(Collider2D col) {
        if (Time.time < timeWhenBorn+0.1f) { return; } // I was like JUST born?? Do nothin'.
        
        bool isPlayer = LayerMask.LayerToName(col.gameObject.layer) == Layers.Player;
        if (isPlayer) {
            GoToMyLevel();
        }
    }



}


