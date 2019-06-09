using UnityEngine;
using System;
using System.Collections;
using System.Globalization;

public static class TextUtils {
    // Properties
    private static string[] LINE_BREAKS_STRINGS = new string[] { System.Environment.NewLine };
    private static CultureInfo parserCulture = CultureInfo.CreateSpecificCulture ("en"); // We ONLY want to parse (number) strings with English culture!

    /** Use THIS function instead of float.Parse!! Because... on PlayStation 4, if the system's language is French, it treats periods as commas. We want ONLY to use English-style punctuation throughout all our backend. */
    public static float ParseFloat (string _string) {
        return float.Parse (_string, parserCulture);
    }
    public static int ParseInt (string _string) { return int.Parse (_string, parserCulture); }
    public static bool ParseBool(string str) { return bool.Parse(str); }

    /** Converts "123" to {"1","2","3"}. */
    public static string[] StringToStringArray (string str) {
        char[] charArray = str.ToCharArray();
        string[] returnArray = new string[charArray.Length];
        for (int i=0; i<charArray.Length; i++) {
            returnArray[i] = charArray[i].ToString();
        }
//      str.Split(new string[]{""}, System.StringSplitOptions.None);
        return returnArray;
    }

    /** E.g. "1:23.45" returns 83.45, "1:00" returns 60, "50" returns 50, etc. */
    static public float TimeToSeconds (string timeString) {
        try {
        if (timeString==null) { return 0; }
        int colonIndex = timeString.IndexOf(":");
        float timeFloat = 0;
        string secondsString;
        if (colonIndex != -1) {
            int minutes = int.Parse(timeString.Substring(0, colonIndex));
            timeFloat += minutes*60;
            secondsString = timeString.Substring(colonIndex+1); // seconds are everything past the colon.
        }
        else {
            secondsString = timeString; // No colon? Then the seconds is the whole thing.
        }
        timeFloat += float.Parse (secondsString);
        return timeFloat;
        }
        catch (Exception e) { Debug.LogError ("Invalid time string: " + timeString + ". Error: " + e.ToString()); return 0; }
    }
    static public string ToTimeString_msm (float timeFloat, string displayStringIf0) {
        if (timeFloat == 0) { return displayStringIf0; }
        return ToTimeString_msm (timeFloat);
    }
    /** E.g. "1:23.45" */
    static public string ToTimeString_msm (float timeFloat) {
//      return GameMathUtils.RoundTo2DPs (timeFloat).ToString ();// for dev purposes. So I only have one unit to look at/work with for now: pure seconds.
        int timeInt = (int) timeFloat;
        int minutes = timeInt / 60;
        int seconds = timeInt % 60;
        int millis = (int) ((timeFloat*100) % 100);
        return string.Format ("{0}:{1:00}.{2:00}", minutes, seconds, millis);
//      return string.Format ("{0}:{1:00}:{00}", (int)timeFloat / 60, (int)timeFloat % 60, timeFloat%1);
    }
    /** E.g. "1:23" */
    static public string ToTimeString_ms (float timeFloat, bool alwaysShowMinutes=true) {
        int timeInt = (int) timeFloat;
        if (!alwaysShowMinutes && timeInt < 60) { // Less than a minute?? ONLY show straight-up seconds!
            return timeInt.ToString ();
        }
        int minutes = timeInt / 60;
        int seconds = timeInt % 60;
        return string.Format ("{0}:{1:00}", minutes, seconds);
    }
    static public string DecimalPlaces (float timeFloat) {
        int millis = (int) ((timeFloat*100) % 100);
        return string.Format ("{0:00}", millis);
    }

