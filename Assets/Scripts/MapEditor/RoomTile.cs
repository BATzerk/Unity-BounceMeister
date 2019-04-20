using UnityEngine;
using System.Collections;

namespace MapEditorNamespace {
public class RoomTile : MonoBehaviour {
	// Properties
	private bool isDragReadyMouseOverMe; // true if the mouse is over me AND able to click and drag RoomTiles.
	private bool isWithinRoomTileSelectionRect; // true if the RoomTileSelectionRect is over me!
	private bool isRoomLinkViewSelectedOverMePrimary=false; // true when the CURRENT connectionCircle being dragged is over me!
	private bool isRoomLinkViewSelectedOverMeSecondary=false; // true when the OTHER connectionCircle of roomLinkViewSelected is over me!
	private bool isSelected; // true when we're clicked on to be dragged about!
//	private Rect myRect; // the clickable and displaying area.
//	private float backingXMin, backingXMax, backingYMin, backingYMax;
	private Vector3 mouseClickOffset;
	// Components
	[SerializeField] private RoomTileBodyCollider bodyCollider=null;
	[SerializeField] private RoomTileContents contents=null;
	[SerializeField] private SpriteRenderer sr_backing=null; // my whole placemat thing.
	[SerializeField] private SpriteRenderer sr_border=null; // borders still look nice.
	// References
    public RoomData MyRoomData { get; private set; }
	public MapEditor MapEditor { get; private set; }



    // ================================================================
    //  Getters / Setters
    // ================================================================
    //	public float BackingXMin { get { return backingXMin; } }
    //	public float BackingXMax { get { return backingXMax; } }
    //	public float BackingYMin { get { return backingYMin; } }
    //	public float BackingYMax { get { return backingYMax; } }
    public RoomTileBodyCollider BodyCollider { get { return bodyCollider; } }
    public int WorldIndex { get { return MyRoomData.WorldIndex; } }
	public string RoomKey { get { return MyRoomData.RoomKey; } }
	//	public Rect PlacematRect { get { return new Rect (x-w*0.5f,y-h*0.5f, w,h); } }
	public Rect BoundsGlobal { get { return MyRoomData.BoundsGlobal; } }
	public Rect BoundsLocal { get { return MyRoomData.BoundsLocal; } }
	public bool IsRoomLinkViewSelectedOverMePrimary {
		get { return isRoomLinkViewSelectedOverMePrimary; }
		set {
			isRoomLinkViewSelectedOverMePrimary = value;
			UpdateBorderLine();
		}
	}
	public bool IsRoomLinkViewSelectedOverMeSecondary {
		get { return isRoomLinkViewSelectedOverMeSecondary; }
		set {
			isRoomLinkViewSelectedOverMeSecondary = value;
			UpdateBorderLine();
		}
	}
	private Vector2 Pos { get { return MyRoomData.PosGlobal; } }
    public bool IsDragReadyMouseOverMe {
		get { return isDragReadyMouseOverMe; }
		set {
			if (isDragReadyMouseOverMe == value) { return; }
			isDragReadyMouseOverMe = value;
			UpdateBorderLine();
		}
	}
	public bool IsWithinRoomTileSelectionRect {
		get { return isWithinRoomTileSelectionRect; }
		set {
			if (isWithinRoomTileSelectionRect == value) { return; }
			isWithinRoomTileSelectionRect = value;
			UpdateBorderLine();
		}
	}
	public void SetMouseClickOffset(Vector3 _mousePosWorld) {
		mouseClickOffset = this.transform.localPosition - _mousePosWorld;
	}
	
	
	// ================================================================
	//  Initialize
	// ================================================================
	public void Initialize (MapEditor _mapEditorRef, RoomData _myRoomData, Transform tf_parent) {
		MapEditor = _mapEditorRef;
		MyRoomData = _myRoomData;
        GameUtils.ParentAndReset(this.gameObject, tf_parent);
		this.gameObject.name = "RoomTile " + MyRoomData.roomKey;
//		SetPosAndSizeValues ();

		contents.Initialize (this);
		
		RefreshAllVisuals ();

		ApplyPosition ();
		ApplySize ();

		OnDeselected ();
		IsDragReadyMouseOverMe = false;

		// Hide by default
		HideContents ();
	}

//	private void SetPosAndSizeValues () {
//		Rect bounds = roomDataRef.BoundsGlobal;// MathUtils.GetMinScreenRectangle (roomDataRef.BoundsLocalWithoutSegueStreets, Vector2.zero);
//		backingXMin = bounds.xMin;
//		backingXMax = bounds.xMax;
//		backingYMin = bounds.yMin;
//		backingYMax = bounds.yMax;
////		x = (backingXMin + backingXMax) * 0.5f;
////		y = (backingYMin + backingYMax) * 0.5f;
////		w = backingXMax - backingXMin;
////		h = backingYMax - backingYMin;
////		myRect = new Rect(x,y, w,h);
//		myRect = bounds;
//	}
	
	
	
	// ================================================================
	//  Update
	// ================================================================
	private void Update() {
		RegisterMouseInput ();
		// If I'm a candidate for a RoomLinkView being dragged, update my borderLine!
		if (isRoomLinkViewSelectedOverMePrimary) {
			UpdateBorderLine();
		}
	}
	
