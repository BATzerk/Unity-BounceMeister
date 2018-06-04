using System.Collections;
using UnityEngine;
//using UnityStandardAssets.ImageEffects;

public class GameCameraController : MonoBehaviour {
	// Camera
	[SerializeField] private Camera primaryCamera=null;
	[SerializeField] private SpriteRenderer sr_bounds=null; // we use a sprite to represent the visual bounds of each level!
	// Constants
	private const float ConstOrthoScale = 0.06f;//HACKy non-pixel-perfect estimation.
	private const float ZPos = -10; // lock z pos.
	// Properties
	private float orthoSizeNeutral;
	private float zoomAmount = 1; // UNUSED currently. Stays at 1. It's here for if/when we need it.
	private float screenShakeVolume;
	private float screenShakeVolumeVel;
	private Vector2 vel;
	private Rect viewRect;
	private Rect viewRectBounds; // set from sr_bounds. Our viewRect may not exit this rect!
	private Rect posBounds; // this is viewRectBounds, collapsed to just what viewRect's center can be set to.
	// References
	[SerializeField] private FullScrim fullScrim=null;
	[SerializeField] private GameController gameController=null;

	// Getters / Setters
	public Rect ViewRect { get { return viewRect; } }
	private Transform tf_player { get { return gameController.Player.transform; } }

	private float rotation {
		get { return this.transform.localEulerAngles.z; }
		set { this.transform.localEulerAngles = new Vector3 (0, 0, value); }
	}
	private Vector2 pos {
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

	// Debug
	private void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube (viewRect.center, new Vector3(viewRect.size.x,viewRect.size.y, 10));

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube (viewRectBounds.center, new Vector3(viewRectBounds.size.x,viewRectBounds.size.y, 10));

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube (posBounds.center, new Vector3(posBounds.size.x,posBounds.size.y, 10));
//		Gizmos.color = Color.yellow;
//		Gizmos.DrawWireCube (viewRect.center*GameVisualProperties.WORLD_SCALE, new Vector3(ScreenHandler.RelativeScreenSize.x+11,ScreenHandler.RelativeScreenSize.y+11, 10)*GameVisualProperties.WORLD_SCALE);//+11 for bloat so we can still see it if there's overlap.
	}



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Awake () {
		// Add event listeners!
		GameManagers.Instance.EventManager.PlayerDieEvent += OnPlayerDie;
		GameManagers.Instance.EventManager.ScreenSizeChangedEvent += OnScreenSizeChanged;
		GameManagers.Instance.EventManager.StartLevelEvent += OnStartLevel;
	}
	private void OnDestroy () {
		// Remove event listeners!
		GameManagers.Instance.EventManager.PlayerDieEvent -= OnPlayerDie;
		GameManagers.Instance.EventManager.ScreenSizeChangedEvent -= OnScreenSizeChanged;
		GameManagers.Instance.EventManager.StartLevelEvent -= OnStartLevel;
	}
	private void Reset () {
		UpdateOrthoSizeNeutral ();

		SetBoundsFromSprite();

		// Reset values
		vel = Vector2.zero;
		screenShakeVolume = 0;
		screenShakeVolumeVel = 0;

		viewRect = new Rect ();
		viewRect.size = GetViewRectSizeFromZoomAmount (1);
		pos = new Vector2(tf_player.localPosition.x, this.transform.localPosition.y); // Start us with the Player in view.

		ApplyViewRect ();
	}
	private void SetBoundsFromSprite() {
		if (sr_bounds != null) {
			viewRectBounds = new Rect();
			viewRectBounds.size = sr_bounds.size;
			viewRectBounds.center = sr_bounds.transform.localPosition;
			// Of course, hide the sprite! It's just for the editor.
			sr_bounds.enabled = false;
		}
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnStartLevel() {
		// Reset us now! Now that the player and everything is in place. :)
		Reset();
	}
	private void OnScreenSizeChanged () {
//		// Go ahead and totally reset me completely when the screen size changes, just to be safe.
//		Reset ();
	}



	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void FixedUpdate() {
		UpdateApplyVel();
		UpdateScreenShake ();

		ApplyViewRect();
	}

	private void UpdateApplyVel() {
		// test doing this every frame. Only do it when our zoom changes, ok?
		posBounds = new Rect();
		posBounds.size = viewRectBounds.size;
		posBounds.size -= viewRect.size; // collapse our possible pos space! (So viewRect's edge goes up against viewRectBounds's edge, instead of viewRect's pos going up against the edge.)
		posBounds.size = new Vector2(Mathf.Max(0, posBounds.size.x), Mathf.Max(0, posBounds.size.y)); // Don't let the rect invert.
		posBounds.center = viewRectBounds.center;

		float targetX = tf_player.localPosition.x;
		float targetY = tf_player.localPosition.y;
		targetX = Mathf.Clamp(targetX, posBounds.xMin, posBounds.xMax);
		targetY = Mathf.Clamp(targetY, posBounds.yMin, posBounds.yMax);

		float velX = (targetX - pos.x) * 0.1f;
		float velY = (targetY - pos.y) * 0.1f;
		vel = new Vector2(velX, velY);
		pos += vel;
	}


	private void UpdateScreenShake () {
		if (screenShakeVolume==0 && screenShakeVolumeVel==0) {
			return;
		}
		screenShakeVolume += screenShakeVolumeVel;
		screenShakeVolumeVel += (0-screenShakeVolume) / 5f;
		screenShakeVolumeVel *= 0.9f;
		if (screenShakeVolume != 0) {
			if (Mathf.Abs (screenShakeVolume) < 0.001f && Mathf.Abs (screenShakeVolumeVel) < 0.001f) {
				screenShakeVolume = 0;
				screenShakeVolumeVel = 0;
			}
		}

		float rotation = screenShakeVolume;
		if (transform.localEulerAngles.z != rotation) {
			transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, transform.localEulerAngles.y, rotation);
		}
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	private void UpdateOrthoSizeNeutral () {
		orthoSizeNeutral = ScreenHandler.OriginalScreenSize.y / 2f * ConstOrthoScale;// * GameVisualProperties.WORLD_SCALE;
	}

	private void ApplyViewRect () {
		this.transform.localPosition = new Vector3 (viewRect.center.x, viewRect.center.y, ZPos);
		ApplyZoomAmountToCameraOrthographicSize ();
	}
	private void ApplyZoomAmountToCameraOrthographicSize () {
		float zoomAmount = GetZoomAmountForViewRect (viewRect);
		float targetOrthoSize = orthoSizeNeutral / zoomAmount;
		primaryCamera.orthographicSize = targetOrthoSize;
	}

//	private void UpdateViewRectActual () {
//		viewRect_actual = GetViewRect (this.transform.localPosition, ZoomAmount);
//	}

	public void DarkenScreenForSceneTransition () {
		fullScrim.Show (0.5f);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnPlayerDie(Player player) {
		screenShakeVolumeVel = 0.7f;
//		fullScrim.FadeFromAtoB(Color.clear, new Color(1,1,1, 0.2f), 1f, true);
	}


}



