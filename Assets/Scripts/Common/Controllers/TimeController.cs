using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour {
	// Properties
	static private float frameTimeScale;
	static private float frameTimeScaleUnscaled;

	// Getters
	static public float FrameTimeScale { get { return frameTimeScale; } }
	static public float FrameTimeScaleUnscaled { get { return frameTimeScaleUnscaled; } }


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		frameTimeScale = GameProperties.TARGET_FRAME_RATE * Time.deltaTime;
		frameTimeScaleUnscaled = GameProperties.TARGET_FRAME_RATE * Time.unscaledDeltaTime;
	}



}
