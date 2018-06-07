using UnityEngine;
using System.Collections;

public class MapEditorCamera : MonoBehaviour {
	// Components
	[SerializeField] new private Camera camera;


	private void Awake() {
		camera = this.GetComponent<Camera>();
	}

	public void SetBackgroundColor (int worldIndex) {
//		BGTheme bgTheme = Colors.GetBGTheme (worldIndex);
//		camera.backgroundColor = Color.Lerp (Colors.GetBGColor_ViewGameplay (bgTheme), Color.white, 0.2f); // make it lighter so we can see shit better
	}
	public void ResetPos() {
		this.transform.localPosition = new Vector3 (0, 0, transform.localPosition.z);
	}

	public void BlurScreenForLoading() {
//		// Enable my blur script!
//		BlurOptimized blurScript = this.GetComponent<BlurOptimized> ();
//		if (blurScript != null) {
//			blurScript.enabled = true;
//		}
	}

	public void SetScale(float mapScale) {
		float cameraSize = ScreenHandler.OriginalScreenSize.y/2f;
		cameraSize /= mapScale;
		camera.orthographicSize = cameraSize;
	}


}
