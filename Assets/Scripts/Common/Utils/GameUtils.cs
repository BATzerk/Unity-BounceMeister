using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class GameUtils {

    /** Parents GO to TF, and resets GO's pos, scale, and rotation! */
    public static void ParentAndReset(GameObject go, Transform tf) {
        go.transform.SetParent(tf);
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
    }

    public static void SetExpandedRecursive(GameObject go,bool expand) { // QQQ TODO: Use one or the other method.
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        var methodInfo = type.GetMethod("SetExpandedRecursive");
        EditorWindow window = EditorWindow.GetWindow(type);
        methodInfo.Invoke(window,new object[] { go.GetInstanceID(),expand });
    }
    //public static void SetGOCollapsed(Transform tf, bool isCollapsed) {
    //    if (tf.childCount == 0) return; // No children? Do nothin'.
    //    var hierarchy = GetFocusedWindow("General/Hierarchy");
    //    SelectObject(tf);
    //    // Create a new key event (RightArrow for collapsing, LeftArrow for folding)
    //    var key = new Event { keyCode = isCollapsed ? KeyCode.RightArrow : KeyCode.LeftArrow, type = EventType.KeyDown };
    //    // Finally, send the window the event
    //    hierarchy.SendEvent(key);
    //}
    //public static void SelectObject(Object obj) {
    //    Selection.activeObject = obj;
    //}
    //public static EditorWindow GetFocusedWindow(string window) {
    //    FocusOnWindow(window);
    //    return EditorWindow.focusedWindow;
    //}
    public static void FocusOnWindow(string window) {
        EditorApplication.ExecuteMenuItem("Window/General/" + window);
    }

    public static GameObject CurrSelectedGO() {
        if (UnityEngine.EventSystems.EventSystem.current==null) { return null; }
        return UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    }

    public static void CopyToClipboard(string str) {
        UnityEngine.GUIUtility.systemCopyBuffer = str;
        //UnityEditor.EditorGUIUtility.systemCopyBuffer = str;
    }

    public static string GetSecondsToTimeString (float _totalSeconds) {
        string minutes = Mathf.Floor (_totalSeconds / 60).ToString("0");
        string seconds = Mathf.Floor (_totalSeconds % 60).ToString("00");
        return minutes + ":" + seconds;
    }

    /** Provides the index of which available screen resolution the current Screen.width/Screen.height combo is at. Returns null if there's no perfect fit. */
    public static int GetClosestFitScreenResolutionIndex () {
        for (int i=0; i<Screen.resolutions.Length; i++) {
            // We found a match!?
            if (Screen.width==Screen.resolutions[i].width && Screen.height==Screen.resolutions[i].height) {
                return i;
            }
        }
        return -1; // Hmm, nah, the current resolution doesn't match anything our monitor recommends.
    }

    /** This function simulates ONE frame's worth of our standard easing, but from one COLOR to another. :)
     easing: Higher is slower. */
    public static Color EaseColorOneStep (Color appliedColor, Color targetColor, float easing) {
        return new Color (appliedColor.r+(targetColor.r-appliedColor.r)/easing, appliedColor.g+(targetColor.g-appliedColor.g)/easing, appliedColor.b+(targetColor.b-appliedColor.b)/easing);
    }

    /** The final alpha will be the provided alpha * the color's base alpha. */
    public static void SetSpriteColorWithCompoundAlpha (SpriteRenderer sprite, Color color, float alpha) {
        sprite.color = new Color (color.r, color.g, color.b, color.a*alpha);
    }
    /** The final alpha will be the provided alpha * the color's base alpha. */
    public static void SetUIGraphicColorWithCompoundAlpha (UnityEngine.UI.Graphic uiGraphic, Color color, float alpha) {
        uiGraphic.color = new Color (color.r, color.g, color.b, color.a*alpha);
    }

    /** The sprite's base color alpha is ignored/overridden. */
    public static void SetSpriteAlpha(SpriteRenderer sprite, float alpha) {
        sprite.color = new Color (sprite.color.r, sprite.color.g, sprite.color.b, alpha);
    }
    public static void SetTextMeshAlpha(TextMesh textMesh, float alpha) {
        textMesh.color = new Color (textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);
    }
    public static void SetUIGraphicAlpha (UnityEngine.UI.Graphic uiGraphic, float alpha) {
        uiGraphic.color = new Color (uiGraphic.color.r, uiGraphic.color.g, uiGraphic.color.b, alpha);
    }

    static public void SizeSpriteMask (SpriteMask sm, Vector2 size) { SizeSpriteMask(sm, size.x,size.y); }
    static public void SizeSpriteMask (SpriteMask sm, float desiredWidth,float desiredHeight, bool doPreserveRatio=false) {
        if (sm == null) {
            Debug.LogError("Oops! We've passed in a null SpriteMask into GameUtils.SizeSpriteMask."); return;
        }
        if (sm.sprite == null) {
            Debug.LogError("Oops! We've passed in a SpriteMask with a NULL Sprite into GameUtils.SizeSpriteMask."); return;
        }
        // Reset my sprite's scale; find out its neutral size; scale the images based on the neutral size.
        sm.transform.localScale = Vector2.one;
        float imgW = sm.sprite.bounds.size.x;
        float imgH = sm.sprite.bounds.size.y;
        if (doPreserveRatio) {
            if (imgW > imgH) {
                desiredHeight *= imgH/imgW;
            }
            else if (imgW < imgH) {
                desiredWidth *= imgW/imgH;
            }
        }
        sm.transform.localScale = new Vector2(desiredWidth/imgW, desiredHeight/imgH);
    }
    static public void SizeSpriteRenderer (SpriteRenderer sr, float widthAndHeight) {
        SizeSpriteRenderer(sr, widthAndHeight,widthAndHeight);
    }
    static public void SizeSpriteRenderer (SpriteRenderer sr, Vector2 size) {
        SizeSpriteRenderer(sr, size.x,size.y);
    }
    static public void SizeSpriteRenderer (SpriteRenderer sr, float desiredWidth,float desiredHeight, bool doPreserveRatio=false) {
        if (sr == null) {
            Debug.LogError("Oops! We've passed in a null SpriteRenderer into GameUtils.SizeSpriteRenderer."); return;
        }
        if (sr.sprite == null) {
            Debug.LogError("Oops! We've passed in a SpriteRenderer with a NULL Sprite into GameUtils.SizeSpriteRenderer."); return;
        }
        // Reset my sprite's scale; find out its neutral size; scale the images based on the neutral size.
        sr.transform.localScale = Vector2.one;
        float imgW = sr.sprite.bounds.size.x;
        float imgH = sr.sprite.bounds.size.y;
        if (doPreserveRatio) {
            if (imgW > imgH) {
                desiredHeight *= imgH/imgW;
            }
            else if (imgW < imgH) {
                desiredWidth *= imgW/imgH;
            }
        }
        sr.transform.localScale = new Vector2(desiredWidth/imgW, desiredHeight/imgH);
    }
//  static public void SizeMeshRenderer (MeshRenderer mesh, float desiredDiameter) { SizeMeshRenderer (mesh, desiredDiameter,desiredDiameter); }
//  static public void SizeMeshRenderer (MeshRenderer mesh, float desiredWidth,float desiredHeight, bool doPreserveRatio=false) {
//      if (mesh == null) { Debug.LogError("Oops! We've passed in a null MeshRenderer into GameUtils.SizeMeshRenderer."); return; }
//      // Reset its scale; find out its neutral size; scale the images based on the neutral size.
//      mesh.transform.localScale = Vector2.one;
//      float imgW = mesh.bounds.size.x;
//      float imgH = mesh.bounds.size.z;
//      if (doPreserveRatio) {
//          if (imgW > imgH) { desiredHeight *= imgH/imgW; }
//          else if (imgW < imgH) { desiredWidth *= imgW/imgH; }
//      }
//      mesh.transform.localScale = new Vector3 (desiredWidth/imgW, desiredHeight/imgH);
//  }
    static public void SizeUIGraphic (UnityEngine.UI.Graphic uiGraphic, Vector2 size) { SizeUIGraphic(uiGraphic, size.x,size.y); }
    static public void SizeUIGraphic (UnityEngine.UI.Graphic uiGraphic, float desiredWidth,float desiredHeight) {//, bool doPreserveRatio=false) {
//      uiGraphic.rectTransform.localScale = Vector2.one;
//      float imgW = uiGraphic.rectTransform.sizeDelta.bounds.size.x;
//      float imgH = sprite.sprite.bounds.size.y;
//      if (doPreserveRatio) {
//          if (imgW > imgH) {
//              desiredHeight *= imgH/imgW;
//          }
//          else if (imgW < imgH) {
//              desiredWidth *= imgW/imgH;
//          }
//      }
        uiGraphic.rectTransform.sizeDelta = new Vector2 (desiredWidth, desiredHeight);
    }


    /** Returns number of seconds elapsed since 1970. */
    public static int GetSecondsSinceEpochStart () {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
    }



    public static Texture2D LoadPNG(string filePath) {
        Texture2D tex = null;
        byte[] fileData;
        
        if (File.Exists(filePath)) {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        else {
            Debug.LogWarning ("Can't find a PNG in this path: " + filePath);
        }
        return tex;
    }

    public static void SetParticleSystemEmissionEnabled (ParticleSystem particleSystem, bool isEnabled) {
        ParticleSystem.EmissionModule m;
        m = particleSystem.emission;
        m.enabled = isEnabled;
    }
    public static void SetParticleSystemColor (ParticleSystem ps, Color _color) {
        ParticleSystem.MainModule m = ps.main;
        m.startColor = _color;
    }
    public static void SetParticleSystemShapeRadius (ParticleSystem particleSystem, float radius) {
        ParticleSystem.ShapeModule m;
        m = particleSystem.shape;
        m.radius = radius;
    }


    public static void DestroyAllChildren (Transform parentTF) {
        for (int i=parentTF.childCount-1; i>=0; --i) {
            Transform child = parentTF.GetChild(i);
            Object.Destroy(child.gameObject);
        }
    }

    static public void SetEditorCameraPos(Vector2 pos) {
        if (UnityEditor.SceneView.lastActiveSceneView != null) {
            UnityEditor.SceneView.lastActiveSceneView.LookAt(new Vector3(pos.x,pos.y, -10));
        }
        else { Debug.LogWarning("Can't set editor camera position: UnityEditor.SceneView.lastActiveSceneView is null."); }
    }



}





