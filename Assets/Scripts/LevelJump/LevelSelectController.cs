using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
public class LevelSelectController : MonoBehaviour {
	// Components
	[SerializeField] private Transform tf_levelButtons=null;
	[SerializeField] private Text t_version=null;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		t_version.text = Application.version;

		MakeLevelButtons();
	}

	private void MakeLevelButtons() {
//		UnityEditor.EditorBuildSettingsScene[] scenes = UnityEditor.EditorBuildSettings.scenes;
//		List<string> levelNames = new List<string>();
//		for (int i=0; i<scenes.Length; i++) {
//			string path = scenes[i].path;
//			string name = path.Substring(path.LastIndexOf('/')+1); // Cut off prefix "Assets/Scenes/".
//			name = name.Substring(0,name.Length-6); // Cut off suffix ".unity".
//			levelNames.Add(name);
//		}
		string[] levelNames = new string[]{
//			"Level_0_0",
//			"Level_0_1",
//			"Level_0_2",
//			"Level_LightDraft",
//			"Level_Rebounce1",
//			"Level_Rebounce2",
//			"Level_Rebounce3",
//			"Level_Test1",
//			"Level_TestCrates",
//			"Level_TestEnemies",
//			"Level_TestSpikes",
			"ToggleGrounds1",
			"ToggleGrounds2",
			"ToggleGrounds3",
			"ToggleGrounds4",
			"ToggleGrounds5",
			"DGThereAndBackPit",
			"Level_0_3",
			"Level_0_5",
			"Level_0_6",
			"Level_0_7",
			"TGHallwayDangerHard",
			"TGJumpParadox1",
			"TGJumpParadox2",
		};
		GameObject buttonPrefab = ResourcesHandler.Instance.levelSelectLevelButton;

		for (int i=0; i<levelNames.Length; i++) {
			LevelButton newButton = Instantiate(buttonPrefab).GetComponent<LevelButton>();
			newButton.Initialize(tf_levelButtons, i, levelNames[i]);
		}

	}


}
*/
