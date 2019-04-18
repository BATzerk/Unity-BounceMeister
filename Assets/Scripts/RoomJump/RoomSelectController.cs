using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
public class RoomSelectController : MonoBehaviour {
	// Components
	[SerializeField] private Transform tf_roomButtons=null;
	[SerializeField] private Text t_version=null;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		t_version.text = Application.version;

		MakeRoomButtons();
	}

	private void MakeRoomButtons() {
//		UnityEditor.EditorBuildSettingsScene[] scenes = UnityEditor.EditorBuildSettings.scenes;
//		List<string> roomNames = new List<string>();
//		for (int i=0; i<scenes.Length; i++) {
//			string path = scenes[i].path;
//			string name = path.Substring(path.LastIndexOf('/')+1); // Cut off prefix "Assets/Scenes/".
//			name = name.Substring(0,name.Length-6); // Cut off suffix ".unity".
//			roomNames.Add(name);
//		}
		string[] roomNames = new string[]{
//			"Room_0_0",
//			"Room_0_1",
//			"Room_0_2",
//			"Room_LightDraft",
//			"Room_Rebounce1",
//			"Room_Rebounce2",
//			"Room_Rebounce3",
//			"Room_Test1",
//			"Room_TestCrates",
//			"Room_TestEnemies",
//			"Room_TestSpikes",
			"ToggleGrounds1",
			"ToggleGrounds2",
			"ToggleGrounds3",
			"ToggleGrounds4",
			"ToggleGrounds5",
			"DGThereAndBackPit",
			"Room_0_3",
			"Room_0_5",
			"Room_0_6",
			"Room_0_7",
			"TGHallwayDangerHard",
			"TGJumpParadox1",
			"TGJumpParadox2",
		};
		GameObject buttonPrefab = ResourcesHandler.Instance.roomSelectRoomButton;

		for (int i=0; i<roomNames.Length; i++) {
			RoomButton newButton = Instantiate(buttonPrefab).GetComponent<RoomButton>();
			newButton.Initialize(tf_roomButtons, i, roomNames[i]);
		}

	}


}
*/
