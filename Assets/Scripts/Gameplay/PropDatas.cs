﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PropData {
	public float rotation;
	public Vector2 pos;
}

public struct OnOfferData {
    public float durOn;
    public float durOff;
    public float startOffset;
    
    //public OnOfferData() {
    //    durOn = 0.3f;
    //    durOff = 1.7f;
    //    startOffset = 0;
    //}
    public OnOfferData(float durOn,float durOff,float startOffset) {
        this.durOn = durOn;
        this.durOff = durOff;
        this.startOffset = startOffset;
    }
    public OnOfferData(PropOnOffer onOfferScript) {
        if (onOfferScript != null) {
            durOn = onOfferScript.DurOn;
            durOff = onOfferScript.DurOff;
            startOffset = onOfferScript.StartOffset;
        }
        else {
            durOn = 0;
            durOff = 0;
            startOffset = 0;
        }
    }
    //public override string ToString() {
    //    string str = "(";
    //    str += "durOn:" + durOn;
    //    str += ", durOff:" + durOff;
    //    str += ", startOffset:" + startOffset;
    //    str += ")";
    //    return str;
    //}
    public override string ToString() {
        return "(" + durOn + ", " + durOff + ", " + startOffset + ")";
    }
    /// str Example: (durOn:0.6, durOff:1.5, startOffset:0)
    static public OnOfferData FromString(string str) {
        OnOfferData data = new OnOfferData();
        
        //int colon, comma;
        //colon = str.IndexOf (':');
        //comma = str.IndexOf (',');
        //data.durOn       = TextUtils.ParseFloat(str.Substring (colon+1, comma - (colon+1)));
        //colon = str.IndexOf (':', colon+1);
        //comma = str.IndexOf (',', comma+1);
        //data.durOff      = TextUtils.ParseFloat(str.Substring (colon+1, comma - (colon+1)));
        //colon = str.IndexOf (':', colon+1);
        //comma = str.Length - 1;
        //data.startOffset = TextUtils.ParseFloat(str.Substring (colon+1, comma - (colon+1)));
        str = str.Substring(1, str.Length-2); // cut the parenthesis.
        string[] values = str.Split(',');
        data.durOn = TextUtils.ParseFloat(values[0]);
        data.durOff = TextUtils.ParseFloat(values[1]);
        data.startOffset = TextUtils.ParseFloat(values[2]);

        return data;
    }
}


public class BatteryData : PropData {
}
public class BuzzsawData : PropData {
    public Vector2 size;
    public float locOffset = 0;
    public float speed = 1;
    public Vector2 posA = new Vector2(0, 0);
    public Vector2 posB = new Vector2(10, 0);
}
public class CameraBoundsData : PropData {
	public Rect myRect=new Rect();
}
public class CharBarrelData : PropData {
    public string otherCharName;
}
public class EnemyData : PropData {
}
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
	public bool mayPlayerEat=true;
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
	public bool mayBounce=true;
	public bool doRechargePlayer=true;
}
public class PlatformData : BaseGroundData {
    public bool canDropThru=true; // if TRUE, Player can fall through me when pressing down!
}
public class TravelingPlatformData : PlatformData {
    public float locOffset = 0;
    public float speed = 1;
    public Vector2 posA = new Vector2(0, 0);
    public Vector2 posB = new Vector2(10, 0);
}
public class ToggleGroundData : BaseGroundData {
	public bool startsOn;
    public bool togFromContact=false;
    public bool togFromAction=true;
}
public class DamageableGroundData : BaseGroundData {
	public bool doRegen;
	public bool dieFromBounce;
	public bool dieFromPlayerLeave;
    public bool dieFromVel;
    public float regenTime = DamageableGround.RegenTimeDefault;
}
//public class ConditionalGroundData : BaseGroundData {
//	public bool isOffWhenPlungeSpent;
//	public bool isOffWhenBounceRecharged;
//}

public class RoomDoorData : PropData {
	public string myID;
    public int worldToIndex;
	public string roomToKey;
	public string doorToID;
//	public RoomDoorData (Vector2 pos, string myID, string roomToKey, string doorToID) {
//		this.pos = pos;
//		this.myID = myID;
//		this.roomToKey = roomToKey;
//		this.doorToID = doorToID;
//	}
}

public class LaserData : PropData {
    //public Rect myRect=new Rect();
    public OnOfferData onOfferData;
}

public class LiftData : PropData {
    public float strength;
    public Rect myRect=new Rect();
}

public class PlayerData : PropData {
    public PlayerTypes type = PlayerTypes.Undefined;
    public Vector2 vel = Vector2.zero;
    public int dirFacing = 1;
}

public class PlayerStartData : PropData {
}

public class ProgressGateData : BaseGroundData {
    //public int numGemsReq;
    public int numSnacksReq;
}

public class InfoSignData : PropData {
    public string myText="Sign text goes here.";
}
public class SnackData : PropData {
    public string playerType="Plunga";
}

public class SpikesData : PropData {
	public Rect myRect=new Rect();
    public OnOfferData onOfferData;
}

public class VeilData : PropData {
    public Rect myRect=new Rect();
}




//public class BoardSpaceData : BoardObjectData {
//	public bool isPlayable = true;
//	public BoardSpaceData (int _col,int _row) {
//		boardPos.col = _col;
//		boardPos.row = _row;
//	}
//}
