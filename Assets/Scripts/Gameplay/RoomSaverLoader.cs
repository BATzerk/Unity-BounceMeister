using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;


static public class RoomSaverLoader {
	// Constants (Properties)
	const string ROOM_PROPERTIES = "roomProperties";
	// Constants (Objects)
	const string BATTERY = "Battery";
    const string BUZZSAW = "Buzzsaw";
	const string CAMERA_BOUNDS = "CameraBounds";
    const string CHAR_BARREL = "CharBarrel";
    const string CHAR_UNLOCK_ORB = "CharUnlockOrb";
	const string CRATE = "Crate";
	const string DISP_GROUND = "DispGround";
    const string DWEEB = "Dweeb";
    const string GATE = "Gate";
	const string GATE_BUTTON = "GateButton";
	const string GEM = "Gem";
	const string GROUND = "Ground";
	const string ROOM_DOOR = "RoomDoor";
    const string LASER = "Laser";
    const string LIFT = "Lift";
    const string PLATFORM = "Platform";
	const string PLAYER_START = "PlayerStart";
    const string PROGRESS_GATE = "ProgressGate";
    const string INFOSIGN = "InfoSign";
    const string SNACK = "Snack";
    const string SPIKES = "Spikes";
    const string TOGGLE_GROUND = "ToggleGround";
    const string TURRET = "Turret";
    const string VEIL = "Veil";
	// Properties
	private static string debug_roomDataLoadingRoomKey; // purely for printing to the console without having to pass this parameter through a chain of functions


	// ================================================================
	//  Getters
	// ================================================================
	static private bool IsStringAnAffectName(string str) {
		return str.IndexOf(":", StringComparison.InvariantCulture) == -1; // If the line DOESN'T feature a colon (the characteristic of property lines), it's an "affect"!
	}
	static private string[] GetRoomFileAsStringArray(int worldIndex, string roomKey) {
		string filePath = FilePaths.WorldFileAddress (worldIndex) + roomKey + ".txt";
		if (File.Exists(filePath)) {
			StreamReader file = File.OpenText(filePath);
			string wholeFile = file.ReadToEnd();
			file.Close();
			return TextUtils.GetStringArrayFromStringWithLineBreaks(wholeFile, StringSplitOptions.None);
		}
		else {
			Debug.LogError("Room file not found! World " + worldIndex + ", roomKey " + roomKey + "\nfilePath: \"" + filePath + "\"");
			return null;
		}
	}

	private static string GetRoomPropertiesLine(RoomData rd) {
		string returnString = ROOM_PROPERTIES + " ";
		returnString += "posGlobal:" + rd.PosGlobal;
		returnString += ";designerFlag:" + rd.DesignerFlag;
        if (rd.isClustStart) { returnString += ";isClustStart:" + rd.isClustStart; }
        if (rd.IsSecret) { returnString += ";isSecret:" + rd.IsSecret; }
		return returnString;
	}


	// ================================================================
	//  Saving
	// ================================================================
	static private string fs; // Messy. I don't like how this is out here. This could be avoided if we were able to use anonymous functions.
    static public void SaveRoomFile (Room r) { SaveRoomFile (r.ToData()); }
    static public void SaveRoomFile (RoomData rd) { SaveRoomFileAs (rd, rd.WorldIndex, rd.RoomKey); }
    static public void SaveRoomFileAs(RoomData rd, int worldIndex,string roomKey) {
		fs = ""; // fileString. this guy will be packed with \n line-breaks, then at the very end split by \n. It's less code to look at.
        
        // Alphabetize the props list, so all Grounds are clumped together, then Platforms, etc.
        rd.allPropDatas.Sort( (a,b) => string.Compare(a.GetType().FullName, b.GetType().FullName, StringComparison.Ordinal));

		// Room Properties
		AddFSLine(GetRoomPropertiesLine(rd));

        fs += GetPropName(rd.cameraBoundsData) + " ";
        fs += GetPropPropertiesLine(rd.cameraBoundsData);
        AddFSLineBreak();

		foreach (PropData propData in rd.allPropDatas) {
            propData.rotation = Mathf.Round(propData.rotation); // Round rotation so it's not like "90.000001".
            
            fs += GetPropName(propData) + " ";
            fs += GetPropPropertiesLine(propData);
            AddFSLineBreak();
		}


		string[] roomFileArray = fs.Split('\n');

		// Save it!
		SaveRoomFileFromStringArray(worldIndex, roomKey, roomFileArray);

		// We're done saving! Now that we've got the updated saved text file, reload the corresponding RoomData.
		rd.MyWorldData.ReloadRoomData(roomKey);

//		// Update who the most recently created room is!
//		GameManagers.Instance.DataManager.mostRecentlySavedRoom_worldIndex = WorldIndex;
//		GameManagers.Instance.DataManager.mostRecentlySavedRoom_roomKey = roomKey;
	}
    

