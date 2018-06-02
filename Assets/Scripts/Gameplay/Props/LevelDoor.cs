using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDoor : MonoBehaviour {
	// Components
	[SerializeField] private SpriteRenderer sr_body;
	// Properties
	[SerializeField] private string myID;
	[SerializeField] private string levelToName;
	[SerializeField] private string levelToDoorID;
	private bool isTouchingPlayer;
//	// References
//	[SerializeField] private LayerMask PlayerLayer;

	// Getters
	public string MyID { get { return myID; } }
	public Vector2 Pos { get { return this.transform.localPosition; } }


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void GoToMyLevel() {
		// Set the door we're gonna start at!
		GameManagers.Instance.DataManager.levelToDoorID = levelToDoorID;
		// Load the level!
		string sceneName = "Level_" + levelToName;
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

		sr_body.color = isTouchingPlayer ? Color.green : Color.magenta;
	}

	// ----------------------------------------------------------------
	//  FixedUpdate
	// ----------------------------------------------------------------
	private void FixedUpdate() {
		isTouchingPlayer = false; // Reset this here, yo.
	}
	private void OnTriggerStay2D(Collider2D col) {
		bool isPlayer = LayerMask.LayerToName(col.gameObject.layer) == LayerNames.Player;
		if (isPlayer) {
			isTouchingPlayer = true;
		}
	}


}


