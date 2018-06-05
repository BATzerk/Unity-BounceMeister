using UnityEngine;
using System.Collections;

[System.Serializable]
public class LevelLinkData {
	// Properties
	private string levelAKey,levelBKey;
	private Vector2 connectingPosA,connectingPosB; // where the START of the SegueStreets are, local to its level.
	private int levelLinkID; // will almost always be 0. Unless we have TWO (or more) connections between the same two levels! In which case, we need to know which connection is which.


	// ================================================================
	//  Constructor
	// ================================================================
	public LevelLinkData(string _levelAKey,string _levelBKey, Vector2 _connectingPosA,Vector2 _connectingPosB, int _levelLinkID) {
		SetLevelAKey(_levelAKey);
		SetLevelBKey(_levelBKey);
		connectingPosA = _connectingPosA;
		connectingPosB = _connectingPosB;
		levelLinkID = _levelLinkID;
	}


	// ================================================================
	//  Getters
	// ================================================================
	public string LevelAKey { get { return levelAKey; } }
	public string LevelBKey { get { return levelBKey; } }
	public Vector2 ConnectingPosA { get { return connectingPosA; } }
	public Vector2 ConnectingPosB { get { return connectingPosB; } }
	public int LevelLinkID { get { return levelLinkID; } }
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
	public Vector2 GetConnectingPosA(string levelKey) { return GetConnectingPosA(IsLevelA(levelKey)); }
	public Vector2 GetConnectingPosB(string levelKey) { return GetConnectingPosB(IsLevelA(levelKey)); }
	public Vector2 GetConnectingPosA(bool isLevelA) { return isLevelA ? connectingPosA : connectingPosB; }
	public Vector2 GetConnectingPosB(bool isLevelA) { return isLevelA ? connectingPosB : connectingPosA; }
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
	public void SetConnectingPos(string levelKey, Vector2 newConnectingPos) {
		SetConnectingPos(IsLevelA(levelKey), newConnectingPos);
	}
	public void SetConnectingPos(bool isLevelA, Vector2 newConnectingPos) {
		if (isLevelA) connectingPosA = newConnectingPos;
		else connectingPosB = newConnectingPos;
	}


}