    static private string GetPropName(PropData data) {
        string dataName = data.GetType().ToString();
        return dataName.Substring(0, dataName.Length-4); // cut out the "Data" part!
    }
    static private string GetPropPropertiesLine(PropData propData) {
        List<string> propertyNames = GetPropPropertyNamesToSave(propData);
        return GetPropFieldsAsString(propData, propertyNames);
    }
    
    static private List<string> GetPropPropertyNamesToSave(PropData propData) {
        List<string> ps = new List<string>(); // property names!
        // YES travelMind?? Add that!
        if (propData.travelMind.IsUsed) { ps.Add("travelMind"); }
        // NO TravelMind?? Add pos!
        else { ps.Add("pos"); }
        if (!Mathf.Approximately(propData.rotation, 0)) { ps.Add("rotation"); }
        // BaseGroundData?
        if (propData is BaseGroundData) {
            ps.Add("size");
            BaseGroundData d = propData as BaseGroundData;
            if (!d.mayPlayerEat) { ps.Add("mayPlayerEat"); }
            if (d.isPlayerRespawn) { ps.Add("isPlayerRespawn"); }
            if (d.preventHappyHop) { ps.Add("preventHappyHop"); }
        }
        
        Type type = propData.GetType();
        if (type == typeof(BatteryData)) { }
        else if (type == typeof(BuzzsawData)) { ps.Add("size"); }
        else if (type == typeof(CameraBoundsData)) { ps.Add("size"); }
        else if (type == typeof(CharBarrelData)) { ps.Add("otherCharName"); }
        else if (type == typeof(CharUnlockOrbData)) { ps.Add("myCharName"); }
        else if (type == typeof(CrateData)) { ps.Add("hitsUntilBreak"); ps.Add("numCoinsInMe"); }
        else if (type == typeof(GateData)) { ps.Add("channelID"); }
        else if (type == typeof(GateButtonData)) { ps.Add("channelID"); }
        else if (type == typeof(GemData)) { ps.Add("type"); }
        else if (type == typeof(InfoSignData)) { ps.Add("myText"); }
        else if (type == typeof(LiftData)) { ps.Add("size"); ps.Add("strength"); }
        else if (type == typeof(PlayerStartData)) { }
        else if (type == typeof(ProgressGateData)) { ps.Add("numSnacksReq"); }
        else if (type == typeof(RoomDoorData)) { ps.Add("myID"); ps.Add("worldToIndex"); ps.Add("roomToKey"); ps.Add("doorToID"); }
        else if (type == typeof(SnackData)) { ps.Add("playerType"); }
        else if (type == typeof(VeilData)) { ps.Add("size"); }
        // Enemies
        else if (type == typeof(DweebData)) { ps.Add("speed"); }
        // Props with optional params
        else if (type == typeof(TurretData)) {
            TurretData d = propData as TurretData;
            ps.Add("interval");
            ps.Add("speed");
            if (d.startOffset > 0) { ps.Add("startOffset"); }
        }
        else if (type == typeof(SpikesData)) {
            SpikesData d = propData as SpikesData;
            ps.Add("size");
            if (d.onOffer.IsUsed) { ps.Add("onOffer"); }
        }
        else if (type == typeof(LaserData)) {
            LaserData d = propData as LaserData;
            if (d.onOffer.IsUsed) { ps.Add("onOffer"); }
        }
        else if (type == typeof(DispGroundData)) {
            DispGroundData d = propData as DispGroundData;
            ps.Add("doRegen");
            if (d.regenTime!=DispGround.RegenTimeDefault) { ps.Add("regenTime"); }
            if (d.dieFromBounce) { ps.Add("dieFromBounce"); }
            if (d.dieFromPlayerLeave) { ps.Add("dieFromPlayerLeave"); }
            if (d.dieFromVel) { ps.Add("dieFromVel"); }
        }
        else if (type == typeof(GroundData)) {
            GroundData d = propData as GroundData;
            if (d.isBouncy) { ps.Add("isBouncy"); }
            if (!d.mayBounce) { ps.Add("mayBounce"); }
            if (!d.doRechargePlayer) { ps.Add("doRechargePlayer"); }
        }
        else if (type == typeof(PlatformData)) {
            PlatformData d = propData as PlatformData;
            if (!d.canDropThru) { ps.Add("canDropThru"); }
        }
        else if (type == typeof(ToggleGroundData)) {
            ps.Add("startsOn");
            ps.Add("togFromContact");
            ps.Add("togFromAction");
        }
        else {
            Debug.LogWarning("Prop in Room not recognized for serialization: " + type);
        }
        return ps;
    }

