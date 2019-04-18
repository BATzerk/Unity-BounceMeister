using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour {
	// Components
	[SerializeField] private Text t_roomName;
	// References
	private RoomData myRoomData;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(RectTransform rt_parent, RoomData _myRoomData, Vector2 _pos, Vector2 _size) {
		this.myRoomData = _myRoomData;
		t_roomName.text = myRoomData.RoomKey;

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
		GameManagers.Instance.DataManager.currRoomData = myRoomData;

		UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Gameplay);
	}


}
