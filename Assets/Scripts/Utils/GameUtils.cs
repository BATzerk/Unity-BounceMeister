using UnityEngine;
using System.Collections;
using System.IO;

public class GameUtils {

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
	static public void SizeSpriteRenderer (SpriteRenderer sr, Vector2 desiredSize) { SizeSpriteRenderer(sr, desiredSize.x,desiredSize.y); }
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
//	static public void SizeMeshRenderer (MeshRenderer mesh, float desiredDiameter) { SizeMeshRenderer (mesh, desiredDiameter,desiredDiameter); }
//	static public void SizeMeshRenderer (MeshRenderer mesh, float desiredWidth,float desiredHeight, bool doPreserveRatio=false) {
//		if (mesh == null) { Debug.LogError("Oops! We've passed in a null MeshRenderer into GameUtils.SizeMeshRenderer."); return; }
//		// Reset its scale; find out its neutral size; scale the images based on the neutral size.
//		mesh.transform.localScale = Vector2.one;
//		float imgW = mesh.bounds.size.x;
//		float imgH = mesh.bounds.size.z;
//		if (doPreserveRatio) {
//			if (imgW > imgH) { desiredHeight *= imgH/imgW; }
//			else if (imgW < imgH) { desiredWidth *= imgW/imgH; }
//		}
//		mesh.transform.localScale = new Vector3 (desiredWidth/imgW, desiredHeight/imgH);
	//	}
	static public void SizeUIGraphic (UnityEngine.UI.Graphic uiGraphic, float desiredWidth,float desiredHeight) {//, bool doPreserveRatio=false) {
//		uiGraphic.rectTransform.localScale = Vector2.one;
//		float imgW = uiGraphic.rectTransform.sizeDelta.bounds.size.x;
//		float imgH = sprite.sprite.bounds.size.y;
//		if (doPreserveRatio) {
//			if (imgW > imgH) {
//				desiredHeight *= imgH/imgW;
//			}
//			else if (imgW < imgH) {
//				desiredWidth *= imgW/imgH;
//			}
//		}
		uiGraphic.rectTransform.sizeDelta = new Vector2 (desiredWidth, desiredHeight);
	}
	static public void SizeUIGraphicX (UnityEngine.UI.Graphic uiGraphic, float desiredWidth) {
		uiGraphic.rectTransform.sizeDelta = new Vector2 (desiredWidth, uiGraphic.rectTransform.sizeDelta.y);
	}
	static public void SizeUIGraphicY (UnityEngine.UI.Graphic uiGraphic, float desiredHeight) {
		uiGraphic.rectTransform.sizeDelta = new Vector2 (uiGraphic.rectTransform.sizeDelta.x, desiredHeight);
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

	public static void SetParticleSystemEmissionEnabled (ParticleSystem ps, bool isEnabled) {
//		ps.Play();
//		ps.loop = isEnabled;
		//ps.enableEmission = isEnabled; // Don't have time right now to figure out how to get this to work (I gave it 15 minutes, I am done!). <3
		ParticleSystem.EmissionModule em;
		em = ps.emission;
		em.enabled = isEnabled;
	}
	public static void SetParticleSystemColor (ParticleSystem ps, Color _color) {
//		ps.startColor = _color;
		ParticleSystem.MainModule main = ps.main;
		main.startColor = new ParticleSystem.MinMaxGradient(_color);
//		var main = ps.main;
//		main.startColor = Color.red;
	}


	public static void DestroyAllChildren (Transform parentTF) {
		for (int i=parentTF.childCount-1; i>=0; --i) {
			Transform child = parentTF.GetChild(i);
			GameObject.Destroy (child.gameObject);
		}
	}

	static public void SetEditorCameraPos(Vector2 pos) {
		if (UnityEditor.SceneView.lastActiveSceneView != null) {
			UnityEditor.SceneView.lastActiveSceneView.LookAt(new Vector3(pos.x,pos.y, -10));
		}
		else { Debug.LogWarning("Can't set editor camera position: UnityEditor.SceneView.lastActiveSceneView is null."); }
	}

}





