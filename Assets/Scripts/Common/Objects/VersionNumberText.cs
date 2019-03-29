using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionNumberText : MonoBehaviour {
    void Start() {
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        if (text != null) { text.text = "v" + Application.version; }
        else { Debug.LogWarning("Oops! A VersionNumberText doesn't have a TextMeshProUGUI field on it."); }
    }
}
