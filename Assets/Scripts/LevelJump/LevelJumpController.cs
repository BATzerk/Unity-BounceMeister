using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelJumpController : MonoBehaviour {
	// Components
	[SerializeField] private Text t_version=null;
	[SerializeField] private Text t_worldHeader=null;
	[SerializeField] private RectTransform rt_levelButtons=null;
	// Properties
	private int selectedWorldIndex;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	private void Start() {
		t_version.text = Application.version;

		LevelData currentLD = GameManagers.Instance.DataManager.currLevelData;
		int worldIndex = currentLD == null ? 0 : currentLD.WorldIndex;
		SetSelectedWorld(worldIndex);
	}

	private void SetSelectedWorld(int _selectedWorldIndex) {
		// Outta bounds? Do nothin'!
		if (_selectedWorldIndex<0 || _selectedWorldIndex>=GameManagers.Instance.DataManager.NumWorldDatas) {
			return;
		}

		selectedWorldIndex = _selectedWorldIndex;

		t_worldHeader.text = "World " + selectedWorldIndex;

		DestroyLevelButtons();
		MakeLevelButtons();
	}
	private void DestroyLevelButtons() {
		GameUtils.DestroyAllChildren(rt_levelButtons.gameObject.transform);
	}
	private void MakeLevelButtons() {
		GameObject buttonPrefab = ResourcesHandler.Instance.LevelJumpLevelButton;

		WorldData wd = GameManagers.Instance.DataManager.GetWorldData(selectedWorldIndex);
		float tempX = 0;
		float tempY = 0;
		Vector2 buttonSize = new Vector2(200, 30);
		Vector2 buttonGap = new Vector2(8, 6);
		foreach (LevelData ld in wd.levelDatas.Values) {
			LevelButton newButton = Instantiate(buttonPrefab).GetComponent<LevelButton>();
			newButton.Initialize(rt_levelButtons, ld, new Vector2(tempX,tempY), buttonSize);

			tempY -= buttonSize.y + buttonGap.y;
			if (-tempY+buttonSize.y+buttonGap.y > rt_levelButtons.rect.height) { // Loop da loop.
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



    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Debug
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
        if (UnityEditor.EditorApplication.isPlaying) {
            SceneHelper.ReloadScene();
        }
    }
#endif

}