using System.Collections;
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
public class GateData : BaseGroundData {
	public int channelID;
}
public class GateButtonData : PropData {
	public int channelID;
}

public class GemData : PropData {
    public int type=0;
}

public class BaseGroundData : PropData {
	public Rect myRect;
	public bool canEatGems=true;
    public bool isPlayerRespawn=false;
    public Rect MyRectTLAligned() {
        return new Rect(myRect.position-myRect.size*0.5f, myRect.size);
    }
}
public class CrateData : BaseGroundData {
	public int hitsUntilBreak;
	public int numCoinsInMe;
}
public class GroundData : BaseGroundData {
    public bool isBouncy=false;
	public bool canBounce=true;
	public bool doRechargePlayer=true;
	public int colorType;
}
public class PlatformData : BaseGroundData {
    public bool canDropThru=true; // if TRUE, Player can fall through me when pressing down!
}
public class ToggleGroundData : BaseGroundData {
	public bool startsOn;
}
public class DamageableGroundData : BaseGroundData {
	public bool doRegen;
	public bool dieFromBounce;
	public bool dieFromPlayerLeave;
    public bool dieFromVel;
}
//public class ConditionalGroundData : BaseGroundData {
//	public bool isOffWhenPlungeSpent;
//	public bool isOffWhenBounceRecharged;
//}

public class LevelDoorData : PropData {
	public string myID;
    public int worldToIndex;
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

public class ProgressGateData : BaseGroundData {
    //public int numGemsReq;
    public int numSnacksReq;
}

public class SnackData : PropData {
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
