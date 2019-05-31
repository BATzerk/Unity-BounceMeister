using UnityEngine;
using System.Collections;

namespace MapEditorNamespace {
public class MapEditorCamera : MonoBehaviour {
    // Constants
	private const float ARROW_KEYS_PAN_SPEED = 20f; // for panning the map with the arrow keys. Scales based on mapScale.
	private const float DRAG_PANNING_SPEED = 0.1f; // higher is faster.
	private const float MOUSE_SCROLL_ZOOM_RATIO = 0.02f; // mouseScrollDelta * this = the % zoom change
	private const float MAP_SCALE_DEFAULT = 0.9f;
	private const float ZOOM_SPEED_KEYBOARD = 0.04f; // higher is faster.
	// Components
	new private Camera camera; // set in Awake.
    // Properties
    private bool isDragPanning; // true when we right-mouse-click and drag to fast-pan around the map.
    private bool isGrabPanning; // true when we middle-mouse-click to pan around the map. This is the limited scroll.
    public float MapScale { get; private set; }
    private Vector2 posOnMouseDown;
    // References
    private MapEditor editor; // set in Awake.

    // Getters (Private)
    private float fts { get { return TimeController.FrameTimeScaleUnscaled; } }
    private Vector2 MousePosScreen { get { return editor.MousePosScreen; } }
    private Vector2 MousePosScreenOnDown { get { return editor.MousePosScreenOnDown; } }
    //// Getters (Public)
    //public float MapScale { get { return mapScale; } }

    // Setters
    public Vector2 Pos {
        get { return transform.localPosition; }
        set {
            transform.localPosition = new Vector3(value.x, value.y, transform.localPosition.z);
            SavePos();
        }
    }


    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake() {
		camera = this.GetComponent<Camera>();
        editor = FindObjectOfType<MapEditor>();

        // Move the camera to where we remember it was last time we was here!
        LoadPos();
        LoadScale();
        // Add event listeners.
        GameManagers.Instance.EventManager.MapEditorSetCurrWorldEvent += OnSetCurrWorld;
    }
    private void OnDestroy() {
        // Remove event listeners.
        GameManagers.Instance.EventManager.MapEditorSetCurrWorldEvent -= OnSetCurrWorld;
    }


    // ----------------------------------------------------------------
    //  Save / Load
    // ----------------------------------------------------------------
    private void LoadPos() {
        Pos = new Vector2(SaveStorage.GetFloat(SaveKeys.MapEditor_CameraPosX), SaveStorage.GetFloat(SaveKeys.MapEditor_CameraPosY));
    }
    private void LoadScale() {
        SetMapScale(SaveStorage.GetFloat(SaveKeys.MapEditor_MapScale, MAP_SCALE_DEFAULT));
    }
    private void SavePos() {
        SaveStorage.SetFloat(SaveKeys.MapEditor_CameraPosX, Pos.x);
        SaveStorage.SetFloat(SaveKeys.MapEditor_CameraPosY, Pos.y);
    }
    private void SaveScale() {
        SaveStorage.SetFloat(SaveKeys.MapEditor_MapScale, MapScale);
    }



    // ----------------------------------------------------------------
    //  Set / Reset
    // ----------------------------------------------------------------
    private void EaseToNeutral() {
        // Reset scale
        Vector2 averageRoomPos = new Vector2 (0,0);
        WorldData wd = editor.CurrWorldData;
        foreach (RoomData rd in wd.RoomDatas.Values) {
            averageRoomPos += rd.PosGlobal;
        }
        averageRoomPos /= wd.RoomDatas.Count;
        // Ease-y!
        LeanTween.cancel(this.gameObject);
        LeanTween.value(this.gameObject, SetMapScale, MapScale,MAP_SCALE_DEFAULT, 0.26f).setEaseOutQuint();
        LeanTween.value(this.gameObject, SetPos, Pos,averageRoomPos, 0.3f).setEaseOutQuint();
    }

    private void SetBackgroundColor(int worldIndex) {
//		BGTheme bgTheme = Colors.GetBGTheme (WorldIndex);
//		camera.backgroundColor = Color.Lerp (Colors.GetBGColor_ViewGameplay (bgTheme), Color.white, 0.2f); // make it lighter so we can see shit better
	}
	public void BlurScreenForLoading() {
//		// Enable my blur script!
//		BlurOptimized blurScript = this.GetComponent<BlurOptimized> ();
//		if (blurScript != null) {
//			blurScript.enabled = true;
//		}
	}

