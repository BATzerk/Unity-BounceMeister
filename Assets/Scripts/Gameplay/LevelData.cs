using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LevelData {
//	// Constants
//	public static readonly LevelData undefined = new LevelData(0, "undefined");
	// Components!
	public CameraBoundsData cameraBoundsData;
	public List<PropData> allPropDatas;
	// Properties
	public bool hasPlayerBeenHere; // false until the player enters me!
	public bool isConnectedToStart; // true if I'm connected to the starting level of this world. Used to determine to render me on zooming the camera out.
	public string levelKey; // everything we use to reference this level! Including the level's file name (minus the .txt suffix).
	public int designerFlag; // for the level designer! We can flag any level to be like "testing" or "good" or etc.
	public int worldIndex; // which world. w1 is index 1.
	public bool WasUsedInSearchAlgorithm { get; set; }
	public Vector2 posGlobal; // my position, global to ALL worlds! These values get big (up to around 70,000)!

	// Getters
	public string LevelKey { get { return levelKey; } }
	public int DesignerFlag { get { return designerFlag; } }
	public int WorldIndex { get { return worldIndex; } }
	public Rect BoundsLocal { get { return new Rect(cameraBoundsData.myRect); } } // Currently, the camera bounds and level bounds are one in the same.
	public Rect BoundsGlobal { get { return new Rect(cameraBoundsData.myRect.center+posGlobal, cameraBoundsData.myRect.size); } }
	public Vector2 PosGlobal { get { return posGlobal; } }
//	public Rect BoundsLocal { get { return boundsLocal; } }
	public WorldData WorldDataRef { get { return GameManagers.Instance.DataManager.GetWorldData(worldIndex); } }
    /// Returns the closest PlayerStart to the provided pos.
	public Vector2 PlayerStartPos(Vector2 playerPos) {
        float bestDist = Mathf.Infinity;
        Vector2 bestPos = Vector2.zero;
		foreach (PropData pd in allPropDatas) {
			if (pd is PlayerStartData) {
                float dist = Vector2.Distance(playerPos, pd.pos);
                if (bestDist > dist) {
                    bestDist = dist;
                    bestPos = pd.pos;
                }
			}
		}
		return bestPos;
	}

	// Setters
	public void SetPosGlobal (Vector2 _posGlobal, bool doUpdateLevelFile) {
		// Set it, AND update my globalBounds (which have changed, hmm!)!
		posGlobal = new Vector2 (Mathf.Round(_posGlobal.x*0.5f)*2f, Mathf.Round (_posGlobal.y*0.5f)*2f); // Round my posGlobal values to even numbers... because that's what I did in Linelight. :p
		if (doUpdateLevelFile) {
			// Update the file!
			LevelSaverLoader.UpdateLevelPropertiesInLevelFile(this);
		}
	}
	public void SetDesignerFlag (int _designerFlag, bool doUpdateLevelFile) {
		if (designerFlag != _designerFlag) {
			designerFlag = _designerFlag;
			if (doUpdateLevelFile) { // Update the file!
				LevelSaverLoader.UpdateLevelPropertiesInLevelFile(this);
			}
		}
	}
//	public void SetPosWorld (Vector2 _posWorld) {
//		posWorld = _posWorld;
//	}


	// ================================================================
	//  Initialize
	// ================================================================
	public LevelData (int _worldIndex, string _key) { //, Vector2 defaultAbsolutePos) {
		worldIndex = _worldIndex;
		levelKey = _key;
		// Initialize all my PropData lists.
		ClearAllPropDataLists ();
	}


	public void ClearAllPropDataLists () {
		allPropDatas = new List<PropData>();
		cameraBoundsData = new CameraBoundsData();
	}






}






