using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;


static public class LevelSaverLoader {
	// Constants (Properties)
	const string LEVEL_PROPERTIES = "levelProperties";
	// Constants (Objects)
	const string BATTERY = "Battery";
	const string CAMERA_BOUNDS = "CameraBounds";
	const string CHAR_BARREL = "CharBarrel";
	const string CRATE = "Crate";
	const string DAMAGEABLE_GROUND = "DamageableGround";
	const string GATE = "Gate";
	const string GATE_BUTTON = "GateButton";
	const string GEM = "Gem";
	const string GROUND = "Ground";
	const string LEVEL_DOOR = "LevelDoor";
	const string LIFT = "Lift";
    const string PLATFORM = "Platform";
	const string PLAYER_START = "PlayerStart";
    const string PROGRESS_GATE = "ProgressGate";
    const string SNACK = "Snack";
    const string SPIKES = "Spikes";
	const string TOGGLE_GROUND = "ToggleGround";
	// Properties
	private static string debug_levelDataLoadingLevelKey; // purely for printing to the console without having to pass this parameter through a chain of functions


	// ================================================================
	//  Getters
	// ================================================================
	static private bool IsStringAnAffectName(string str) {
		return str.IndexOf (":") == -1; // If the line DOESN'T feature a colon (the characteristic of property lines), it's an "affect"!
	}
	static private string[] GetLevelFileAsStringArray(int worldIndex, string levelKey) {
		string filePath = FilePaths.WorldFileAddress (worldIndex) + levelKey + ".txt";
//		TextAsset textAsset = (Resources.Load (filePath) as TextAsset);
//		return TextUtils.GetStringArrayFromTextAsset (textAsset);

		if (File.Exists(filePath)) {
			StreamReader file = File.OpenText(filePath);
			string wholeFile = file.ReadToEnd();
			file.Close();
			return TextUtils.GetStringArrayFromStringWithLineBreaks(wholeFile, StringSplitOptions.None);
		}
//		string finalPath = "file://" + absoluteImagePath;
//		WWW localFile = new WWW(localFile);
//
//		texture = localFile.texture;
//		sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
//
//		System.IO.FileStream stream = System.IO.File.OpenRead (filePath);
//		string[] stringArray = stream.
//		stream.Close();
//		}
		else {
			Debug.LogError("Level file not found! World " + worldIndex + ", levelKey " + levelKey + "\nfilePath: \"" + filePath + "\"");
			return null;
		}
	}

	private static string GetLevelPropertiesLine (LevelData ld) {
		string returnString = LEVEL_PROPERTIES + " ";
		returnString += "posGlobal:" + ld.PosGlobal;
		returnString += ";designerFlag:" + ld.DesignerFlag;
        if (ld.isClustStart) { returnString += ";isClustStart:" + ld.isClustStart; }
		return returnString;
	}


	// ================================================================
	//  Saving
	// ================================================================
	/** I couldn't decide where this function belonged. It's here in LevelData (instead of in Level, or in WorldData) so it can be right by the loading function. */
	static private string fs; // Messy. I don't like how this is out here. This could be avoided if we were able to use anonymous functions.
    static public void SaveLevelFile (Level l) { SaveLevelFile (l.SerializeAsData()); }
    static public void SaveLevelFile (LevelData ld) { SaveLevelFileAs (ld, ld.WorldIndex, ld.LevelKey); }
	//static public void SaveLevelFileAs (Level l, int worldIndex,string levelKey) {
	//	SaveLevelFileAs(l.SerializeAsData(), worldIndex, levelKey);
 //       //// Update ALL WorldData level stuff (actually just for openings/neighbors atm).
 //       //GameManagers.Instance.DataManager.GetWorldData(worldIndex).SetAllLevelDatasFundamentalProperties();
	//}

