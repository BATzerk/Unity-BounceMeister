using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Owned and managed by InputController, this class translates mobile touch input into gameplay input. */
public class TouchInputDetector {
	// Constants
	private const float PUSH_REQUEST_INTERVAL = 0.18f; // in seconds.
	private const float SWIPE_THRESHOLD = 15f; // how many pixels we gotta move a finger to count a swipe.
	// Properties
	private bool isTouch, isTouchDown, isTouchUp; // updated at the beginning of every frame.
	private bool pisTouch; // isTouch for the previous frame. Used to detect touch-downs/ups.
	private int pushSide; // a "push" is a request to move. If -1, no "push". If 0-3, we're requesting a "push" towards that side.
	private int pushRequestSide; // POORLY WORDED. It's for registering the actual push.
	private float timeUntilPushRequest; // so we can hold our finger down and keep making a move every 0.4 seconds.
//	private float simulatedMovePercent; // from 0 to 1. BoardView uses this value.
	private Vector2 touchDownPos;
//	private Vector2Int simulatedMoveDir;

	// Getters
	public int PushRequestSide { get { return pushRequestSide; } }
	public float SimulatedMovePercent { get { return 0;}}//simulatedMovePercent; } }
	public Vector2Int SimulatedMoveDir { get { return Vector2Int.zero;}}//simulatedMoveDir; } }

	private bool IsTouch() { return (Input.touchSupported && Input.touchCount>0) || Input.GetMouseButton(0); }
	private Vector2 GetTouchPos() {
		if (Input.touchSupported && Input.touchCount>0) { return Input.touches[0].position; }
		return Input.mousePosition;
	}
	public Vector2 Debug_TouchUpPos { get; set; }
	public Vector2 Debug_TouchDownPos { get { return touchDownPos; } }




	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	public void Update () {
		// Reset pushRequestSide every frame.
		pushRequestSide = -1;

		// Touch-down/-up/-hold!
		isTouch = IsTouch();
		isTouchUp = !isTouch && pisTouch;
		isTouchDown = isTouch && !pisTouch;

		if (isTouchDown) { OnTouchDown (); }
		if (isTouchUp) { OnTouchUp (); }
		if (isTouch) { OnTouchHeld (); }

		CountdownToPushRequest ();

		pisTouch = isTouch;
	}

	private void OnTouchHeld () {
		Debug_TouchUpPos = GetTouchPos(); // For debugging!

		// Are we NOT already pushing?? Maybe register a push!!
		if (pushSide == -1) {
			Vector2 touchPos = GetTouchPos();
			if (touchPos.x-touchDownPos.x > SWIPE_THRESHOLD) {
				RegisterPush (1);
			}
			else if (touchDownPos.x-touchPos.x > SWIPE_THRESHOLD) {
				RegisterPush (3);
			}
			else if (touchPos.y-touchDownPos.y > SWIPE_THRESHOLD) {
				RegisterPush (0);
			}
			else if (touchDownPos.y-touchPos.y > SWIPE_THRESHOLD) {
				RegisterPush (2);
			}
		}
	}
	private void OnTouchDown () {
		touchDownPos = GetTouchPos();
	}
	private void OnTouchUp () {
		// NOW we're not pushing anymore. :)
		pushSide = -1;
		timeUntilPushRequest = -1;
	}

	private void RegisterPush (int _side) {
		pushSide = _side;
		pushRequestSide = _side;
		timeUntilPushRequest = PUSH_REQUEST_INTERVAL;
	}

	private void CountdownToPushRequest () {
		if (timeUntilPushRequest > 0) {
			timeUntilPushRequest -= Time.deltaTime;
			if (timeUntilPushRequest <= 0) {
				pushRequestSide = pushSide;
				timeUntilPushRequest = PUSH_REQUEST_INTERVAL;
			}
		}
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------

}




/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Owned and managed by InputController, this class translates mobile touch input into gameplay input. * /
public class TouchInputDetector {
	// Constants
	private const float minVirtualJoystickThreshold = 0.05f; // in percent (from 0 to 1). anything movement than this will be ignored.
	private const float VJ_RADIUS = 50f;
	private const float VJ_MAGNITUDE_SWIPE_THRESHOLD = 0.5f; // if our VJ's magnitude is greater than this on a TouchUp, we'll register a swipe!
	// Properties
	private bool isTouch, isTouchDown, isTouchUp; // updated at the beginning of every frame.
	private bool pisTouch; // isTouch for the previous frame. Used to detect touch-downs/ups.
	private bool isSwipe_L; // these are true only for one frame, and then set back to false.
	private bool isSwipe_R; // these are true only for one frame, and then set back to false.
	private bool isSwipe_D; // these are true only for one frame, and then set back to false.
	private bool isSwipe_U; // these are true only for one frame, and then set back to false.
	private float simulatedMovePercent; // from 0 to 1. BoardView uses this value.
	private Vector2 touchDownPos;
	private Vector2 vjAxes; // From -1 to 1.
	private Vector2 vjCenter; // starts at touchDownPos, but will be dragged along towards the current touchPos.
	private Vector2Int simulatedMoveDir;

