using System;
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

    public static void SetBool (string key, bool val) {
        PlayerPrefs.SetInt (key, val?1:0);
    }
    public static void SetFloat (string key, float val) {
        PlayerPrefs.SetFloat (key, val);
    }
    public static void SetInt (string key, int val) {
        PlayerPrefs.SetInt (key, val);
    }
    public static void SetString (string key, string val) {
        PlayerPrefs.SetString (key, val);
    }
    public static void SetDateTime (string key, DateTime val) {
        PlayerPrefs.SetString (key, val.ToBinary().ToString());
    }


    public static bool GetBool (string key) {
        return GetBool(key, false);
    }
    public static bool GetBool (string key, bool defaultVal) {
        return PlayerPrefs.GetInt (key,defaultVal?1:0) == 1;
    }
    public static float GetFloat (string key) {
        return PlayerPrefs.GetFloat (key);
    }
    public static float GetFloat (string key, float defaultVal) {
        return PlayerPrefs.GetFloat (key, defaultVal);
    }
    public static int GetInt (string key) {
        return PlayerPrefs.GetInt (key);
    }
    public static int GetInt (string key, int defaultVal) {
        return PlayerPrefs.GetInt (key, defaultVal);
    }
    public static string GetString (string key) {
        return PlayerPrefs.GetString (key);
    }
    public static string GetString (string key, string defaultVal) {
        return PlayerPrefs.GetString (key, defaultVal);
    }
    public static DateTime GetDateTime(string key) {
        return GetDateTime(key, DateTime.MinValue);
    }
    public static DateTime GetDateTime(string key, DateTime defaultVal) {
        // No key? Set default value!
        if (!HasKey(key)) { SetDateTime(key, defaultVal); }
        // Return save.
        long temp = Convert.ToInt64(GetString(key));
        return DateTime.FromBinary(temp);
    }


}
