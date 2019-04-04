using UnityEngine;
using System.Collections;

/** NOTE: UNUSED. Artifact from Linelight. */
[System.Serializable]
public class LevelLinkData {
	// Properties
	private string levelAKey,levelBKey;


	// ================================================================
	//  Constructor
	// ================================================================
	public LevelLinkData(string _levelAKey,string _levelBKey) {
		SetLevelAKey(_levelAKey);
		SetLevelBKey(_levelBKey);
	}


	// ================================================================
	//  Getters
	// ================================================================
	public string LevelAKey { get { return levelAKey; } }
	public string LevelBKey { get { return levelBKey; } }
	public bool IsLevelA(string levelKey) {
		if (levelAKey == levelKey) return true;
		if (levelBKey == levelKey) return false;
		Debug.LogError("ERROR. We're asking if a level is A or B, but we've given C as a value: " + levelKey);
		return false;
	}
	//  string Key(string levelKey) {       return Key(IsLevelA(levelKey)); } // NOTE: Commented out because this function would literally return the exact string we give it.
	public string Key(bool isLevelA) {      return isLevelA ? levelAKey : levelBKey; }
	public string OtherKey(string levelKey) {  return OtherKey(IsLevelA(levelKey)); }
	public string OtherKey(bool isLevelA) { return isLevelA ? levelBKey : levelAKey; }
	public bool DoesLinkLevel(string levelKey) {
		return levelAKey==levelKey || levelBKey==levelKey;
	}
	public bool DoesLinkLevels(string KeyA, string KeyB) {
		// Are both these keys the same as my two keys (order irrelevant)?
		if (levelAKey == KeyA && levelBKey == KeyB) return true;
		if (levelAKey == KeyB && levelBKey == KeyA) return true;
		return false;
	}


	// ================================================================
	//  Setters
	// ================================================================
	public void SetLevelAKey(string _levelAKey) { levelAKey = _levelAKey; }
	public void SetLevelBKey(string _levelBKey) { levelBKey = _levelBKey; }


}