	///** Use this when we want to tack on optional params after, within the same line. */
	//static private void AddPropFieldsToFS(PropData data, params string[] fieldNames) {
	//	fs += GetPropFieldsAsString(data, fieldNames);
	//}
	///** Use this when this prop has no optional params. */
	//static private void AddAllPropFieldsToFS(PropData data, params string[] fieldNames) {
	//	// Create and add the line to fileString!
	//	string propName = GetPropName(data);
	//	AddFS (propName + " " + GetPropFieldsAsString(data, fieldNames));
	//}
    //static private void AddOnOfferDataToFS(OnOfferData data) {
    //    if (data.durOff > 0) { // If there IS an OnOffer, tack on its properties to the whole string.
    //        AddFS(";durOn:"+data.durOn);
    //        AddFS(";durOff:"+data.durOff);
    //        AddFS(";startOffset:"+data.startOffset);
    //    }
    //}
    /// Returns e.g. "pos:(0,10);size:(8,1);strength:4"
    static private string GetPropFieldsAsString(PropData data, List<string> fieldNames) {
        string str = "";
        Type classType = data.GetType();
        for (int i=0; i<fieldNames.Count; i++) {
            FieldInfo fieldInfo = GetFieldInfoFromClass (classType, fieldNames[i]);
            if (fieldInfo == null) { // Safety check.
                Debug.LogWarning("We're trying to save a prop with a field its class (or superclasses) doesn't have: " + classType + ", " + fieldNames[i]);
                continue;
            }
            str += fieldNames[i] + ":"; // start it with, like, "startPos:"
            str += GetFieldValueAsString(fieldInfo, data); // Add the actual value!
            if (i<fieldNames.Count-1) { str += ";"; } // Semicolon to separate each field.
        }
        return str;
    }
	//static private string GetPropFieldsAsString(PropData data, params string[] fieldNames) {
	//	// Prepare the string to be added.
	//	string returnString = "";
	//	Type classType = data.GetType ();
	//	for (int i=0; i<fieldNames.Length; i++) {
	//		FieldInfo fieldInfo = GetFieldInfoFromClass (classType, fieldNames[i]);
	//		// Don't proceed if this field doesn't exist.
	//		if (fieldInfo == null) {
	//			Debug.LogWarning("We're trying to save a prop with a field its class (or superclasses) doesn't have: " + classType + ", " + fieldNames[i]);
	//			continue;
	//		}
	//		returnString += fieldNames[i] + ":"; // start it with, like, "startPos:"
	//		returnString += GetFieldValueAsString(fieldInfo, data); // Add the actual value!
	//		if (i<fieldNames.Length-1) { // If this is NOT the last value, add a semicolon to separate us from the next value!
	//			returnString += ";";
	//		}
	//	}
	//	return returnString;
	//}
	static private FieldInfo GetFieldInfoFromClass(Type classType, string fieldName) {
		const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
		FieldInfo fieldInfo = classType.GetField(fieldName, flags);

		// If this class DIDN'T have the requested field, ask its base/super class!
		if (fieldInfo == null) {
			Type superclassType = classType.BaseType;
			if (superclassType != null) {
				return GetFieldInfoFromClass(superclassType, fieldName);
			}
		}
		// Return the found fieldInfo! It MAY be null if we weren't able to find this field in any of this class's super-classes.
		return fieldInfo;
	}
	static private string GetFieldValueAsString(FieldInfo fieldInfo, PropData data) {
		return fieldInfo.GetValue (data).ToString ();
	}
	// This function is shorthand, because "pos" is such a common value among props.
	//	static void AddPropPosFieldToFS (Prop prop) {
	//		Vector3 pos = prop.transform.localPosition;
	//		AddPropFieldToFS("pos", pos.x+","+pos.y);
	//	}
	// This function adds one property to the current prop being saved in our fileString.
	//	static private void AddPropFieldToFS(string fieldName, string fieldValue) {
	//		fs += fieldName + ":" + fieldValue + ";";
	//	}
	static private void AddFS(string stringToAdd) {
		fs += stringToAdd;
	}
	static private void AddFSLineBreak() { AddFSLine (""); }
	static private void AddFSLine(string stringToAdd) {
		AddFS(stringToAdd + "\n");
	}
//	static private void AddFSLineHeader(string stringToAdd) {
//		// If this ISN'T the first line in fs, then add a line break. (We don't want a line break to start the file off.)
//		if (fs.IndexOf("\n") != -1) {
//			fs += "\n";
//		}
//		AddFSLine(stringToAdd);
//	}

