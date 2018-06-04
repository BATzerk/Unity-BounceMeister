using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {
	// Properties
	private string levelName;
	// Components
	[SerializeField] private Text t_levelName;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Transform parentTransform, int index, string levelName) {
		this.levelName = levelName;
		t_levelName.text = levelName;

		this.transform.SetParent(parentTransform);
		this.transform.localScale = Vector3.one;
		this.transform.localEulerAngles = Vector3.zero;
		RectTransform myRectTransform = GetComponent<RectTransform>();
		myRectTransform.anchoredPosition = new Vector2(-20, -20 - index*40);

	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnClick() {
		UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
	}


}