    static public string RemoveWhitespace (string _string) {
//      return Regex.Replace(_string, @"\s+", "");public static string RemoveWhitespace(this string str) {
        return string.Join("", _string.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }

//  static public string[] GetStringArrayFromTextAsset (TextAsset textAsset) {
//      if (textAsset == null) { return null; } // If this file doesn't even exist, then return null.
//      return GetStringArrayFromStringWithLineBreaks (textAsset.text);
//  }
    static public string[] GetStringArrayFromStringWithLineBreaks (string wholeString, StringSplitOptions stringSplitOptions) {
//      string[] lines = File.ReadAllLines(worldDataRef.GetWorldFileLocation() + Key + ".txt");
//      return lines;
        return wholeString.Split (LINE_BREAKS_STRINGS, stringSplitOptions);
    }

    static public void RemoveExcessLineBreaksFromStringArray(ref string[] stringArray) {
        int numExcessLineBreaks = 0;
        for (int i=stringArray.Length-1; i>=0; --i) {
            if (stringArray[i].Length == 0) numExcessLineBreaks ++;
            else break;
        }
        string[] returnStringArray = new string[stringArray.Length - numExcessLineBreaks];
        for (int i=0; i<returnStringArray.Length; i++) {
            returnStringArray[i] = stringArray[i];
        }
        stringArray = returnStringArray;
    }




    /// This function parses a string AS FORMATTED by Rect's ToString() function. Example: (x:0.68, 76.18);size:(400.00, 400.00)
    static public Rect GetRectFromString (string str) {
        int colonIndex, commaIndex;
        string xString, yString, wString, hString;

        colonIndex = str.IndexOf (':');
        commaIndex = str.IndexOf (',');
        xString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));
        colonIndex = str.IndexOf (':', colonIndex+1);
        commaIndex = str.IndexOf (',', commaIndex+1);
        yString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));
        colonIndex = str.IndexOf (':', colonIndex+1);
        commaIndex = str.IndexOf (',', commaIndex+1);
        wString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));
        colonIndex = str.IndexOf (':', colonIndex+1);
        commaIndex = str.Length - 1;
        hString = str.Substring (colonIndex+1, commaIndex - (colonIndex+1));

        Rect returnRect = new Rect (TextUtils.ParseFloat (xString),TextUtils.ParseFloat (yString), TextUtils.ParseFloat (wString),TextUtils.ParseFloat (hString));
        return returnRect;
    }
    /** Pass in like this: "-100,320". */
    static public Vector2 GetVector2FromString (string str) {
        if (str==null) { return Vector2.zero; }
        str = str.Substring(1, str.Length-2); // cut the parenthesis.
        int indexOfComma = str.IndexOf (',');
        string xString = str.Substring (0, indexOfComma);
        string yString = str.Substring (indexOfComma+2); // starting after ", ".
        try { // TEST
            float x = ParseFloat (xString);
            float y = ParseFloat (yString);
            return new Vector2 (x,y);
        }
        catch {
            Debug.Log ("Error parsing Vector2 string. x: \"" + xString + "\",  \"" + yString+"\"");
            return Vector2Extensions.NaN;
        }
    }
//  // This function parses a string AS FORMATTED by Vector2's ToString() function.
//  static public Vector2 GetVector2FromString (string str) {
//      if (str==null) { return Vector2.zero; }
//      int indexOfComma = str.IndexOf (',');
//      string xString = str.Substring (1, (-1) + (indexOfComma));
//      string yString = str.Substring (indexOfComma+1, -(indexOfComma+1) + (str.Length-1));
//      float x = ParseFloat (xString);
//      float y = ParseFloat (yString);
//      return new Vector2 (x,y);
//  }
    /** Pass in like this: "0.5,1,0, 1". */
    static public Color ColorFromString(string str) {
        if (str==null) { return Color.white; }
        string[] strings = str.Split(","[0]);
        Color output = Color.white;
        for (int i=0; i<4&&i<strings.Length; i++) {
            output[i] = System.Single.Parse (strings[i]);
        }
        return output;
    }
    static public float[] GetFloatArrayFromString (string _string, char separator=',') {
        string[] stringArray = _string.Split (separator);
        float[] floatArray = new float[stringArray.Length];
        for (int i=0; i<floatArray.Length; i++) {
            floatArray [i] = TextUtils.ParseFloat (stringArray[i]);
        }
        return floatArray;
    }


}