	static private void SaveRoomFileFromStringArray(int worldIndex, string roomKey, string[] roomFileArray) {
		// Otherwise, SAVE! :D
		string filePath = FilePaths.RoomFile(worldIndex, roomKey);
		try {
			StreamWriter sr = File.CreateText (filePath);
			foreach (string lineString in roomFileArray) {
				sr.WriteLine (lineString);
			}
			sr.Close();
			Debug.Log("SAVED ROOM " + roomKey);

			GameManagers.Instance.EventManager.OnEditorSaveRoom();

			//// Reload the text file right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
			//#if UNITY_EDITOR
			//UnityEditor.AssetDatabase.ImportAsset(filePath);
			//#endif
		}
		catch (Exception e) {
			Debug.LogError ("Error saving a room to a text file: " + roomKey + ". Save location: " + filePath + ". Error string: " + e.ToString ());
		}

//		// Finally! Delete the saved snapshot of this room completely! If we don't, a number of odd behaviors can occur from the data conflict. One notable effect is when changing posGlobal: carts' joints will be all wonky (because they were saved in a different part of the world).
//		GameplaySnapshotController.DeleteRoomSnapshotFromSaveStorage (WorldIndex, roomKey);
//		// Also delete the saved snapshot of the player if it's in this exact room.
//		GameplaySnapshotController.DeletePlayerDataSnapshotFromSaveStorageIfInRoomStatic (WorldIndex, roomKey);
	}


	public static void UpdateRoomPropertiesInRoomFile(RoomData rd) {
		// Load the file!
		String[] roomFileArray = GetRoomFileAsStringArray(rd.WorldIndex, rd.RoomKey);
		// No file? No cheese.
		if (roomFileArray == null) { return; }
		// Look through the file for the line to overwrite.
		for (int i=0; i<roomFileArray.Length; i++) {
			String lineString = roomFileArray[i];
			// This is the line!!
			if (lineString.StartsWith(ROOM_PROPERTIES, StringComparison.InvariantCulture)) {
				// Replace the properties line!
				roomFileArray[i] = GetRoomPropertiesLine(rd);
				break;
			}
		}
		// Remove any extra line breaks at the end.
		TextUtils.RemoveExcessLineBreaksFromStringArray (ref roomFileArray);
		// Now, re-save the whole thing!
		SaveRoomFileFromStringArray(rd.WorldIndex, rd.RoomKey, roomFileArray);
	}




