using System.Collections;
using UnityEngine;
//using UnityStandardAssets.ImageEffects;

public class GameCameraController : MonoBehaviour {
	// Components
    [SerializeField] private Camera primaryCamera=null;
    [SerializeField] private GameCameraScreenShake screenShake=null;
	// Constants
	private const float ConstOrthoScale = 0.065f;//HACKy non-pixel-perfect estimation.
	private const float ZPos = -10; // lock z pos.
	// Properties
	private float orthoSizeNeutral;
	private float zoomAmount = 1; // UNUSED currently. Stays at 1. It's here for if/when we need it.
	private Rect viewRect;
	private Rect viewRectBounds; // set from each Room's CameraBounds sprite. Our viewRect is confined to this rect!
	private Rect centerBounds; // this is viewRectBounds, collapsed to just what viewRect's center can be set to.
	private Vector2 targetCenter;
	// References
	[SerializeField] private FullScrim fullScrim=null;
	[SerializeField] private GameController gameController=null;

	// Getters / Setters
	public Rect ViewRect { get { return viewRect; } }

	private Player player { get { return gameController==null ? null : gameController.Player; } }
	private float rotation {
		get { return this.transform.localEulerAngles.z; }
		set { this.transform.localEulerAngles = new Vector3 (0, 0, value); }
	}
	private Vector2 center { // Note: To avoid center/BL-corner confusion, we ONLY address our "position" as the center. Never "pos".
		get { return viewRect.center; }
		set { viewRect.center = value; }
	}

	private Rect GetViewRect (Vector2 _rectCenter, float _zoomAmount) {
		Vector2 rectSize = GetViewRectSizeFromZoomAmount (_zoomAmount);
		return new Rect (_rectCenter-rectSize*0.5f, rectSize); // Note: Convert from center to bottom-left pos.
	}
	private Vector2 GetViewRectSizeFromZoomAmount (float zoomAmountConstOrthoScale) {
		return ScreenHandler.RelativeScreenSize / zoomAmount * ConstOrthoScale;
	}
	private float GetZoomAmountForViewRect (Rect rect) {
		return Mathf.Min (ScreenHandler.RelativeScreenSize.x/(float)rect.width, ScreenHandler.RelativeScreenSize.y/(float)rect.height) * ConstOrthoScale;
	}
	private float ZoomAmount { get { return orthoSizeNeutral / primaryCamera.orthographicSize; } }
//	Debug.Log(GetExactWorldRect Camera.main.ViewportToWorldPoint(new Vector3(0,0,0));

	// Debug
	private void OnDrawGizmos() {
		//Gizmos.color = Color.blue;
		//Gizmos.DrawWireCube (viewRect.center, new Vector3(viewRect.size.x,viewRect.size.y, 10));

		Gizmos.color = new Color(0.85f,0.7f,0.2f);
		Gizmos.DrawWireCube (viewRectBounds.center, new Vector3(viewRectBounds.size.x,viewRectBounds.size.y, 10));
		Gizmos.DrawWireCube (centerBounds.center, new Vector3(centerBounds.size.x,centerBounds.size.y, 10));
        
//		Gizmos.color = Color.yellow;
//		Gizmos.DrawWireCube (viewRect.center*GameVisualProperties.WORLD_SCALE, new Vector3(ScreenHandler.RelativeScreenSize.x+11,ScreenHandler.RelativeScreenSize.y+11, 10)*GameVisualProperties.WORLD_SCALE);//+11 for bloat so we can still see it if there's overlap.
	}



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Awake () {
		// Add event listeners!
		GameManagers.Instance.EventManager.EditorSaveRoomEvent += OnEditorSaveRoom;
		GameManagers.Instance.EventManager.StartRoomEvent += OnStartRoom;
	}
	private void OnDestroy () {
		// Remove event listeners!
		GameManagers.Instance.EventManager.EditorSaveRoomEvent -= OnEditorSaveRoom;
		GameManagers.Instance.EventManager.StartRoomEvent -= OnStartRoom;
	}
	private void Reset () {
		UpdateOrthoSizeNeutral ();

		// Reset values
        screenShake.Reset();

		viewRect = new Rect ();
		viewRect.size = GetViewRectSizeFromZoomAmount (1);
		UpdateTargetCenter();
		center = new Vector2(targetCenter.x, targetCenter.y); // Start us with the Player in view.

		ApplyViewRect ();
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnStartRoom(Room room) {
		viewRectBounds = room.GetCameraBoundsGlobal();

		// Reset us now! Now that the player and everything is in place. :)
		Reset();
	}
	private void OnEditorSaveRoom() {
		if (fullScrim!=null) {
			fullScrim.FadeFromAtoB(new Color(1,1,1, 0.5f), Color.clear, 0.2f, true);
		}
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void FixedUpdate() {
		if (player == null) { return; } // Safety check.

		UpdateTargetCenter();
		StepTowardTargetCenter();

		ApplyViewRect();
	}

	private void StepTowardTargetCenter() {
		float velX = (targetCenter.x - center.x) * 0.1f;
		float velY = (targetCenter.y - center.y) * 0.1f;
		center += new Vector2(velX, velY);
	}
	private void UpdateTargetCenter() {
		// test doing this every frame. Only do it when our zoom changes, ok?
		centerBounds = new Rect();
		centerBounds.size = viewRectBounds.size;
		centerBounds.size -= viewRect.size; // collapse our possible pos space! (So viewRect's edge goes up against viewRectBounds's edge, instead of viewRect's pos going up against the edge.)
		centerBounds.size = new Vector2(Mathf.Max(0, centerBounds.size.x), Mathf.Max(0, centerBounds.size.y)); // Don't let the rect invert.
		centerBounds.center = viewRectBounds.center;

		Vector2 targetPos = player.PosGlobal;
		float targetX = Mathf.Clamp(targetPos.x, centerBounds.xMin, centerBounds.xMax);
		float targetY = Mathf.Clamp(targetPos.y, centerBounds.yMin, centerBounds.yMax);
		targetCenter = new Vector2(targetX, targetY);
	}




	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void UpdateOrthoSizeNeutral () {
		orthoSizeNeutral = ScreenHandler.OriginalScreenSize.y / 2f * ConstOrthoScale;// * GameVisualProperties.WORLD_SCALE;
	}

	private void ApplyViewRect () {
        // Base position/zoom.
		this.transform.localPosition = new Vector3 (viewRect.center.x, viewRect.center.y, ZPos);
        ApplyZoomAmountToCameraOrthographicSize ();
        // Add screen shake!
        rotation = screenShake.ShakeRot;
        this.transform.localPosition += new Vector3(screenShake.ShakePos.x,screenShake.ShakePos.y);
	}
	private void ApplyZoomAmountToCameraOrthographicSize () {
		float zoomAmount = GetZoomAmountForViewRect (viewRect);
		float targetOrthoSize = orthoSizeNeutral / zoomAmount;
		// For runtime compile. In case the zoom's gone nuts, keep it clamped.
		if (float.IsNaN(targetOrthoSize)) { targetOrthoSize = 20f; }
		targetOrthoSize = Mathf.Clamp(targetOrthoSize, 1f, 9999f);
		primaryCamera.orthographicSize = targetOrthoSize;
	}

//	private void UpdateViewRectActual () {
//		viewRect_actual = GetViewRect (this.transform.localPosition, ZoomAmount);
//	}

	public void DarkenScreenForSceneTransition () {
		fullScrim.Show (0.5f);
	}




}