	// Getters
	//	public bool IsTouchDown { get { return isTouchDown; } }
	public bool IsSwipe_L { get { return isSwipe_L; } }
	public bool IsSwipe_R { get { return isSwipe_R; } }
	public bool IsSwipe_D { get { return isSwipe_D; } }
	public bool IsSwipe_U { get { return isSwipe_U; } }
	public float SimulatedMovePercent { get { return simulatedMovePercent; } }
	public Vector2Int SimulatedMoveDir { get { return simulatedMoveDir; } }

	private bool IsTouch() { return (Input.touchSupported && Input.touchCount>0) || Input.GetMouseButton(0); }
	private Vector2 GetTouchPos() {
		if (Input.touchSupported && Input.touchCount>0) { return Input.touches[0].position; }
		return Input.mousePosition;
	}
	public Vector2 Debug_TouchUpPos { get; set; }
	public Vector2 Debug_TouchDownPos { get { return touchDownPos; } }

	private Vector2 GetVirtualJoystickAxes (Vector2 _touchPos, Vector2 _touchDownPos) {
		Vector2 returnVector = new Vector2 ((_touchPos.x-_touchDownPos.x)/VJ_RADIUS, (_touchPos.y-_touchDownPos.y)/VJ_RADIUS);
		returnVector = Vector2.ClampMagnitude (returnVector, 1);
		return returnVector;
	}



	private Vector2Int GetSimulatedMoveDir (Vector2 virtualJoystickAxes) {
		if (virtualJoystickAxes.magnitude > minVirtualJoystickThreshold) {
			if (Mathf.Abs(virtualJoystickAxes.x) > Mathf.Abs(virtualJoystickAxes.y)) {
				if (virtualJoystickAxes.x<0) { return new Vector2Int (-1,0); }
				return new Vector2Int ( 1,0);
			}
			else {
				if (virtualJoystickAxes.y<0) { return new Vector2Int (0, 1); }
				return new Vector2Int (0,-1);
			}
		}
		else {
			return Vector2Int.zero;
		}
	}
	private float GetSimulatedMovePercent (Vector2 _virtualJoystickAxes) {
		return _virtualJoystickAxes.magnitude;
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	public void Update () {
		// Reset swipe truthiness!
		isSwipe_L = isSwipe_R = isSwipe_D = isSwipe_U = false;

		// Touch-down/-up/-hold!
		isTouch = IsTouch();
		isTouchUp = !isTouch && pisTouch;
		isTouchDown = isTouch && !pisTouch;

		if (isTouchDown) { OnTouchDown (); }
		if (isTouchUp) { OnTouchUp (); }
		if (isTouch) { OnTouchHeld (); }

		pisTouch = isTouch;
	}

	private void OnTouchHeld () {
		Debug_TouchUpPos = GetTouchPos(); // For debugging!
		// Update the VirtualJoystick's values!
		UpdateVJValues ();
		// Update simulated move, yo!
		UpdateSimulatedMove ();
	}
	private void OnTouchDown () {
		touchDownPos = GetTouchPos();
		vjCenter = touchDownPos;
	}
	private void OnTouchUp () {
		// Update the VirtualJoystick's values!
		UpdateVJValues ();
		// Maybe register a swipe!!
		if (vjAxes.magnitude > VJ_MAGNITUDE_SWIPE_THRESHOLD) {
			RegisterVJSwipe ();
		}
		// Nix any simulated move.
		SetSimulatedMoveDir (Vector2Int.zero);
	}

	private void UpdateVJValues () {
		Vector2 touchPos = GetTouchPos();
		UpdateVJCenterPos (touchPos);
		vjAxes = GetVirtualJoystickAxes (touchPos, vjCenter);
	}
	/** Drags/"tethers" the center to be at least X close to our finger. * /
	private void UpdateVJCenterPos (Vector2 touchPos) {
		float distTouchToCenter = Vector2.Distance (touchPos, vjCenter);
		// If our finger's moved too far from the center, bring the center TO us!
		if (distTouchToCenter > VJ_RADIUS) {
			float angle = Mathf.Atan2 (touchPos.y-vjCenter.y, touchPos.x-vjCenter.x);
			vjCenter = touchPos - new Vector2 (Mathf.Cos (angle)*VJ_RADIUS, Mathf.Sin (angle)*VJ_RADIUS);
		}
	}

	private void UpdateSimulatedMove () {
		Vector2Int thisSimulatedMoveDir = GetSimulatedMoveDir (vjAxes);
		if (thisSimulatedMoveDir != simulatedMoveDir) { // If the proposed simulated moveDir is *different* from the current one...!
			SetSimulatedMoveDir (thisSimulatedMoveDir);
		}
		else if (simulatedMoveDir != Vector2Int.zero) {
			simulatedMovePercent = GetSimulatedMovePercent (vjAxes);
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void RegisterVJSwipe () {
		Vector2 swipeAxes = vjAxes;
		// Which direction did we swipe, Chez??
		if (Mathf.Abs(swipeAxes.x) > Mathf.Abs(swipeAxes.y)) { // more horizontal than vertical!
			if (swipeAxes.x < 0) { isSwipe_L = true; }
			else { isSwipe_R = true; }
		}
		else { // more vertical than horizontal!
			if (swipeAxes.y < 0) { isSwipe_D = true; }
			else { isSwipe_U = true; }
		}
	}
	private void SetSimulatedMoveDir (Vector2Int _simulatedMoveDir) {
		simulatedMoveDir = _simulatedMoveDir;
	}

}
*/