    private void SetMapScale(float _mapScale) {
        // Set/save value.
		MapScale = _mapScale;
		MapScale = Mathf.Max(0.4f, Mathf.Min (10, MapScale)); // Don't let scale get TOO crazy now.
        SaveScale();
        // Apply value.
        float cameraSize = ScreenHandler.OriginalScreenSize.y/2f;
        cameraSize /= MapScale;
        camera.orthographicSize = cameraSize;
        // Tell the Editor!
        editor.OnSetMapScale();
    }
    private void SetPos(Vector2 _pos) {
        Pos = _pos;
    }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void MoveCamera(float xMove, float yMove) {
		Pos += new Vector2(xMove, yMove);
    }
    private void ZoomMapAtPoint(Vector3 screenPoint,float deltaZoom) {
        float pmapScale = MapScale;
        SetMapScale(MapScale * (1 + deltaZoom));
        // If we DID change the zoom, then also move to focus on that point!
        if (MapScale - pmapScale != 0) {
            MoveCamera(screenPoint.x * deltaZoom / MapScale,screenPoint.y * deltaZoom / MapScale);
        }
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnSetCurrWorld(int worldIndex) {
        SetBackgroundColor(worldIndex); // Set background colla
    }


    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void LateUpdate() {
        RegisterMouseInput();
        RegisterButtonInput();
        UpdatePanning();
    }
    private void RegisterMouseInput() {
        // Touch down/up
        if (InputController.IsMouseButtonDown()) { OnMouseDown(); }
        else if (InputController.IsMouseButtonUp()) { OnMouseUp(); }
        // Wheelies
        if (Input.mouseScrollDelta != Vector2.zero) {
            float zoomSpeedScale = (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) ? 5 : 1; // hold down SHIFT to zoom fastahh!
            ZoomMapAtPoint(MousePosScreen, Input.mouseScrollDelta.y*MOUSE_SCROLL_ZOOM_RATIO * zoomSpeedScale);
        }
    }
    
	private void OnMouseDown() {
		int mouseButton = InputController.GetMouseButtonDown();
		// RIGHT click?
		if (mouseButton == 1) {
			isDragPanning = true;
		}
		// MIDDLE click?
		else if (mouseButton == 2) {
            isGrabPanning = true;
        }
        // Update on-mouse-down vectors!
        posOnMouseDown = Pos;
    }
	private void OnMouseUp() {
		int mouseButton = InputController.GetMouseButtonUp();

		// RIGHT click?
		if (mouseButton == 1) {
            isDragPanning = false;
		}
		// MIDDLE click?
		else if (mouseButton == 2) {
			isGrabPanning = false;
		}
	}
    private void RegisterButtonInput() {
        if (!InputController.IsKey_alt && !InputController.IsKey_control && !InputController.IsKey_shift) {
            // C = Reset to neutral
		    if (Input.GetKeyDown(KeyCode.C)) { EaseToNeutral(); }

            // Arrow/WASD/Joystick Panning!
            //else if (Input.GetKey(KeyCode.LeftArrow)) { MoveCamera(-ARROW_KEYS_PAN_SPEED/mapScale*fTS,0); }
            //      else if (Input.GetKey(KeyCode.RightArrow)) { MoveCamera(ARROW_KEYS_PAN_SPEED/mapScale*fTS,0); }
            //      else if (Input.GetKey(KeyCode.DownArrow)) { MoveCamera(0,-ARROW_KEYS_PAN_SPEED/mapScale*fTS); }
            //      else if (Input.GetKey(KeyCode.UpArrow)) { MoveCamera(0,ARROW_KEYS_PAN_SPEED/mapScale*fTS); }
            Vector2 inputAxis = InputController.Instance.LeftStick;
            if (inputAxis.magnitude > 0.1f) {
                Pos += inputAxis * (ARROW_KEYS_PAN_SPEED/MapScale*fts);
            }

            // Zoom
		    if (Input.GetKey(KeyCode.Z)) { SetMapScale(MapScale/(1-ZOOM_SPEED_KEYBOARD*fts)); }
            else if (Input.GetKey(KeyCode.X)) { SetMapScale(MapScale*(1-ZOOM_SPEED_KEYBOARD*fts)); }
        }
    }

    private void UpdatePanning() {
        // Grab Panning!
        if (isGrabPanning) {
            Pos = posOnMouseDown + new Vector2(MousePosScreenOnDown.x-MousePosScreen.x,MousePosScreenOnDown.y-MousePosScreen.y) / MapScale;
        }
        // Drag Panning!
        else if (isDragPanning) {
            Pos += new Vector2(MousePosScreen.x-MousePosScreenOnDown.x,MousePosScreen.y-MousePosScreenOnDown.y) * DRAG_PANNING_SPEED * fts;
        }
    }



}
}