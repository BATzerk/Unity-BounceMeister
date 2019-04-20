using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapEditorNamespace {
public class RoomTileSelectionRect : MonoBehaviour {
	// Components
	[SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	private bool isActive; // this is true when user clicks on NOT a RoomTile or anything, and is then draggin' around. False when we release.
	private Vector2 clickPos; // in world coordinates
	private Rect selectionRect = new Rect (); // the Rect that represents our selction box.
	// References
	[SerializeField] private MapEditor mapEditorRef=null;

	// Getters (private)
	private Vector2 MousePosWorld { get { return mapEditorRef.MousePosWorld; } }
	// Getters (public)
	public bool IsActive { get { return isActive; } }
	public Rect SelectionRect { get { return selectionRect; } }



	void Update () {
		RegisterMouseInput ();
		UpdateSelectionRect ();
		UpdateSelectionBorderLine ();
	}


	private void UpdateSelectionRect () {
		// I'm active!
		if (isActive) {
			selectionRect.width = Mathf.Abs (clickPos.x - MousePosWorld.x);
			selectionRect.height = Mathf.Abs (clickPos.y - MousePosWorld.y);
			
			if (MousePosWorld.x > clickPos.x) {
				selectionRect.x = clickPos.x;
			}
			else {
				selectionRect.x = clickPos.x - selectionRect.width;
			}
			if (MousePosWorld.y > clickPos.y) {
				selectionRect.y = clickPos.y;
			}
			else {
				selectionRect.y = clickPos.y - selectionRect.height;
			}
		}
		// I'm NOT active.
		else {
			// Give me no width/height.
			selectionRect.width = selectionRect.height = 0;
		}
	}
	private void UpdateSelectionBorderLine() {
		// Update its activeness!
		sr_body.enabled = isActive;

		// Update the points!!
		if (isActive) {
			sr_body.size = selectionRect.size;
			sr_body.transform.localPosition = selectionRect.center;
		}
	}


	private void RegisterMouseInput() {
//		if (Input.GetMouseButtonDown(0)) {
//			OnMouseDown ();
//		}
		if (Input.GetMouseButtonUp(0)) {
			OnMouseUp ();
		}
	}
	//	private void OnMouseDown () {
//		if (!isActive) {
//			Activate ();
//		}
//	}
	private void OnMouseUp () {
		if (isActive) {
			Deactivate();
		}
	}

	public void Activate() {
		clickPos = MousePosWorld;
		isActive = true;
	}
	private void Deactivate() {
		mapEditorRef.OnRoomTileSelectionRectDeactivated ();
		isActive = false;
	}


}
}



