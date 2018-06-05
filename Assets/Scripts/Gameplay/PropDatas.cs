using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropData {
}



public class CameraBoundsData : PropData {
	public Rect myRect=new Rect();
//	public CameraBoundsData (Rect myRect) {
//		this.myRect = myRect;
//	}
}
public class GroundData : PropData {
	public Rect myRect;
//	public GroundData (Rect myRect) {
//		this.myRect = myRect;
//	}
}
public class LevelDoorData : PropData {
	public Vector2 pos;
	public string myID;
	public string levelToKey;
	public string levelToDoorID;
//	public LevelDoorData (Vector2 pos, string myID, string levelToKey, string levelToDoorID) {
//		this.pos = pos;
//		this.myID = myID;
//		this.levelToKey = levelToKey;
//		this.levelToDoorID = levelToDoorID;
//	}
}




//public class BoardSpaceData : BoardObjectData {
//	public bool isPlayable = true;
//	public BoardSpaceData (int _col,int _row) {
//		boardPos.col = _col;
//		boardPos.row = _row;
//	}
//}
