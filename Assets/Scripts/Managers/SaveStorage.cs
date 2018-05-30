using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveStorage {
	public static void Save () {
		PlayerPrefs.Save ();
	}

	public static bool HasKey (string _string) {
		return PlayerPrefs.HasKey (_string);
	}

	public static void DeleteKey (string key) {
		PlayerPrefs.DeleteKey (key);
	}
	public static void DeleteAll () {
		PlayerPrefs.DeleteAll ();
	}

	public static void SetFloat (string key, float value) {
		PlayerPrefs.SetFloat (key, value);
	}
	public static void SetInt (string key, int value) {
		PlayerPrefs.SetInt (key, value);
	}
	public static void SetString (string key, string value) {
		PlayerPrefs.SetString (key, value);
	}

	public static float GetFloat (string key) {
		return PlayerPrefs.GetFloat (key);
	}
	public static float GetFloat (string key, float defaultValue) {
		return PlayerPrefs.GetFloat (key, defaultValue);
	}
	public static int GetInt (string key) {
		return PlayerPrefs.GetInt (key);
	}
	public static int GetInt (string key, int defaultValue) {
		return PlayerPrefs.GetInt (key, defaultValue);
	}
	public static string GetString (string key) {
		return PlayerPrefs.GetString (key);
	}
	public static string GetString (string key, string defaultValue) {
		return PlayerPrefs.GetString (key, defaultValue);
	}


}