	static public void LoadRoomDataFromItsFile (RoomData rd) {
		// First, make empty buckets of all PropDatas.
		rd.ClearAllPropDataLists ();

		// Load the file!
		string[] roomFile = GetRoomFileAsStringArray(rd.WorldIndex, rd.RoomKey);

		// NULL room file...
		if (roomFile == null) {
			AddEmptyRoomElements(ref rd);
		}
		// There IS a room file!...
		else {
			debug_roomDataLoadingRoomKey = rd.RoomKey; // for printing to console.
			foreach (string lineString in roomFile) {
				if (lineString == "") continue; // If this line is EMPTY, skip it!
				int affectNameEndIndex = lineString.IndexOf(' ');
				if (affectNameEndIndex==-1) { continue; } // Wrong formatting!
				string affectName = lineString.Substring(0, affectNameEndIndex);
				string propertiesString = lineString.Substring(affectNameEndIndex+1);
				// Room Properties
				if (affectName == ROOM_PROPERTIES) {
					SetRoomPropertyFieldValuesFromFieldsString (rd, propertiesString);
				}
				// Props!
				else {
					PropData propData = GetNewPropDataFromAffectName(affectName);
					if (propData == null) { // Safety check.
						Debug.LogError ("Oops! Unidentifiable text in room file: " + rd.RoomKey + ". Text: \"" + lineString + "\"");
						continue;
					}
					SetPropDataFieldValuesFromFieldsString (propData, propertiesString);
					rd.AddPropData(propData);
				}
			}
		}
    }
    static private PropData GetNewPropDataFromAffectName(string affectName) {
        switch (affectName) {
            case DWEEB: return new DweebData();
            case BATTERY: return new BatteryData();
            case BUZZSAW: return new BuzzsawData();
            case CAMERA_BOUNDS: return new CameraBoundsData();
            case CHAR_BARREL: return new CharBarrelData();
            case CHAR_UNLOCK_ORB: return new CharUnlockOrbData();
            case CRATE: return new CrateData();
            case DISP_GROUND: return new DispGroundData();
            case GATE: return new GateData();
            case GATE_BUTTON: return new GateButtonData();
            case GEM: return new GemData();
            case GROUND: return new GroundData();
            case ROOM_DOOR: return new RoomDoorData();
            case LASER: return new LaserData();
            case LIFT: return new LiftData();
            case PLATFORM: return new PlatformData();
            case PLAYER_START: return new PlayerStartData();
            case PROGRESS_GATE: return new ProgressGateData();
            case INFOSIGN: return new InfoSignData();
            case SNACK: return new SnackData();
            case SPIKES: return new SpikesData();
            case TOGGLE_GROUND: return new ToggleGroundData();
            case TURRET: return new TurretData();
            case VEIL: return new VeilData();
            default: return null;
        }
    }
	static public void AddEmptyRoomElements(ref RoomData rd) {
		CameraBoundsData cameraBoundsData = new CameraBoundsData();
		cameraBoundsData.size = new Vector2(52, 38);
		cameraBoundsData.pos = Vector2.zero;
        rd.cameraBoundsData = cameraBoundsData;
        rd.AddPropData(cameraBoundsData);

		PlayerStartData playerStartData = new PlayerStartData();
		playerStartData.pos = new Vector2(0,0);
        rd.AddPropData(playerStartData);

		Rect[] groundRects = {
            new Rect(0,17.5f, 52,3), // top
            new Rect(0,-16.5f, 52,5), // bottom
            new Rect(-24,0, 4,38), // left
            new Rect(24,0, 4,38), // right
		};
		foreach (Rect rect in groundRects) {
            GroundData newGroundData = new GroundData {
                pos = rect.position,
                size = rect.size,
            };
            rd.AddPropData(newGroundData);
		}
	}

	static private void SetRoomPropertyFieldValuesFromFieldsString(RoomData rd, string fieldsString) {
		// Divide the long string of ALL fields into an array, each slot just ONE field.
		string[] fieldStrings = fieldsString.Split (';');
		for (int i=0; i<fieldStrings.Length; i++) {
			// Divide this one field and its value into a two-part array: first element is the NAME, second element is the VALUE.
			string[] nameAndValue = fieldStrings[i].Split(':');
			string name = nameAndValue[0];
			string value = nameAndValue[1];
			try {
				if (name == "posGlobal") {
					rd.SetPosGlobal(TextUtils.GetVector2FromString(value));
				}
				else if (name == "designerFlag") {
					rd.SetDesignerFlag(TextUtils.ParseInt(value));
				}
                else if (name == "isClustStart") {
                    rd.isClustStart = TextUtils.ParseBool(value);
                }
                else if (name == "isSecret") {
                    rd.SetIsSecret(TextUtils.ParseBool(value));
                }
			}
			catch (Exception e) {
				Debug.LogError ("Room file has a parameter we don't recognize: " + rd.RoomKey + ", " + name + ", " + value + ". " + e.ToString ());
			}
		}
	}

