using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/** Add this class to the root Canvas. Then you can access the Canvas properties from anywhere in the program! :) */
public class MainCanvas : MonoBehaviour {
    // References
    public static Canvas Canvas { get; private set; }
    public static RectTransform MyRectTransform { get; private set; }

    // Getters
    public static float Width { get { return MyRectTransform.rect.width; } }
    public static float Height { get { return MyRectTransform.rect.height; } }
    public static Vector2 Size { get { return MyRectTransform.rect.size; } }


    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake () {
        // Set references!
        Canvas = GetComponent<Canvas>();
        MyRectTransform = GetComponent<RectTransform>();
    }
    private void OnDestroy() {
        // For cleanliness. If I'm being destroyed and the static ref is to me, null it out.
        if (Canvas == GetComponent<Canvas>()) {
            Canvas = null;
            MyRectTransform = null;
        }
    }


}

