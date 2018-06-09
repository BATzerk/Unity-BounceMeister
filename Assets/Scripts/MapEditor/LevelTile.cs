using UnityEngine;
using System.Collections;

public class LevelTile : MonoBehaviour {
	// References
	private LevelData levelDataRef;
	private MapEditor mapEditorRef;
	// Properties
	private bool isDragReadyMouseOverMe; // true if the mouse is over me AND able to click and drag LevelTiles.
	private bool isWithinLevelTileSelectionRect; // true if the LevelTileSelectionRect is over me!
	private bool isLevelLinkViewSelectedOverMePrimary=false; // true when the CURRENT connectionCircle being dragged is over me!
	private bool isLevelLinkViewSelectedOverMeSecondary=false; // true when the OTHER connectionCircle of levelLinkViewSelected is over me!
	private bool isSelected; // true when we're clicked on to be dragged about!
//	private Rect myRect; // the clickable and displaying area.
//	private float backingXMin, backingXMax, backingYMin, backingYMax;
	private Vector3 mouseClickOffset;
	// Components
	[SerializeField] private LevelTileBodyCollider bodyCollider=null;
	[SerializeField] private LevelTileContents contents=null;
	[SerializeField] private SpriteRenderer sr_backing=null; // my whole placemat thing.
	[SerializeField] private SpriteRenderer sr_border=null; // borders still look nice.

	
	
	// ================================================================
	//  Getters / Setters
	// ================================================================
//	public float BackingXMin { get { return backingXMin; } }
//	public float BackingXMax { get { return backingXMax; } }
//	public float BackingYMin { get { return backingYMin; } }
//	public float BackingYMax { get { return backingYMax; } }
	public int WorldIndex { get { return levelDataRef.WorldIndex; } }
	public string LevelKey { get { return levelDataRef.LevelKey; } }
	//	public Rect PlacematRect { get { return new Rect (x-w*0.5f,y-h*0.5f, w,h); } }
	public Rect BoundsGlobal { get { return levelDataRef.BoundsGlobal; } }
	public Rect BoundsLocal { get { return levelDataRef.BoundsLocal; } }
	public bool IsLevelLinkViewSelectedOverMePrimary {
		get { return isLevelLinkViewSelectedOverMePrimary; }
		set {
			isLevelLinkViewSelectedOverMePrimary = value;
			UpdateBorderLine();
		}
	}
	public bool IsLevelLinkViewSelectedOverMeSecondary {
		get { return isLevelLinkViewSelectedOverMeSecondary; }
		set {
			isLevelLinkViewSelectedOverMeSecondary = value;
			UpdateBorderLine();
		}
	}
	private Vector2 Pos { get { return levelDataRef.PosGlobal; } }
	public LevelData LevelDataRef { get { return levelDataRef; } }
	public bool IsDragReadyMouseOverMe {
		get { return isDragReadyMouseOverMe; }
		set {
			if (isDragReadyMouseOverMe == value) { return; }
			isDragReadyMouseOverMe = value;
			UpdateBorderLine();
		}
	}
	public bool IsWithinLevelTileSelectionRect {
		get { return isWithinLevelTileSelectionRect; }
		set {
			if (isWithinLevelTileSelectionRect == value) { return; }
			isWithinLevelTileSelectionRect = value;
			UpdateBorderLine();
		}
	}
	public void SetMouseClickOffset(Vector3 _mousePosWorld) {
		mouseClickOffset = this.transform.localPosition - _mousePosWorld;
	}
	
	
	// ================================================================
	//  Initialize
	// ================================================================
	public void Initialize (MapEditor _mapEditorRef, LevelData _levelDataRef) {
		mapEditorRef = _mapEditorRef;
		levelDataRef = _levelDataRef;
		this.gameObject.name = "LevelTile " + levelDataRef.levelKey;
//		SetPosAndSizeValues ();

		this.transform.localScale = Vector3.one;

		contents.Initialize (this);
		
		UpdateComponentVisibilities ();

		ApplyPosition ();
		ApplySize ();

		OnDeselected ();
		IsDragReadyMouseOverMe = false;

		// Hide by default
		HideContents ();
	}

//	private void SetPosAndSizeValues () {
//		Rect bounds = levelDataRef.BoundsGlobal;// MathUtils.GetMinScreenRectangle (levelDataRef.BoundsLocalWithoutSegueStreets, Vector2.zero);
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
		// If I'm a candidate for a LevelLinkView being dragged, update my borderLine!
		if (isLevelLinkViewSelectedOverMePrimary) {
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

		Color backingColor = new Color(0.3f,0.3f,0.3f);//Colors.GetBGColor_ViewGameplay (Colors.GetBGTheme (levelDataRef.WorldIndex));
		backingColor = new Color (backingColor.r*0.5f, backingColor.g*0.5f, backingColor.b*0.5f, 0.5f);
		sr_backing.color = backingColor;
	}
	public void ShowContents () {
		this.gameObject.SetActive(true);// TEMP!! TOtally hiding.
		contents.Show ();
		contents.OnMapScaleChanged(mapEditorRef.MapScale);
		
		Color backingColor = new Color(0.3f,0.3f,0.3f);//Colors.GetBGColor_ViewGameplay (Colors.GetBGTheme (levelDataRef.WorldIndex));
		backingColor = new Color (backingColor.r*1.4f, backingColor.g*1.4f, backingColor.b*1.4f, 0.2f);
		sr_backing.color = backingColor;

		UpdateComponentVisibilities ();
	}

