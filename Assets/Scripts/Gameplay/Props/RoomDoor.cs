using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDoor : Prop {
	// Components
	//[SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	[SerializeField] private string myID;
    [SerializeField] private int worldToIndex=-1; // if this is -1, we'll stay in THIS world.
	[SerializeField] private string roomToKey;
	[SerializeField] private string doorToID;
	//private bool isTouchingPlayer;
    private float timeWhenBorn; // in SCALED seconds. Ignore Player touching me for first second of my existence, so we don't flip-flop between two rooms in a loop.

	// Getters
    public string MyID { get { return myID; } }
    public int WorldToIndex { get { return worldToIndex; } }
    public string RoomToKey { get { return roomToKey; } }
    public string DoorToID { get { return doorToID; } }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData ToData() {
        RoomDoorData data = new RoomDoorData {
            pos = PosLocal,
            myID = myID,
            worldToIndex = worldToIndex,
            roomToKey = roomToKey,
            doorToID = doorToID,
            travelMind = new TravelMindData(travelMind),
        };
        return data;
    }

	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, RoomDoorData data) {
		base.BaseInitialize(_myRoom, data);

		myID = data.myID;
        worldToIndex = data.worldToIndex;
		roomToKey = data.roomToKey;
		doorToID = data.doorToID;
        timeWhenBorn = Time.time;
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	//private void Update() {
	//	if (isTouchingPlayer) {
	//		if (Input.GetKeyDown(KeyCode.DownArrow)) {
	//			GoToMyRoom();
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
            MyRoom.GameController.OnPlayerTouchRoomDoor(this);
        }
    }



}