	static private void SetPropDataFieldValuesFromFieldsString(PropData propData, string fieldsString) {
		// Divide the long string of ALL fields into an array, each slot just ONE field.
		string[] fieldStrings = fieldsString.Split (';');
		for (int i=0; i<fieldStrings.Length; i++) {
			// Divide this one field and its value into a two-part array: first element is the NAME, second element is the VALUE.
			int colonIndex = fieldStrings[i].IndexOf (':');
			if (colonIndex < 0) { // This string isn't legit. Whoopsies!
				Debug.LogError ("Invalid line in room file: " + debug_roomDataLoadingRoomKey + ". \"" + fieldStrings[i] + "\"");
				continue;
			}
			string name = fieldStrings[i].Substring (0, colonIndex);
			string value = fieldStrings[i].Substring (colonIndex+1);
			// Set this specified field for this PropData!
			SetPropDataFieldValue (propData, name,value);
		}
	}

	static private void SetPropDataFieldValue(PropData propData, string fieldName, string fieldValueString) {
		// What extension of PropData is this?
		Type propDataType = propData.GetType ();
		// Get the FieldInfo of the requested name from this propData's class.
		FieldInfo fieldInfo = propDataType.GetField(fieldName);
		// Get the VALUE of this field from the string!
		if (fieldInfo == null) {
		    Debug.LogError("We've been provided an unidentified prop field type. " + debug_roomDataLoadingRoomKey + ". PropData: " + propData + ", fieldName: " + fieldName);
		}
		else if (fieldInfo.FieldType == typeof(bool)) {
			fieldInfo.SetValue(propData, bool.Parse(fieldValueString));
		}
		else if (fieldInfo.FieldType == typeof(float)) {
			fieldInfo.SetValue(propData, TextUtils.ParseFloat(fieldValueString));
		}
		else if (fieldInfo.FieldType == typeof(int)) {
			fieldInfo.SetValue(propData, TextUtils.ParseInt(fieldValueString));
		}
		else if (fieldInfo.FieldType == typeof(string)) {
			fieldInfo.SetValue(propData, fieldValueString);
		}
		else if (fieldInfo.FieldType == typeof(Rect)) {
			fieldInfo.SetValue(propData, TextUtils.GetRectFromString(fieldValueString));
		}
		else if (fieldInfo.FieldType == typeof(Vector2)) {
			fieldInfo.SetValue(propData, TextUtils.GetVector2FromString(fieldValueString));
		}
        else if (fieldInfo.FieldType == typeof(OnOfferData)) {
            fieldInfo.SetValue(propData, OnOfferData.FromString(fieldValueString));
        }
        else if (fieldInfo.FieldType == typeof(TravelMindData)) {
            fieldInfo.SetValue(propData, TravelMindData.FromString(fieldValueString));
        }
        else {
            Debug.LogWarning("Unrecognized field type in Room file: " + debug_roomDataLoadingRoomKey + ". PropData: " + propData + ", fieldName: " + fieldName);
        }
	}

    
    static public bool MayRenameRoomFile(Room room, string newName) { return MayRenameRoomFile(room.MyRoomData, newName); }
    static public bool MayRenameRoomFile(RoomData rd, string newName) {
        if (string.IsNullOrEmpty(newName)) { return false; } // Empty name? Nah.
        if (rd.RoomKey == newName) { return false; } // Same name? Nah.
        string newPath = FilePaths.RoomFile(rd.WorldIndex, newName);
        if (File.Exists(newPath)) { return false; } // File already here? Nah.
        // Looks good!
        return true;
    }
    static public void RenameRoomFile(Room room, string newName) { RenameRoomFile(room.MyRoomData, newName); }
    static public void RenameRoomFile(RoomData rd, string newName) {
        string oldPath = FilePaths.RoomFile(rd.WorldIndex, rd.RoomKey);
        string newPath = FilePaths.RoomFile(rd.WorldIndex, newName);
        // File already here with this name?
        if (File.Exists(newPath)) {
            Debug.LogWarning("Can't rename room. Already a file here: \"" + newPath + "\"");
        }
        else {
            File.Move(oldPath, newPath);
        }
    }


}