	public void UpdateVisibilityFromSearchCriteria (string searchString) {
		bool isInSearch = false;
		// If there is no search, consider me in "the search!"
		if (searchString == "" || searchString == null) {
			isInSearch = true;
		}
		// If my name's got this search string in it, then yeah!
//		if (levelDataRef.LevelKey.Contains (searchString)) {
		if (levelDataRef.LevelKey.ToLower().Contains (searchString.ToLower())) {
			isInSearch = true;
		}
		// Go ahead and enable/disable the WHOLE thing, mate.
		this.gameObject.SetActive (isInSearch);
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
		// levelNameText
		contents.SetTextPosY (0);//MyRect.size.y*0.5f);
	}
	
	
	public void UpdateComponentVisibilities () {
		contents.UpdateComponentVisibilities ();
	}

	public void UpdateBorderLine() {
		// Selected!
		if (isSelected || isWithinLevelTileSelectionRect) {
			sr_border.color = new Color(1,0.8f,0f);
		}
		// BOTH LevelLinkView circles are over me!
		else if (isLevelLinkViewSelectedOverMePrimary && isLevelLinkViewSelectedOverMeSecondary) {
			sr_border.color = Color.clear;
		}
		// Currently dragged LevelLinkView circle is over me!
		else if (isLevelLinkViewSelectedOverMePrimary) {
			float alpha = 0.7f + Mathf.Sin(Time.time*8)*0.4f;
			sr_border.color = new Color(1,0.8f,0f, alpha);
		}
		// Other (non-dragged) LevelLinkView circle is over me!
		else if (isLevelLinkViewSelectedOverMeSecondary) {
			sr_border.color = new Color(1,0.8f,0f, 0.4f);
		}
		// Drag-ready mouse over me!
		else if (mapEditorRef.CanSelectALevelTile() && isDragReadyMouseOverMe) {
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
	public void OnMapScaleChanged (float mapScale) {
		contents.OnMapScaleChanged (mapScale);
	}
	public void OnMouseEnterBodyCollider() {
		if (mapEditorRef.CanSelectALevelTile()) {
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
			mapEditorRef.OnClickLevelTile (this);
		}
		
		// Dragging me??
		if (isSelected && mapEditorRef.IsDraggingSelectedLevelTiles()) {
			// Update my LevelData's PosGlobal!!
			Vector2 newPosGlobal = mapEditorRef.MousePosWorldDraggingGrid(mouseClickOffset);
			levelDataRef.SetPosGlobal (newPosGlobal, false);
			ApplyPosition();
		}
	}




}







