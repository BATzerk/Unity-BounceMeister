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
	const string CAMERA_BOUNDS = "CameraBounds";
	const string CHAR_BARREL = "CharBarrel";
	const string CRATE = "Crate";
	const string DAMAGEABLE_GROUND = "DamageableGround";
    const string ENEMY = "Enemy";
    const string GATE = "Gate";
	const string GATE_BUTTON = "GateButton";
	const string GEM = "Gem";
	const string GROUND = "Ground";
	const string ROOM_DOOR = "RoomDoor";
	const string LIFT = "Lift";
    const string PLATFORM = "Platform";
	const string PLAYER_START = "PlayerStart";
    const string PROGRESS_GATE = "ProgressGate";
    const string INFOSIGN = "InfoSign";
    const string SNACK = "Snack";
    const string SPIKES = "Spikes";
    const string TOGGLE_GROUND = "ToggleGround";
    const string TRAVELING_PLATFORM = "TravelingPlatform";
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
    static public void SaveRoomFile (Room r) { SaveRoomFile (r.SerializeAsData()); }
    static public void SaveRoomFile (RoomData rd) { SaveRoomFileAs (rd, rd.WorldIndex, rd.RoomKey); }
	//static public void SaveRoomFileAs (Room r, int worldIndex,string roomKey) {
	//	SaveRoomFileAs(r.SerializeAsData(), worldIndex, roomKey);
 //       //// Update ALL WorldData room stuff (actually just for openings/neighbors atm).
 //       //GameManagers.Instance.DataManager.GetWorldData(worldIndex).SetAllRoomDatasFundamentalProperties();
	//}
	static public void SaveRoomFileAs(RoomData rd, int worldIndex,string roomKey) {
		fs = ""; // fileString. this guy will be packed with \n line-breaks, then at the very end split by \n. It's less code to look at.
        
        // Alphabetize the props list, so all Grounds are clumped together, then Platforms, etc.
        rd.allPropDatas.Sort( (a,b) => string.Compare(a.GetType().FullName, b.GetType().FullName, StringComparison.Ordinal));

		// Room Properties
		AddFSLine (GetRoomPropertiesLine(rd));
		AddAllPropFieldsToFS(rd.cameraBoundsData, "myRect");

		foreach (PropData propData in rd.allPropDatas) {
			Type type = propData.GetType();
			if (type == typeof(BatteryData)) { AddAllPropFieldsToFS(propData, "pos"); }
			else if (type == typeof(CharBarrelData)) { AddAllPropFieldsToFS(propData, "pos", "otherCharName"); }
            else if (type == typeof(EnemyData)) { AddAllPropFieldsToFS(propData, "pos"); }
            else if (type == typeof(GateButtonData)) { AddAllPropFieldsToFS(propData, "pos", "channelID"); }
			else if (type == typeof(GemData)) { AddAllPropFieldsToFS(propData, "pos", "type"); }
            else if (type == typeof(RoomDoorData)) { AddAllPropFieldsToFS(propData, "pos", "myID", "worldToIndex", "roomToKey", "doorToID"); }
            else if (type == typeof(LiftData)) { AddAllPropFieldsToFS(propData, "myRect", "rotation", "strength"); }
			else if (type == typeof(PlayerStartData)) { AddAllPropFieldsToFS(propData, "pos"); }
            else if (type == typeof(SnackData)) { AddAllPropFieldsToFS(propData, "pos", "playerType"); }
            else if (type == typeof(SpikesData)) { AddAllPropFieldsToFS(propData, "myRect", "rotation"); }
            else if (type == typeof(VeilData)) { AddAllPropFieldsToFS(propData, "myRect"); }
			// Props with optional params
			else if (type == typeof(CrateData)) {
				CrateData d = propData as CrateData;
				AddSomePropFieldsToFS(propData, "myRect", "hitsUntilBreak", "numCoinsInMe");
                if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
			}
			else if (type == typeof(DamageableGroundData)) {
				DamageableGroundData d = propData as DamageableGroundData;
				AddSomePropFieldsToFS(propData, "myRect", "doRegen");
                if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                if (d.regenTime!=DamageableGround.RegenTimeDefault) { fs += ";regenTime:" + d.regenTime; }
                if (d.dieFromBounce) { fs += ";dieFromBounce:" + d.dieFromBounce; }
                if (d.dieFromPlayerLeave) { fs += ";dieFromPlayerLeave:" + d.dieFromPlayerLeave; }
                if (d.dieFromVel) { fs += ";dieFromVel:" + d.dieFromVel; }
                AddFSLine();
			}
			else if (type == typeof(GateData)) {
				GateData d = propData as GateData;
				AddSomePropFieldsToFS(propData, "myRect", "channelID");
				if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
			}
			else if (type == typeof(GroundData)) {
				GroundData d = propData as GroundData;
				AddSomePropFieldsToFS(propData, "myRect");
                if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (d.isBouncy) { fs += ";isBouncy:" + d.isBouncy; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                if (!d.canBounce) { fs += ";canBounce:" + d.canBounce; }
                if (!d.doRechargePlayer) { fs += ";doRechargePlayer:" + d.doRechargePlayer; }
                AddFSLine();
			}
            else if (type == typeof(InfoSignData)) {
                InfoSignData d = propData as InfoSignData;
                AddSomePropFieldsToFS(propData, "pos", "myText");
                if (!Mathf.Approximately(d.rotation, 0)) { fs += ";rotation:" + Mathf.Round(d.rotation); }
                AddFSLine();
            }
            else if (type == typeof(PlatformData)) {
                PlatformData d = propData as PlatformData;
                AddSomePropFieldsToFS(propData, "myRect");
                if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (!d.canDropThru) { fs += ";canDropThru:" + d.canDropThru; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
            }
            else if (type == typeof(ProgressGateData)) {
                ProgressGateData d = propData as ProgressGateData;
                AddSomePropFieldsToFS(propData, "myRect", "numSnacksReq");
                if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
            }
            else if (type == typeof(ToggleGroundData)) {
                ToggleGroundData d = propData as ToggleGroundData;
                AddSomePropFieldsToFS(propData, "myRect", "startsOn", "togFromContact", "togFromAction");
                if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
            }
            else if (type == typeof(TravelingPlatformData)) {
                TravelingPlatformData d = propData as TravelingPlatformData;
                AddSomePropFieldsToFS(propData, "myRect", "locOffset", "speed", "posA", "posB");
                if (!d.mayPlayerEat) { fs += ";mayPlayerEat:" + d.mayPlayerEat; }
                if (!d.canDropThru) { fs += ";canDropThru:" + d.canDropThru; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
            }
            else {
                Debug.LogWarning("Prop in Room not recognized for serialization: " + type);
            }
		}


		string[] roomFileArray = fs.Split('\n');

		// Save it!
		SaveRoomFileFromStringArray(worldIndex, roomKey, roomFileArray);

//		// FINALLY, go ahead and update the RoomLinks and the SEPARATE _RoomLinks.txt file. :)
//		for (int i=0; i<l.streets.Count; i++) {
//			Street street = l.streets[i];
//			if (street is SegueStreet) {
//				SegueStreet segueStreet = (SegueStreet) street;
//				Vector2 connectingPos = segueStreet.StartPos;
//				int roomLinkID = segueStreet.RoomLinkID;
//				RoomLinkData roomLinkData = l.WorldDataRef.GetRoomLinkDataConnectingRooms(l.RoomKey, segueStreet.OtherRoomKey, roomLinkID);
//				roomLinkData.SetConnectingPos(l.RoomKey, connectingPos);
//				//				l.WorldDataRef.UpdateRoomLinkInFile(l.RoomKey, segueStreet.RoomLinkRef, );
//			}
//		}
//		l.WorldDataRef.ResaveRoomLinksFile();

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

	/** Use this when we want to tack on optional params after, within the same line. */
	static private void AddSomePropFieldsToFS(PropData data, params string[] fieldNames) {
		// Create and add the line to fileString!
		string propName = GetPropName(data);
		AddFS (propName + " " + GetPropFieldsAsString(data, fieldNames));
	}
	/** Use this when this prop has no optional params. */
	static private void AddAllPropFieldsToFS(PropData data, params string[] fieldNames) {
		// Create and add the line to fileString!
		string propName = GetPropName(data);
		AddFS (propName + " " + GetPropFieldsAsString(data, fieldNames));
		AddFSLine();
	}
	static private string GetPropFieldsAsString(PropData data, params string[] fieldNames) {
        //// Round rotation (so we don't get like "90.000001").
        //data.rotation = MathUtils.RoundTo2DPs(data.rotation);
		// Prepare the string to be added.
		string returnString = "";
		Type classType = data.GetType ();
		for (int i=0; i<fieldNames.Length; i++) {
			FieldInfo fieldInfo = GetFieldInfoFromClass (classType, fieldNames[i]);
			// Don't proceed if this field doesn't exist.
			if (fieldInfo == null) {
				Debug.LogWarning("We're trying to save a prop with a field its class (or superclasses) doesn't have: " + classType + ", " + fieldNames[i]);
				continue;
			}
			returnString += fieldNames[i] + ":"; // start it with, like, "startPos:"
			returnString += GetFieldValueAsString(fieldInfo, data); // Add the actual value!
			if (i<fieldNames.Length-1) { // If this is NOT the last value, add a semicolon to separate us from the next value!
				returnString += ";";
			}
		}
		return returnString;
	}
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
	static private void AddFSLine() { AddFSLine (""); }
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
            case BATTERY: return new BatteryData();
            case CAMERA_BOUNDS: return new CameraBoundsData();
            case CHAR_BARREL: return new CharBarrelData();
            case CRATE: return new CrateData();
            case DAMAGEABLE_GROUND: return new DamageableGroundData();
            case ENEMY: return new EnemyData();
            case GATE: return new GateData();
            case GATE_BUTTON: return new GateButtonData();
            case GEM: return new GemData();
            case GROUND: return new GroundData();
            case ROOM_DOOR: return new RoomDoorData();
            case LIFT: return new LiftData();
            case PLATFORM: return new PlatformData();
            case PLAYER_START: return new PlayerStartData();
            case PROGRESS_GATE: return new ProgressGateData();
            case INFOSIGN: return new InfoSignData();
            case SNACK: return new SnackData();
            case SPIKES: return new SpikesData();
            case TOGGLE_GROUND: return new ToggleGroundData();
            case TRAVELING_PLATFORM: return new TravelingPlatformData();
            case VEIL: return new VeilData();
            default: return null;
        }
    }
	static public void AddEmptyRoomElements(ref RoomData rd) {
		CameraBoundsData cameraBoundsData = new CameraBoundsData();
		cameraBoundsData.myRect = new Rect(-26,-19, 52,38);
		cameraBoundsData.pos = cameraBoundsData.myRect.center;
        rd.cameraBoundsData = cameraBoundsData;
        rd.AddPropData(cameraBoundsData);

		PlayerStartData playerStartData = new PlayerStartData();
		playerStartData.pos = new Vector2(-20, -14);
        rd.AddPropData(playerStartData);

		Rect[] groundRects = {
			new Rect(0,-18, 52,4),
			new Rect(-24,0, 4,32),
			new Rect(24,0, 4,32),
			new Rect(0,18, 52,4),
		};
		foreach (Rect rect in groundRects) {
			GroundData newGroundData = new GroundData();
			newGroundData.myRect = rect;
			newGroundData.pos = rect.center;
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
			string[] nameAndValue = new string[2];
			nameAndValue[0] = fieldStrings[i].Substring (0, colonIndex);
			nameAndValue[1] = fieldStrings[i].Substring (colonIndex+1);
			// Set this specified field for this PropData!
			SetPropDataFieldValue (propData, nameAndValue[0], nameAndValue[1]);
		}
	}

	static private void SetPropDataFieldValue(PropData propData, string fieldName, string fieldValueString) {
		// What extension of PropData is this?
		Type propDataType = propData.GetType ();
		// Get the FieldInfo of the requested name from this propData's class.
		FieldInfo fieldInfo = propDataType.GetField (fieldName);
		// Get the VALUE of this field from the string!
		if (fieldInfo == null) {
			Debug.LogError ("We've been provided an unidentified prop field type. " + debug_roomDataLoadingRoomKey + ". PropData: " + propData + ", fieldName: " + fieldName);
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


