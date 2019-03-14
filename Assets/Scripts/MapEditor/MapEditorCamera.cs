using UnityEngine;
using System.Collections;

public class MapEditorCamera : MonoBehaviour {
	// Components
	new private Camera camera;

    // Setters
    public Vector2 Pos {
        get { return transform.localPosition; }
        set {
            transform.localPosition = new Vector3(value.x, value.y, transform.localPosition.z);
            SavePos();
        }
    }


    // ----------------------------------------------------------------
    //  Awake
    // ----------------------------------------------------------------
    private void Awake() {
		camera = this.GetComponent<Camera>();
        LoadPos();
    }


    // ----------------------------------------------------------------
    //  Save / Load
    // ----------------------------------------------------------------
    private void LoadPos() {
        Pos = new Vector2(SaveStorage.GetFloat(SaveKeys.MapEditor_CameraPosX), SaveStorage.GetFloat(SaveKeys.MapEditor_CameraPosY));
    }
    private void SavePos() {
        SaveStorage.SetFloat(SaveKeys.MapEditor_CameraPosX, Pos.x);
        SaveStorage.SetFloat(SaveKeys.MapEditor_CameraPosY, Pos.y);
    }



    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetBackgroundColor(int worldIndex) {
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


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnSetCurrentWorld(int worldIndex) {
        SetBackgroundColor(worldIndex);
    }


}
