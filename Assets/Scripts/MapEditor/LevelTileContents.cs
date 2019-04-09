using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelTileContents : MonoBehaviour {
	// Constants
	static private readonly Vector2 GemIconSize = new Vector2(2,2);
	// Components
	[SerializeField] private GameObject go_openings=null; // level-openings sprites.
	[SerializeField] private GameObject go_props=null;
	[SerializeField] private LevelTileDesignerFlag designerFlag=null;
	[SerializeField] private SpriteMask propsMask=null;
    private List<SpriteRenderer> srs_grounds=new List<SpriteRenderer>();
    private List<SpriteRenderer> srs_openings;
	// Properties
	private bool hasInitializedContent = false;
	// References
    [SerializeField] private Sprite s_ground=null;
    [SerializeField] private Sprite s_snack=null;
	[SerializeField] private Sprite s_spikes=null;
	[SerializeField] private TextMesh levelNameText=null; // what's my name, again?
    private List<GroundData> groundDatas = new List<GroundData>();
    private LevelData myLD;
    private LevelTile myLevelTile;

    private MapEditorSettings editorSettings { get { return myLevelTile.MapEditor.MySettings; } }


    // ================================================================
    //  Initialize
    // ================================================================
    public void Initialize (LevelTile _levelTile) {
		myLevelTile = _levelTile;
        myLD = myLevelTile.MyLevelData;
        levelNameText.GetComponent<Renderer>().sortingOrder = 110; // render LevelNameText over its contents.

        designerFlag.UpdateDesignerFlagButtonVisuals();
	}
	private void InitializeContent () {
		// Set the mask's pos/size!
		propsMask.transform.localPosition = myLD.BoundsLocal.center;
		GameUtils.SizeSpriteMask (propsMask, myLD.BoundsLocal.size);

		// Set text string!
		levelNameText.text = myLD.LevelKey;
		levelNameText.transform.localPosition = -myLD.BoundsLocal.size*0.5f; // bottom-left align.
//		if (GameManagers.Instance.DataManager.mostRecentlySavedLevel_worldIndex == worldDataRef.WorldIndex && GameManagers.Instance.DataManager.mostRecentlySavedLevel_levelKey==levelTileRef.LevelKey) {
//			levelNameText.color = new Color(1, 0.8f, 0.2f); // If I'm the most recently saved level, make me stand out! :)
//		}

        AddOpeningsSprites();
        AddPropSprites();

        RefreshAllVisuals();

        // Yes, we have! :)
        hasInitializedContent = true;
    }
    private void AddPropSprites() {
		foreach (PropData propData in myLD.allPropDatas) {
			// -- Grounds --
			if (propData.GetType() == typeof(GroundData)) {
				GroundData pd = propData as GroundData;
                groundDatas.Add(pd); // also add it to my ref list!
				srs_grounds.Add(AddSpriteRenderer("Ground", s_ground, go_props, pd.myRect.position, pd.myRect.size, 1, Color.white));//WHY POSITION? why not center?
			}
			// -- DamageableGrounds --
			if (propData.GetType() == typeof(DamageableGroundData)) {
                DamageableGroundData pd = propData as DamageableGroundData;
				Color color = DamageableGround.GetBodyColor(pd);
                color = new Color(color.r,color.g,color.b, color.a*0.6f); // alpha it out a bit, to taste.
                AddSpriteRenderer("DamageableGround", s_ground, go_props, pd.myRect.position, pd.myRect.size, 1, color);
			}
            // -- Gems --
            else if (propData.GetType() == typeof(GemData)) {
                GemData pd = propData as GemData;
                Sprite sprite = ResourcesHandler.Instance.GetGemSprite(pd.type);
                AddSpriteRenderer("Gem",sprite, go_props, pd.pos, GemIconSize, 10, Color.white);
            }
            // -- Snacks --
            else if (propData.GetType() == typeof(SnackData)) {
                SnackData pd = propData as SnackData;
                AddSpriteRenderer("Snack",s_snack, go_props, pd.pos, GemIconSize, 10, Color.white);
            }
			// -- Spikes --
			else if (propData.GetType() == typeof(SpikesData)) {
				SpikesData spikesData = propData as SpikesData;
				SpriteRenderer newSprite = AddSpriteRenderer("Spikes", s_spikes, go_props, spikesData.myRect.position, Vector2.one, 0, new Color(0.7f,0.1f,0f, 0.6f));
				newSprite.drawMode = SpriteDrawMode.Tiled;
				newSprite.size = spikesData.myRect.size;
				newSprite.transform.localEulerAngles = new Vector3(0, 0, spikesData.rotation);
			}
		}
	}

	private SpriteRenderer AddSpriteRenderer(string goName, Sprite sprite, GameObject parentGO, Vector2 pos, Vector2 size, int sortingOrder, Color color) {
		GameObject iconGO = new GameObject ();
		SpriteRenderer sr = iconGO.AddComponent<SpriteRenderer> ();
		sr.name = goName;
		sr.sprite = sprite;
		sr.transform.SetParent (parentGO.transform);
		sr.transform.localPosition = pos;
		GameUtils.SizeSpriteRenderer (sr, size);
		sr.sortingOrder = sortingOrder;
		sr.color = color;
		return sr;
	}

    public void RefreshAllVisuals() {
        designerFlag.gameObject.SetActive (editorSettings.DoShowDesignerFlags);
		levelNameText.gameObject.SetActive (editorSettings.DoShowLevelNames);
		go_props.SetActive (editorSettings.DoShowLevelProps);
        RefreshColors();
		SetMaskEnabled(editorSettings.DoMaskLevelContents);
	}

    // TODO: Fix this positioning.
	public void SetTextPosY (float yPos) {
		levelNameText.transform.localPosition = new Vector3 (levelNameText.transform.localPosition.x, yPos, levelNameText.transform.localPosition.z);
	}
    private void AddOpeningsSprites() {
        srs_openings = new List<SpriteRenderer>();
        for (int i=0; i<myLD.Neighbors.Count; i++) {
            AddOpeningsSprite(myLD.Neighbors[i]);
        }
    }
    private void AddOpeningsSprite(LevelNeighborData ln) {
        LevelOpening lo = ln.OpeningFrom;
        string _name = "Opening" + lo.side;
        Vector2 _pos = lo.posCenter;
        Vector2 _size = GetOpeningSpriteSize(lo);
        Sprite _sprite = ResourcesHandler.Instance.s_whiteSquare;
        SpriteRenderer newSprite = AddSpriteRenderer(_name, _sprite, go_openings, _pos,_size, 120, GetOpeningColor(ln));
        srs_openings.Add(newSprite);
    }

    private Vector2 GetOpeningSpriteSize(LevelOpening lo) {
        float thickness = 1f;
        if (lo.side==Sides.L || lo.side==Sides.R) { return new Vector2(thickness, lo.length); }
        return new Vector2(lo.length, thickness);
    }
    public Color GetOpeningColor(LevelNeighborData ln) {
        return ln.IsLevelTo ? Color.clear : new Color(1,0.3f,0.7f, 0.94f);//new Color(0.4f,1,0, 0.3f)
    }




	public void Hide () {
		this.gameObject.SetActive (false);
	}
	public void Show () {
		if (!hasInitializedContent) {
			InitializeContent ();
		}
	    this.gameObject.SetActive (true);
	}

	public void SetMaskEnabled(bool isEnabled) {
		SpriteMaskInteraction maskInteraction = isEnabled ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.None;
		SpriteRenderer[] propSprites = go_props.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sr in propSprites) {
			sr.maskInteraction = maskInteraction;
		}
	}


	public void OnMapScaleChanged (float mapScale) {
		levelNameText.fontSize = 40 + (int)(34f/mapScale);
		// If our text is too small to read, don't even show it! (NOTE: Our text will be hardest to read when it's HUGEST, because our LevelTile will be so small on the screen.)
		if (mapScale < 0.8f) {//levelNameText.fontSize > 800) {
			levelNameText.gameObject.SetActive (false);
		}
		else {
			levelNameText.gameObject.SetActive (editorSettings.DoShowLevelNames);
		}
	}
	
	public void ApplyPosAndSize (Rect rect) {
		designerFlag.ApplyPosAndSize (rect); // Just pass this along to my designerFlag.
	}

    public void RefreshColors() {
        if (!hasInitializedContent) { return; } // Haven't initted content? Do nothin'.
        // Grounds
        if (editorSettings.DoShowClusters) { // Color ALL by my CLUSTER!
            float s = myLD.isClustStart ? 0.6f : 0.34f;
            Color groundColor = new ColorHSB((20 + myLD.ClusterIndex*60)/255f, s, 0.5f).ToColor();
            for (int i=0; i<srs_grounds.Count; i++) {
                srs_grounds[i].color = groundColor;
            }
        }
        else { // Otherwise, color EACH how Ground ACTUALLY looks.
            //Color groundColor = new Color(91/255f,107/255f,67/255f, 0.92f);
            for (int i=0; i<srs_grounds.Count; i++) {
                srs_grounds[i].color = Ground.GetBodyColor(groundDatas[i]);
            }
        }
        // Openings
        for (int i=0; i<srs_openings.Count; i++) {
            srs_openings[i].color = GetOpeningColor(myLD.Neighbors[i]);
        }
    }




}




/*
// Make a rect; only what's in this will be rendered!
Rect displayBounds = new Rect(ld.BoundsLocal);
displayBounds.center += displayBounds.size*0.5f; // test
//		displayBounds.center -= ld.PosGlobal;

//		AddSpriteRenderer ("test", s_ground, go_propsLayer, displayBounds.center, displayBounds.size, 99, new Color(0,1,1, 0.5f));

foreach (PropData propData in ld.allPropDatas) {
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