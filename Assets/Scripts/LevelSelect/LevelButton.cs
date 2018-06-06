using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {
	// Components
	[SerializeField] private Text t_levelName;
	// References
	private LevelData myLevelData;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(RectTransform rt_parent, LevelData _myLevelData, Vector2 _pos, Vector2 _size) {
		this.myLevelData = _myLevelData;
		t_levelName.text = myLevelData.LevelKey;

		this.transform.SetParent(rt_parent);
		this.transform.localScale = Vector3.one;
		this.transform.localEulerAngles = Vector3.zero;
		RectTransform myRectTransform = GetComponent<RectTransform>();
		myRectTransform.anchoredPosition = _pos;
		myRectTransform.sizeDelta = _size;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnClick() {
		GameManagers.Instance.DataManager.currentLevelData = myLevelData;

		UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Gameplay);
	}


}
