using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MapEditorNamespace {
public class RoomTileContents : MonoBehaviour {
	// Constants
    static private readonly Vector2 BatteryIconSize = new Vector2(2,2);
    static private readonly Vector2 GemIconSize = new Vector2(3,3);
    static private readonly Vector2 SnackIconSize = new Vector2(8,8);
	// Components
	[SerializeField] private GameObject go_openings=null; // room-openings sprites.
	[SerializeField] private GameObject go_props=null;
	[SerializeField] private RoomTileDesignerFlag designerFlag=null;
	[SerializeField] private SpriteMask propsMask=null;
    [SerializeField] private TextMesh t_roomName=null; // what's my name, again?
    private List<SpriteRenderer> srs_grounds=new List<SpriteRenderer>();
    private List<SpriteRenderer> srs_openings;
    // Properties
    private bool hasInitializedContent = false;
    // References
    private List<GroundData> groundDatas = new List<GroundData>();
    private RoomData myRD;
    private RoomTile myRoomTile;

    // Getters (Private)
    private ResourcesHandler rh { get { return ResourcesHandler.Instance; } }
    private MapEditorSettings editorSettings { get { return myRoomTile.MapEditor.MySettings; } }
    private float mapScale { get { return myRoomTile.MapEditor.MapScale; } }


    // ================================================================
    //  Initialize
    // ================================================================
    public void Initialize (RoomTile _roomTile) {
		myRoomTile = _roomTile;
        myRD = myRoomTile.MyRoomData;
        t_roomName.GetComponent<Renderer>().sortingOrder = 110; // render roomName over its contents.

        designerFlag.UpdateDesignerFlagButtonVisuals();
	}
	private void InitializeContent () {
		// Set the mask's pos/size!
		propsMask.transform.localPosition = myRD.BoundsLocalBL.center;
		GameUtils.SizeSpriteMask (propsMask, myRD.Size);

		// Set text string!
		t_roomName.text = myRD.RoomKey;
		t_roomName.transform.localPosition = new Vector2(myRD.BoundsLocalBL.xMin, myRD.BoundsLocalBL.yMin); // bottom-left align.
//		if (GameManagers.Instance.DataManager.mostRecentlySavedRoom_worldIndex == worldDataRef.WorldIndex && GameManagers.Instance.DataManager.mostRecentlySavedRoom_roomKey==roomTileRef.RoomKey) {
//			roomNameText.color = new Color(1, 0.8f, 0.2f); // If I'm the most recently saved room, make me stand out! :)
//		}

        AddOpeningsSprites();
        AddPropSprites();

        RefreshAllVisuals();

        // Yes, we have! :)
        hasInitializedContent = true;
    }
    private void AddPropSprites() {
		foreach (PropData propData in myRD.allPropDatas) {
            // -- Batteries --
            if (propData.GetType() == typeof(BatteryData)) {
                BatteryData pd = propData as BatteryData;
                AddSpriteRenderer("Battery",rh.s_battery, go_props, pd.pos, BatteryIconSize, 10, Color.white);
            }
			// -- Grounds --
			else if (propData.GetType() == typeof(GroundData)) {
				GroundData pd = propData as GroundData;
                groundDatas.Add(pd); // also add it to my ref list!
				srs_grounds.Add(AddSpriteRenderer("Ground", rh.s_ground, go_props, pd.pos, pd.size, 1, Color.white));//WHY POSITION? why not center?
			}
			// -- DispGrounds --
			else if (propData.GetType() == typeof(DispGroundData)) {
                DispGroundData pd = propData as DispGroundData;
				Color color = DispGround.GetBodyColor(pd);
                color = new Color(color.r,color.g,color.b, color.a*0.6f); // alpha it out a bit, to taste.
                AddSpriteRenderer("DispGround", rh.s_ground, go_props, pd.pos, pd.size, 1, color);
			}
            // -- Gems --
            else if (propData.GetType() == typeof(GemData)) {
                GemData pd = propData as GemData;
                Sprite sprite = rh.GetGemSprite(pd.type);
                AddSpriteRenderer("Gem",sprite, go_props, pd.pos, GemIconSize, 10, Color.white);
            }
            // -- Snacks --
            else if (propData.GetType() == typeof(SnackData)) {
                SnackData pd = propData as SnackData;
                Color color = PlayerBody.GetBodyColorNeutral(PlayerTypeHelper.TypeFromString(pd.playerType));
                AddSpriteRenderer("Snack",rh.s_snack, go_props, pd.pos, SnackIconSize, 10, color);
            }
			// -- Spikes --
			else if (propData.GetType() == typeof(SpikesData)) {
				SpikesData spikesData = propData as SpikesData;
                Color color = Colors.Spikes(myRD.WorldIndex);// new Color(0.7f,0.1f,0f, 0.6f);
				SpriteRenderer newSprite = AddSpriteRenderer("Spikes", rh.s_spikes, go_props, spikesData.pos, Vector2.one, 0, color);
				newSprite.drawMode = SpriteDrawMode.Tiled;
				newSprite.size = spikesData.size;
				newSprite.transform.localEulerAngles = new Vector3(0, 0, spikesData.rotation);
			}
		}
	}

	private SpriteRenderer AddSpriteRenderer(string goName, Sprite sprite, GameObject parentGO, Vector2 pos, Vector2 size, int sortingOrder, Color color) {
		GameObject iconGO = new GameObject ();
		SpriteRenderer sr = iconGO.AddComponent<SpriteRenderer> ();
		sr.name = goName;
		sr.sprite = sprite;
        GameUtils.ParentAndReset(sr.gameObject, parentGO.transform);
		sr.transform.localPosition = pos;
		GameUtils.SizeSpriteRenderer (sr, size);
		sr.sortingOrder = sortingOrder;
		sr.color = color;
		return sr;
	}

	//public void SetTextPosY(float yPos) {
	//	t_roomName.transform.localPosition = new Vector3 (t_roomName.transform.localPosition.x, yPos, t_roomName.transform.localPosition.z);
	//}
    private void AddOpeningsSprites() {
        srs_openings = new List<SpriteRenderer>();
        for (int i=0; i<myRD.Openings.Count; i++) {
            AddOpeningsSprite(myRD.Openings[i]);
        }
    }
    private void AddOpeningsSprite(RoomOpening ro) {
        string _name = "Opening" + ro.side;
        Vector2 _pos = ro.posCenter;
        Vector2 _size = GetOpeningSpriteSize(ro);
        Sprite _sprite = ResourcesHandler.Instance.s_whiteSquare;
        SpriteRenderer newSprite = AddSpriteRenderer(_name, _sprite, go_openings, _pos,_size, 120, GetOpeningColor(ro));
        srs_openings.Add(newSprite);
    }

    private Vector2 GetOpeningSpriteSize(RoomOpening lo) {
        float thickness = 1f;
        if (lo.side==Sides.L || lo.side==Sides.R) { return new Vector2(thickness, lo.length); }
        return new Vector2(lo.length, thickness);
    }
    public Color GetOpeningColor(RoomOpening ro) {
        return ro.IsRoomTo ? Color.clear : new Color(1,0.3f,0.7f, 0.94f);//new Color(0.4f,1,0, 0.3f)
    }




	//public void Hide () {
	//	this.gameObject.SetActive (false);
	//}
	public void MaybeInitContent() {
		if (!hasInitializedContent) {
			InitializeContent();
		}
	}

	public void SetMaskEnabled(bool isEnabled) {
		SpriteMaskInteraction maskInteraction = isEnabled ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.None;
		SpriteRenderer[] propSprites = go_props.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sr in propSprites) {
			sr.maskInteraction = maskInteraction;
		}
	}
    
	public void OnMapScaleChanged () {
        UpdateRoomNameText();
    }
	
	public void ApplyPosAndSize (Rect rect) {
		designerFlag.ApplyPosAndSize (rect); // Just pass this along to my designerFlag.
	}


    public void RefreshAllVisuals() {
        designerFlag.gameObject.SetActive (editorSettings.DoShowDesignerFlags);
        UpdateRoomNameText();
        go_props.SetActive (editorSettings.DoShowRoomProps);
        RefreshColors();
        SetMaskEnabled(editorSettings.DoMaskRoomContents);
    }
    public void RefreshColors() {
        if (!hasInitializedContent) { return; } // Haven't initted content? Do nothin'.
        // Grounds
        if (editorSettings.DoShowClusters) { // Color ALL by my CLUSTER!
            float s = myRD.isClustStart ? 0.6f : 0.34f;
            Color groundColor;
            if (!myRD.IsInCluster) { groundColor = new ColorHSB(0.2f, 0.05f, 0.4f).ToColor(); } // No Cluster? Gray-ish.
            else { groundColor = new ColorHSB(((20 + myRD.MyCluster.ClustIndex*40)/360f)%1, s, 0.5f).ToColor(); }
            if (myRD.IsSecret) { groundColor = Color.Lerp(groundColor, Color.black, 0.3f); } // Secret? Darker grounds!
            for (int i=0; i<srs_grounds.Count; i++) {
                srs_grounds[i].color = groundColor;
            }
        }
        else { // Otherwise, color EACH how Ground ACTUALLY looks.
            //Color groundColor = new Color(91/255f,107/255f,67/255f, 0.92f);
            for (int i=0; i<srs_grounds.Count; i++) {
                srs_grounds[i].color = Ground.GetBodyColor(groundDatas[i], myRD.WorldIndex);
            }
        }
        // Openings
        for (int i=0; i<srs_openings.Count; i++) {
            srs_openings[i].color = GetOpeningColor(myRD.Openings[i]);
        }
    }
    private void UpdateRoomNameText() {
        t_roomName.fontSize = 40 + (int)(34f/mapScale);
        bool doShowText = mapScale>0.8f && editorSettings.DoShowRoomNames; // If our text is too small to read, don't even show it! (NOTE: Our text will be hardest to read when it's HUGEST, because our RoomTile will be so small on the screen.)
        t_roomName.gameObject.SetActive (doShowText);
    }




}
}