	static public void SaveLevelFileAs(LevelData ld, int worldIndex,string levelKey) {
		fs = ""; // fileString. this guy will be packed with \n line-breaks, then at the very end split by \n. It's less code to look at.

		// Level Properties
		AddFSLine (GetLevelPropertiesLine(ld));
		AddAllPropFieldsToFS(ld.cameraBoundsData, "myRect");

		foreach (PropData propData in ld.allPropDatas) {
			Type type = propData.GetType();
			if (type == typeof(BatteryData)) { AddAllPropFieldsToFS(propData, "pos"); }
			else if (type == typeof(CharBarrelData)) { AddAllPropFieldsToFS(propData, "pos", "otherCharName"); }
			else if (type == typeof(GateButtonData)) { AddAllPropFieldsToFS(propData, "pos", "channelID"); }
			else if (type == typeof(GemData)) { AddAllPropFieldsToFS(propData, "pos", "type"); }
            else if (type == typeof(LevelDoorData)) { AddAllPropFieldsToFS(propData, "pos", "myID", "worldToIndex", "levelToKey", "levelToDoorID"); }
            else if (type == typeof(LiftData)) { AddAllPropFieldsToFS(propData, "myRect", "rotation", "strength"); }
			else if (type == typeof(PlayerStartData)) { AddAllPropFieldsToFS(propData, "pos"); }
            else if (type == typeof(SnackData)) { AddAllPropFieldsToFS(propData, "pos"); }
			else if (type == typeof(SpikesData)) { AddAllPropFieldsToFS(propData, "myRect", "rotation"); }
			// Props with optional params
			else if (type == typeof(CrateData)) {
				CrateData d = propData as CrateData;
				AddSomePropFieldsToFS(propData, "myRect", "hitsUntilBreak", "numCoinsInMe");
                if (!d.canEatGems) { fs += ";canEatGems:" + d.canEatGems; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
			}
			else if (type == typeof(DamageableGroundData)) {
				DamageableGroundData d = propData as DamageableGroundData;
				AddSomePropFieldsToFS(propData, "myRect", "doRegen");
                if (!d.canEatGems) { fs += ";canEatGems:" + d.canEatGems; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                if (d.dieFromBounce) { fs += ";dieFromBounce:" + d.dieFromBounce; }
                if (d.dieFromPlayerLeave) { fs += ";dieFromPlayerLeave:" + d.dieFromPlayerLeave; }
                if (d.dieFromVel) { fs += ";dieFromVel:" + d.dieFromVel; }
                AddFSLine();
			}
			else if (type == typeof(GateData)) {
				GateData d = propData as GateData;
				AddSomePropFieldsToFS(propData, "myRect", "channelID");
				if (!d.canEatGems) { fs += ";canEatGems:" + d.canEatGems; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
			}
			else if (type == typeof(GroundData)) {
				GroundData d = propData as GroundData;
				AddSomePropFieldsToFS(propData, "myRect");
                if (!d.canEatGems) { fs += ";canEatGems:" + d.canEatGems; }
                if (d.isBouncy) { fs += ";isBouncy:" + d.isBouncy; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                if (d.colorType!=0) { fs += ";colorType:" + d.colorType; }
                if (!d.canBounce) { fs += ";canBounce:" + d.canBounce; }
                if (!d.doRechargePlayer) { fs += ";doRechargePlayer:" + d.doRechargePlayer; }
                AddFSLine();
			}
            else if (type == typeof(PlatformData)) {
                PlatformData d = propData as PlatformData;
                AddSomePropFieldsToFS(propData, "myRect");
                if (!d.canEatGems) { fs += ";canEatGems:" + d.canEatGems; }
                if (!d.canDropThru) { fs += ";canDropThru:" + d.canDropThru; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
            }
            else if (type == typeof(ProgressGateData)) {
                ProgressGateData d = propData as ProgressGateData;
                AddSomePropFieldsToFS(propData, "myRect", "numSnacksReq");
                if (!d.canEatGems) { fs += ";canEatGems:" + d.canEatGems; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
            }
			else if (type == typeof(ToggleGroundData)) {
				ToggleGroundData d = propData as ToggleGroundData;
				AddSomePropFieldsToFS(propData, "myRect", "startsOn", "togFromContact", "togFromPlunge");
				if (!d.canEatGems) { fs += ";canEatGems:" + d.canEatGems; }
                if (d.isPlayerRespawn) { fs += ";isPlayerRespawn:" + d.isPlayerRespawn; }
                AddFSLine();
			}
            else {
                Debug.LogWarning("Prop in Level not recognized for serialization: " + type);
            }
		}


		string[] levelFileArray = fs.Split('\n');

		// Save it!
		SaveLevelFileFromStringArray(worldIndex, levelKey, levelFileArray);

//		// FINALLY, go ahead and update the LevelLinks and the SEPARATE _LevelLinks.txt file. :)
//		for (int i=0; i<l.streets.Count; i++) {
//			Street street = l.streets[i];
//			if (street is SegueStreet) {
//				SegueStreet segueStreet = (SegueStreet) street;
//				Vector2 connectingPos = segueStreet.StartPos;
//				int levelLinkID = segueStreet.LevelLinkID;
//				LevelLinkData levelLinkData = l.WorldDataRef.GetLevelLinkDataConnectingLevels(l.LevelKey, segueStreet.OtherLevelKey, levelLinkID);
//				levelLinkData.SetConnectingPos(l.LevelKey, connectingPos);
//				//				l.WorldDataRef.UpdateLevelLinkInFile(l.LevelKey, segueStreet.LevelLinkRef, );
//			}
//		}
//		l.WorldDataRef.ResaveLevelLinksFile();

		// We're done saving! Now that we've got the updated saved text file, reload the corresponding LevelData.
		ld.WorldDataRef.ReloadLevelData (levelKey);

//		// Update who the most recently created level is!
//		GameManagers.Instance.DataManager.mostRecentlySavedLevel_worldIndex = WorldIndex;
//		GameManagers.Instance.DataManager.mostRecentlySavedLevel_levelKey = levelKey;
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
		// Prepare the string to be added.
		string returnString = "";
		Type classType = data.GetType ();
		for (int i=0; i<fieldNames.Length; i++) {
			FieldInfo fieldInfo = GetFieldInfoFromClass (classType, fieldNames[i]);
			// Don't proceed if this field doesn't exist.
			if (fieldInfo == null) {
//				// HACK! If this is "pos", but our prop doesn't have it as a field (most props DON'T), we know it refers to the prop's localPosition, so use that.
//				if (fieldNames[i] == "pos") {
//					returnString += "pos:" + new Vector2(data.transform.localPosition.x,data.transform.localPosition.y).ToString ();
//					if (i<fieldNames.Length-1) { // If this is NOT the last value, add a semicolon to separate us from the next value!
//						returnString += ";";
//					}
//				}
//				else {
					Debug.LogWarning("We're trying to save a prop with a field its class (or superclasses) doesn't have: " + classType.ToString() + ", " + fieldNames[i]);
//				}
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

	static private void SaveLevelFileFromStringArray(int worldIndex, string levelKey, string[] levelFileArray) {
		// Otherwise, SAVE! :D
		string filePath = FilePaths.LevelFileAddress(worldIndex, levelKey);
		try {
			StreamWriter sr = File.CreateText (filePath);
			foreach (string lineString in levelFileArray) {
				sr.WriteLine (lineString);
			}
			sr.Close();
			Debug.Log("SAVED LEVEL " + levelKey);

			GameManagers.Instance.EventManager.OnEditorSaveLevel();

//			// Reload the text file right away!! (Otherwise, we'll have to ALT + TAB out of Unity and back in for it to be refreshed.)
//			#if UNITY_EDITOR
//			UnityEditor.AssetDatabase.ImportAsset (filePath);
//			#endif
		}
		catch (Exception e) {
			Debug.LogError ("Error saving a level to a text file: " + levelKey + ". Save location: " + filePath + ". Error string: " + e.ToString ());
		}

//		// Finally! Delete the saved snapshot of this level completely! If we don't, a number of odd behaviors can occur from the data conflict. One notable effect is when changing posGlobal: carts' joints will be all wonky (because they were saved in a different part of the world).
//		GameplaySnapshotController.DeleteLevelSnapshotFromSaveStorage (WorldIndex, levelKey);
//		// Also delete the saved snapshot of the player if it's in this exact level.
//		GameplaySnapshotController.DeletePlayerDataSnapshotFromSaveStorageIfInLevelStatic (WorldIndex, levelKey);
	}


	public static void UpdateLevelPropertiesInLevelFile(LevelData ld) {
		// Load the file!
		String[] levelFileArray = GetLevelFileAsStringArray(ld.WorldIndex, ld.LevelKey);
		// No file? No cheese.
		if (levelFileArray == null) { return; }
		// Look through the file for the line to overwrite.
		for (int i=0; i<levelFileArray.Length; i++) {
			String lineString = levelFileArray[i];
			// This is the line!!
			if (lineString.StartsWith(LEVEL_PROPERTIES, StringComparison.InvariantCulture)) {
				// Replace the properties line!
				levelFileArray[i] = GetLevelPropertiesLine(ld);
				break;
			}
		}
		// Remove any extra line breaks at the end.
		TextUtils.RemoveExcessLineBreaksFromStringArray (ref levelFileArray);
		// Now, re-save the whole thing!
		SaveLevelFileFromStringArray(ld.WorldIndex, ld.LevelKey, levelFileArray);
	}




	static public void LoadLevelDataFromItsFile (LevelData ld) {
		// First, make empty buckets of all PropDatas.
		ld.ClearAllPropDataLists ();

		// Load the file!
		string[] levelFile = GetLevelFileAsStringArray(ld.WorldIndex, ld.LevelKey);

		// NULL level file...
		if (levelFile == null) {
			AddEmptyLevelElements(ref ld);
		}
		// There IS a level file!...
		else {
			debug_levelDataLoadingLevelKey = ld.LevelKey; // for printing to console.
			foreach (string lineString in levelFile) {
				if (lineString == "") continue; // If this line is EMPTY, skip it!
				int affectNameEndIndex = lineString.IndexOf(' ');
				if (affectNameEndIndex==-1) { continue; } // Wrong formatting!
				string affectName = lineString.Substring(0, affectNameEndIndex);
				string propertiesString = lineString.Substring(affectNameEndIndex+1);
				// Level Properties
				if (affectName == LEVEL_PROPERTIES) {
					SetLevelPropertyFieldValuesFromFieldsString (ld, propertiesString);
				}
				// Props!
				else {
					PropData propData = GetNewPropDataFromAffectName(affectName);
					if (propData == null) { // Safety check.
						Debug.LogError ("Oops! Unidentifiable text in lvl file: " + ld.LevelKey + ". Text: \"" + lineString + "\"");
						continue;
					}
					SetPropDataFieldValuesFromFieldsString (propData, propertiesString);
					ld.allPropDatas.Add(propData);
					// hacky?
					if (propData is CameraBoundsData) {
						ld.cameraBoundsData = propData as CameraBoundsData;
					}
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
            case GATE: return new GateData();
            case GATE_BUTTON: return new GateButtonData();
            case GEM: return new GemData();
            case GROUND: return new GroundData();
            case LEVEL_DOOR: return new LevelDoorData();
            case LIFT: return new LiftData();
            case PLATFORM: return new PlatformData();
            case PLAYER_START: return new PlayerStartData();
            case PROGRESS_GATE: return new ProgressGateData();
            case SNACK: return new SnackData();
            case SPIKES: return new SpikesData();
            case TOGGLE_GROUND: return new ToggleGroundData();
            default: return null;
        }
    }
	static public void AddEmptyLevelElements(ref LevelData ld) {
		CameraBoundsData cameraBoundsData = new CameraBoundsData();
		cameraBoundsData.myRect = new Rect(-26,-19, 52,38);
		cameraBoundsData.pos = cameraBoundsData.myRect.center;
        ld.cameraBoundsData = cameraBoundsData;
		ld.allPropDatas.Add(cameraBoundsData);

		PlayerStartData playerStartData = new PlayerStartData();
		playerStartData.pos = new Vector2(-20, -14);
		ld.allPropDatas.Add(playerStartData);

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
			ld.allPropDatas.Add (newGroundData);
		}
	}

	static private void SetLevelPropertyFieldValuesFromFieldsString(LevelData ld, string fieldsString) {
		// Divide the long string of ALL fields into an array, each slot just ONE field.
		string[] fieldStrings = fieldsString.Split (';');
		for (int i=0; i<fieldStrings.Length; i++) {
			// Divide this one field and its value into a two-part array: first element is the NAME, second element is the VALUE.
			string[] nameAndValue = fieldStrings[i].Split(':');
			string name = nameAndValue[0];
			string value = nameAndValue[1];
			try {
				if (name == "posGlobal") {
					ld.SetPosGlobal(TextUtils.GetVector2FromString(value));
				}
				else if (name == "designerFlag") {
					ld.SetDesignerFlag(TextUtils.ParseInt(value));
				}
                else if (name == "isClustStart") {
                    ld.isClustStart = TextUtils.ParseBool(value);
                }
			}
			catch (Exception e) {
				Debug.LogError ("Level file has a parameter we don't recognize: " + ld.LevelKey + ", " + name + ", " + value + ". " + e.ToString ());
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
				Debug.LogError ("Invalid line in level file: " + debug_levelDataLoadingLevelKey + ". \"" + fieldStrings[i] + "\"");
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
			Debug.LogError ("We've been provided an unidentified prop field type. " + debug_levelDataLoadingLevelKey + ". PropData: " + propData + ", fieldName: " + fieldName);
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

    
    static public bool MayRenameLevelFile(Level level, string newName) { return MayRenameLevelFile(level.LevelDataRef, newName); }
    static public bool MayRenameLevelFile(LevelData ld, string newName) {
        if (string.IsNullOrEmpty(newName)) { return false; } // Empty name? Nah.
        if (ld.levelKey == newName) { return false; } // Same name? Nah.
        string newPath = FilePaths.LevelFileAddress(ld.WorldIndex, newName);
        if (File.Exists(newPath)) { return false; } // File already here? Nah.
        // Looks good!
        return true;
    }
    static public void RenameLevelFile(Level level, string newName) { RenameLevelFile(level.LevelDataRef, newName); }
    static public void RenameLevelFile(LevelData ld, string newName) {
        string oldPath = FilePaths.LevelFileAddress(ld.WorldIndex, ld.LevelKey);
        string newPath = FilePaths.LevelFileAddress(ld.WorldIndex, newName);
        // File already here with this name?
        if (File.Exists(newPath)) {
            Debug.LogWarning("Can't rename level. Already a file here: \"" + newPath + "\"");
        }
        else {
            File.Move(oldPath, newPath);
        }
    }


}







