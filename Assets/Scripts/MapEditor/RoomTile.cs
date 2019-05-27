using UnityEngine;
using System.Collections;

namespace MapEditorNamespace {
public class RoomTile : MonoBehaviour {
	// Properties
	public bool IsMouseOverMe { get; private set; }
	private bool isInSelectionRect; // true if my center's inside the RoomTileSelectionRect!
	public bool IsSelected { get; private set; } // true when we're clicked on to be dragged about!
//	private Rect myRect; // the clickable and displaying area.
//	private float backingXMin, backingXMax, backingYMin, backingYMax;
	private Vector3 mouseClickOffset;
	// Components
	[SerializeField] private RoomTileBodyCollider bodyCollider=null;
	[SerializeField] private RoomTileContents contents=null;
	[SerializeField] private SpriteRenderer sr_backing=null; // my whole placemat thing.
	[SerializeField] private SpriteRenderer sr_border=null; // borders still look nice.
	// References
    [SerializeField] private Sprite s_borderThick=null;
    [SerializeField] private Sprite s_borderThin=null;
    public RoomData MyRoomData { get; private set; }
    public MapEditor MapEditor { get; private set; }



    // ================================================================
    //  Getters / Setters
    // ================================================================
    public bool IsFullyVisible { get { return this.gameObject.activeInHierarchy && bodyCollider.IsEnabled; } }
    public RoomTileBodyCollider BodyCollider { get { return bodyCollider; } }
    public int WorldIndex { get { return MyRoomData.WorldIndex; } }
	public string RoomKey { get { return MyRoomData.RoomKey; } }
	//	public Rect PlacematRect { get { return new Rect (x-w*0.5f,y-h*0.5f, w,h); } }
	public Rect BoundsGlobal { get { return MyRoomData.BoundsGlobal; } }
	public Rect BoundsLocal { get { return MyRoomData.BoundsLocal; } }
	private Vector2 Pos { get { return MyRoomData.PosGlobal; } }
 //   public bool IsDragReadyMouseOverMe {
	//	get { return isDragReadyMouseOverMe; }
	//	private set {
	//		isDragReadyMouseOverMe = value;
	//		UpdateBorderColor();
	//	}
	//}
	public bool IsInSelectionRect {
		get { return isInSelectionRect; }
		set {
			isInSelectionRect = value;
			UpdateBorderColor();
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
		this.gameObject.name = "RoomTile " + MyRoomData.RoomKey;
//		SetPosAndSizeValues ();

		contents.Initialize (this);
		
		RefreshAllVisuals ();

		ApplyPosition ();
		ApplySize ();

		OnDeselected ();

		// Hide by default
		Hide();
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
        // If I'm selected, oscillate my border color!
        if (IsSelected) {
            UpdateBorderColor();
        }
	}
	
	// ================================================================
	//  Update Components
	// ================================================================
	public void Hide() {
        this.gameObject.SetActive(false);
        //OnDeselected();
        IsMouseOverMe = false;
        //IsDragReadyMouseOverMe = false; // Deselect me from mouse-over just in case.
        //contents.Hide ();

		//Color backingColor = new Color(0.3f,0.3f,0.3f);//Colors.GetBGColor_ViewGameplay (Colors.GetBGTheme (roomDataRef.WorldIndex));
		//backingColor = new Color (backingColor.r*0.5f, backingColor.g*0.5f, backingColor.b*0.5f, 0.5f);
		//sr_backing.color = backingColor;
	}
	public void Show() {
        this.gameObject.SetActive(true);
        contents.MaybeInitContent(MapEditor.MapScale);

		RefreshAllVisuals ();
	}

	public void UpdateVisibilityFromSearchCriteria (string searchString) {
		bool isInSearch = false;
		// If there is no search, consider me in "the search!"
		if (string.IsNullOrEmpty(searchString)) {
			isInSearch = true;
		}
		// If my name's got this search string in it, then yeah!
//		if (roomDataRef.RoomKey.Contains (searchString)) {
		if (MyRoomData.RoomKey.ToLower().Contains (searchString.ToLower())) {
			isInSearch = true;
		}
		// Update visuals!
        bodyCollider.SetIsEnabled(isInSearch);
        contents.gameObject.SetActive(isInSearch);
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
		//// roomNameText
		//contents.SetTextPosY (0);//MyRect.size.y*0.5f);
	}
	
	
	public void RefreshAllVisuals() {
        // Backing
        Color backingColor = new Color(0.3f,0.3f,0.3f);//Colors.GetBGColor_ViewGameplay (Colors.GetBGTheme (roomDataRef.WorldIndex));
        backingColor = new Color (backingColor.r*1.4f, backingColor.g*1.4f, backingColor.b*1.4f, 0.2f);
        if (MyRoomData.IsSecret) { backingColor = new Color(0,0,0, 0.4f); } // Secret? Darker back!
        sr_backing.color = backingColor;
        // Contents
		contents.RefreshAllVisuals();
	}
    public void RefreshColors() {
        contents.RefreshColors();
    }

	public void UpdateBorderColor() {
        // Drag-ready mouse over me!
        if (IsMouseOverMe) {//MapEditor.CanSelectARoomTile()) {
            sr_border.color = new Color(0.5f,0.95f,1, 0.6f);
            sr_border.sprite = s_borderThick;
        }
        // Selected!
        else if (IsSelected || isInSelectionRect) {
            float alpha = MathUtils.SinRange(0.5f, 1f, Time.time*7 + Pos.x*0.2f+Pos.y*0.2f);
            sr_border.color = new Color(1,0.8f,0f, alpha);
            sr_border.sprite = s_borderThick;
        }
		// Not being dragged nor over.
		else {
			sr_border.color = new Color(1,1,1, 0.1f);
            sr_border.sprite = s_borderThin;
		}
	}


	
	
	// ================================================================
	//  Events
	// ================================================================
	public void OnMapScaleChanged() {
		contents.OnMapScaleChanged(MapEditor.MapScale);
	}
	public void OnMouseEnterBodyCollider() {
		IsMouseOverMe = true;
	}
	public void OnMouseExitBodyCollider() {
		IsMouseOverMe = false;
	}

	public void OnSelected(Vector3 _mousePosWorld) {
		IsSelected = true;
		SetMouseClickOffset (_mousePosWorld);
		UpdateBorderColor ();
	}
	public void OnDeselected() {
		IsSelected = false;
		UpdateBorderColor ();
	}


	private void RegisterMouseInput() {
		//// Clicked me??
		//if (IsMouseOverMe && Input.GetMouseButtonDown (0)) {
		//	MapEditor.OnClickRoomTile(this);
		//}
		
		// Dragging me??
		if (IsSelected && MapEditor.IsDraggingSelectedRoomTiles()) {
			// Update my RoomData's PosGlobal!!
			Vector2 newPosGlobal = MapEditor.MousePosWorldDraggingGrid(mouseClickOffset);
			MyRoomData.SetPosGlobal(newPosGlobal);
			ApplyPosition();
		}
	}




}
}