/*
// Make a rect; only what's in this will be rendered!
Rect displayBounds = new Rect(rd.BoundsLocal);
displayBounds.center += displayBounds.size*0.5f; // test
//		displayBounds.center -= rd.PosGlobal;

//		AddSpriteRenderer ("test", s_ground, go_propsLayer, displayBounds.center, displayBounds.size, 99, new Color(0,1,1, 0.5f));

foreach (PropData propData in rd.allPropDatas) {
	// -- Grounds --
	if (propData.GetType() == typeof(GroundData)) {
		GroundData groundData = propData as GroundData;
		Color color = new Color(91/255f,107/255f,67/255f, 0.8f);
		Rect displayRect = new Rect(groundData.myRect);
		//			displayRect.size = new Vector2(Mathf.Min(displayRect.size.x,displayBounds.size.x), Mathf.Min(displayRect.size.y,displayBounds.size.y));
		//			displayRect.center = groundData.myRect.center;

		//			float xMin = Mathf.Max(displayBounds.xMin, displayRect.xMin);
		//			float yMin = Mathf.Max(displayBounds.yMin, displayRect.yMin);
		//			float xMax = Mathf.Min(displayBounds.xMax, displayRect.xMax);
		//			float yMax = Mathf.Min(displayBounds.yMax, displayRect.yMax);
		//			displayRect = new Rect();
		//			displayRect.size = new Vector2(xMax-xMin, yMax-yMin);
		//			displayRect.center = new Vector2((xMax+xMin)*0.5f, (yMax+yMin)*0.5f);
		//
		//			if (displayRect.size.x<=0 || displayRect.size.y<=0) { continue; } // Oh, wow, if this TOTALLY isn't visible, don't add anything.
		AddSpriteRenderer ("Ground", s_ground, go_propsLayer, displayRect.position, displayRect.size, 0, color);//WHY POSITION? why not center?
	}

	*/