	// ================================================================
	//  Update Components
	// ================================================================
	public void HideContents () {
		this.gameObject.SetActive(false);// TEMP!! TOtally hiding.
		contents.Hide ();
		IsDragReadyMouseOverMe = false; // Deselect me from mouse-over just in case.

		Color backingColor = new Color(0.3f,0.3f,0.3f);//Colors.GetBGColor_ViewGameplay (Colors.GetBGTheme (roomDataRef.WorldIndex));
		backingColor = new Color (backingColor.r*0.5f, backingColor.g*0.5f, backingColor.b*0.5f, 0.5f);
		sr_backing.color = backingColor;
	}
	public void ShowContents() {
		this.gameObject.SetActive(true);// TEMP!! TOtally hiding.
		contents.Show();
		contents.OnMapScaleChanged(MapEditor.MapScale);
		
		Color backingColor = new Color(0.3f,0.3f,0.3f);//Colors.GetBGColor_ViewGameplay (Colors.GetBGTheme (roomDataRef.WorldIndex));
		backingColor = new Color (backingColor.r*1.4f, backingColor.g*1.4f, backingColor.b*1.4f, 0.2f);
		sr_backing.color = backingColor;

		RefreshAllVisuals ();
	}

	public void UpdateVisibilityFromSearchCriteria (string searchString) {
		bool isInSearch = false;
		// If there is no search, consider me in "the search!"
		if (searchString == "" || searchString == null) {
			isInSearch = true;
		}
		// If my name's got this search string in it, then yeah!
//		if (roomDataRef.RoomKey.Contains (searchString)) {
		if (MyRoomData.RoomKey.ToLower().Contains (searchString.ToLower())) {
			isInSearch = true;
		}
		// Update visuals!
        bodyCollider.SetIsEnabled(isInSearch);
        contents.gameObject.SetActive(isInSearch);//TEMP TEST
	}
//	public void RemakeWallLines() {
//		contents.CreateWallLines (); // Go ahead and pass this baton on to my content.
//	}
	private void ApplyPosition() {
		this.transform.localPosition = Pos;
	}
	private void ApplySize() {
		// backing and border
		GameUtils.SizeSpriteRenderer (sr_backing, BoundsLocal.size.x,BoundsLocal.size.y);
		sr_border.size = BoundsLocal.size;
		sr_backing.transform.localPosition = BoundsLocal.center;
		sr_border.transform.localPosition = BoundsLocal.center;
		// bodyCollider
		bodyCollider.UpdatePosAndSize (BoundsLocal);
		// Contents may be hot
		contents.ApplyPosAndSize (BoundsLocal);
		// roomNameText
		contents.SetTextPosY (0);//MyRect.size.y*0.5f);
	}
	
	
	public void RefreshAllVisuals() {
		contents.RefreshAllVisuals();
	}
    public void RefreshColors() {
        contents.RefreshColors();
    }

	public void UpdateBorderLine() {
		// Selected!
		if (isSelected || isWithinRoomTileSelectionRect) {
			sr_border.color = new Color(1,0.8f,0f);
		}
		// BOTH RoomLinkView circles are over me!
		else if (isRoomLinkViewSelectedOverMePrimary && isRoomLinkViewSelectedOverMeSecondary) {
			sr_border.color = Color.clear;
		}
		// Currently dragged RoomLinkView circle is over me!
		else if (isRoomLinkViewSelectedOverMePrimary) {
			float alpha = 0.7f + Mathf.Sin(Time.time*8)*0.4f;
			sr_border.color = new Color(1,0.8f,0f, alpha);
		}
		// Other (non-dragged) RoomLinkView circle is over me!
		else if (isRoomLinkViewSelectedOverMeSecondary) {
			sr_border.color = new Color(1,0.8f,0f, 0.4f);
		}
		// Drag-ready mouse over me!
		else if (MapEditor.CanSelectARoomTile() && isDragReadyMouseOverMe) {
			sr_border.color = new Color(1,0.8f,0f);
		}
		// Not being dragged nor over.
		else {
			sr_border.color = new Color(1,1,1, 0.1f);
		}
	}


	
	
	// ================================================================
	//  Events
	// ================================================================
	public void OnMapScaleChanged() {
		contents.OnMapScaleChanged(MapEditor.MapScale);
	}
	public void OnMouseEnterBodyCollider() {
		if (MapEditor.CanSelectARoomTile()) {
			IsDragReadyMouseOverMe = true;
		}
	}
	public void OnMouseExitBodyCollider() {
		IsDragReadyMouseOverMe = false;
	}

	public void OnSelected(Vector3 _mousePosWorld) {
		isSelected = true;
		SetMouseClickOffset (_mousePosWorld);
		UpdateBorderLine ();
	}
	public void OnDeselected() {
		isSelected = false;
		UpdateBorderLine ();
	}


	private void RegisterMouseInput() {
		// Clicked me??
		if (isDragReadyMouseOverMe && Input.GetMouseButtonDown (0)) {
			MapEditor.OnClickRoomTile(this);
		}
		
		// Dragging me??
		if (isSelected && MapEditor.IsDraggingSelectedRoomTiles()) {
			// Update my RoomData's PosGlobal!!
			Vector2 newPosGlobal = MapEditor.MousePosWorldDraggingGrid(mouseClickOffset);
			MyRoomData.SetPosGlobal(newPosGlobal);
			ApplyPosition();
		}
	}




}
}






