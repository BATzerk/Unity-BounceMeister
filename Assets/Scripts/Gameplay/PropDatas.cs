﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropData {
	public float rotation;
	public Vector2 pos;
}



public class BatteryData : PropData {
}
public class CameraBoundsData : PropData {
	public Rect myRect=new Rect();
}
//public class EnemyData : PropData {
//}
public class GemData : PropData {
}

public class BaseGroundData : PropData {
	public Rect myRect;
}
public class CrateData : BaseGroundData {
	public int hitsUntilBreak;
	public int numCoinsInMe;
}
public class GroundData : BaseGroundData {
	public bool doRechargePlayer=true;
	public int colorType;
}
public class PlatformData : BaseGroundData {
}
public class ToggleGroundData : BaseGroundData {
	public bool startsOn;
}
public class DamageableGroundData : BaseGroundData {
	public bool dieFromBounce;
	public bool dieFromPlayerLeave;
	public bool dieFromVel;
	public bool doRegen;
}
//public class ConditionalGroundData : BaseGroundData {
//	public bool isOffWhenPlungeSpent;
//	public bool isOffWhenBounceRecharged;
//}

public class LevelDoorData : PropData {
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

public class LiftData : PropData {
	public float strength;
	public Rect myRect=new Rect();
}

public class PlayerData : PropData {
}

public class PlayerStartData : PropData {
}

public class SpikesData : PropData {
	public Rect myRect=new Rect();
}




//public class BoardSpaceData : BoardObjectData {
//	public bool isPlayable = true;
//	public BoardSpaceData (int _col,int _row) {
//		boardPos.col = _col;
//		boardPos.row = _row;
//	}
//}
