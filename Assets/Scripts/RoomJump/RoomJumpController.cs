using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomJumpController : MonoBehaviour {
	// Components
	[SerializeField] private Text t_version=null;
	[SerializeField] private Text t_worldHeader=null;
	[SerializeField] private RectTransform rt_roomButtons=null;
	// Properties
	private int selectedWorldIndex;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		t_version.text = Application.version;

		RoomData currRD = GameManagers.Instance.DataManager.currRoomData;
		int worldIndex = currRD == null ? 0 : currRD.WorldIndex;
		SetSelectedWorld(worldIndex);
	}

	private void SetSelectedWorld(int _selectedWorldIndex) {
		// Outta bounds? Do nothin'!
		if (_selectedWorldIndex<0 || _selectedWorldIndex>=GameManagers.Instance.DataManager.NumWorldDatas) {
			return;
		}

		selectedWorldIndex = _selectedWorldIndex;

		t_worldHeader.text = "World " + selectedWorldIndex;

		DestroyRoomButtons();
		MakeRoomButtons();
	}
	private void DestroyRoomButtons() {
		GameUtils.DestroyAllChildren(rt_roomButtons.gameObject.transform);
	}
	private void MakeRoomButtons() {
		GameObject buttonPrefab = ResourcesHandler.Instance.RoomJumpRoomButton;

		WorldData wd = GameManagers.Instance.DataManager.GetWorldData(selectedWorldIndex);
		float tempX = 0;
		float tempY = 0;
		Vector2 buttonSize = new Vector2(200, 30);
		Vector2 buttonGap = new Vector2(8, 6);
		foreach (RoomData rd in wd.roomDatas.Values) {
			RoomButton newButton = Instantiate(buttonPrefab).GetComponent<RoomButton>();
			newButton.Initialize(rt_roomButtons, rd, new Vector2(tempX,tempY), buttonSize);

			tempY -= buttonSize.y + buttonGap.y;
			if (-tempY+buttonSize.y+buttonGap.y > rt_roomButtons.rect.height) { // Loop da loop.
				tempY = 0;
				tempX += buttonSize.x + buttonGap.x;
			}
		}

	}


    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
	private void Update() {
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			SetSelectedWorld(selectedWorldIndex-1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow)) {
			SetSelectedWorld(selectedWorldIndex+1);
		}
	}


}