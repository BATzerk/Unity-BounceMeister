using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDoor : Prop, ISerializableData<LevelDoorData> {
	// Components
	[SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	[SerializeField] private string myID;
	[SerializeField] private string levelToKey;
	[SerializeField] private string levelToDoorID;
	private bool isTouchingPlayer;
//	// References
//	[SerializeField] private LayerMask PlayerLayer;

	// Getters
	public string MyID { get { return myID; } }


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, LevelDoorData data) {
		base.BaseInitialize(_myLevel, data);

		myID = data.myID;
		levelToKey = data.levelToKey;
		levelToDoorID = data.levelToDoorID;
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GoToMyLevel() {
		// Set the door we're gonna start at!
//		GameManagers.Instance.DataManager.levelToDoorID = levelToDoorID;
		// Load the level!
		string sceneName = levelToKey;
		UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName);
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update() {
		if (isTouchingPlayer) {
			if (Input.GetKeyDown(KeyCode.DownArrow)) {
				GoToMyLevel();
			}
		}

	}

	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate() {
		float alpha = isTouchingPlayer ? 1f : 0.4f;
		sr_body.color = new Color(sr_body.color.r,sr_body.color.g,sr_body.color.b, alpha);

		isTouchingPlayer = false; // Reset this here, yo.
	}
	private void OnTriggerStay2D(Collider2D col) {
		bool isPlayer = LayerMask.LayerToName(col.gameObject.layer) == Layers.Player;
		if (isPlayer) {
			isTouchingPlayer = true;
		}
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public LevelDoorData SerializeAsData() {
		LevelDoorData data = new LevelDoorData();
		data.pos = PosLocal;
		data.myID = myID;
		data.levelToKey = levelToKey;
		data.levelToDoorID = levelToDoorID;
		return data;
	}

}


