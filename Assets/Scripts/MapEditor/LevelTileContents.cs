using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelTileContents : MonoBehaviour {
	// Constants
	static private readonly Vector2 GemIconSize = new Vector2(2,2);
	// Components
	[SerializeField] private GameObject go_propsLayer=null;
	[SerializeField] private LevelTileDesignerFlag designerFlag=null;
	// Properties
	private bool hasInitializedContent = false;
	// References
	private LevelTile levelTileRef;
//	private WorldData worldDataRef;
	[SerializeField] private Sprite s_gem=null;
	[SerializeField] private Sprite s_ground=null;
	[SerializeField] private Sprite s_spikes=null;
	[SerializeField] private TextMesh levelNameText=null; // what's my name, again?


	// ================================================================
	//  Initialize
	// ================================================================
	public void Initialize (LevelTile _levelTileRef) {
		levelTileRef = _levelTileRef;
		this.transform.localPosition = Vector3.zero;
		this.transform.localScale = Vector3.one;
		
		designerFlag.UpdateDesignerFlagButtonVisuals ();
	}
	private void InitializeContent () {
		LevelData ld = levelTileRef.LevelDataRef;

		// Set text string!
		levelNameText.text = ld.LevelKey;
		levelNameText.transform.localPosition = -ld.BoundsLocal.size*0.5f; // bottom-left align.
//		if (GameManagers.Instance.DataManager.mostRecentlySavedLevel_worldIndex == worldDataRef.WorldIndex && GameManagers.Instance.DataManager.mostRecentlySavedLevel_levelKey==levelTileRef.LevelKey) {
//			levelNameText.color = new Color(1, 0.8f, 0.2f); // If I'm the most recently saved level, make me stand out! :)
//		}

		// Make a rect; only what's in this will be rendered!
		Rect displayBounds = new Rect(ld.BoundsLocal);
		displayBounds.center += displayBounds.size*0.5f; // test
//		displayBounds.center -= ld.PosGlobal;

//		AddSpriteRenderer ("test", s_ground, go_propsLayer, displayBounds.center, displayBounds.size, 99, new Color(0,1,1, 0.5f));

		foreach (PropData propData in ld.allPropDatas) {
			// -- Grounds --
			if (propData.GetType() == typeof(GroundData)) {
				GroundData groundData = propData as GroundData;
				Color color = new Color(0.4f,0.4f,0.4f, 0.8f);
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
			// -- Gems --
			else if (propData.GetType() == typeof(GemData)) {
				GemData gemData = propData as GemData;
				AddSpriteRenderer ("Gem", s_gem, go_propsLayer, gemData.pos, GemIconSize, 0, Color.white);
			}
			// -- Spikes --
			else if (propData.GetType() == typeof(SpikesData)) {
				SpikesData spikesData = propData as SpikesData;
				SpriteRenderer newSprite = AddSpriteRenderer ("Spikes", s_spikes, go_propsLayer, spikesData.myRect.position, Vector2.one, 0, new Color(0.7f,0.1f,0f, 0.6f));
				newSprite.drawMode = SpriteDrawMode.Tiled;
				newSprite.size = spikesData.myRect.size;
				newSprite.transform.localEulerAngles = new Vector3(0, 0, spikesData.rotation);
			}
		}

		UpdateComponentVisibilities ();

		// Yes, we have! :)
		hasInitializedContent = true;
	}

	private SpriteRenderer AddSpriteRenderer(string goName, Sprite sprite, GameObject parentGO, Vector2 pos, Vector2 size, int sortingOrder, Color color) {
		GameObject iconGO = new GameObject ();
		SpriteRenderer newIcon = iconGO.AddComponent<SpriteRenderer> ();
		newIcon.name = goName;
		newIcon.sprite = sprite;
		newIcon.transform.SetParent (parentGO.transform);
		newIcon.transform.localPosition = pos;
		GameUtils.SizeSpriteRenderer (newIcon, size);
		newIcon.sortingOrder = sortingOrder;
		newIcon.color = color;
		return newIcon;
	}

	public void UpdateComponentVisibilities () {
		designerFlag.gameObject.SetActive (MapEditorSettings.DoShowDesignerFlags);
		levelNameText.gameObject.SetActive (MapEditorSettings.DoShowLevelNames);
		go_propsLayer.SetActive (MapEditorSettings.DoShowLevelProps);
	}

	public void SetTextPosY (float yPos) {
		levelNameText.transform.localPosition = new Vector3 (levelNameText.transform.localPosition.x, yPos, levelNameText.transform.localPosition.z);
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


	public void OnMapScaleChanged (float mapScale) {
		levelNameText.fontSize = 40 + (int)(34f/mapScale);
		// If our text is too small to read, don't even show it! (NOTE: Our text will be hardest to read when it's HUGEST, because our LevelTile will be so small on the screen.)
		if (mapScale < 0.8f) {//levelNameText.fontSize > 800) {
			levelNameText.gameObject.SetActive (false);
		}
		else {
			levelNameText.gameObject.SetActive (MapEditorSettings.DoShowLevelNames);
		}
	}
	
	public void ApplyPosAndSize (Rect rect) {
		designerFlag.ApplyPosAndSize (rect); // Just pass this along to my designerFlag.
	}




}





