using UnityEngine;
using System.Collections;


/** If you want a line that's NOT a Vector, use this class.
 * Useful for if you want sprites BETWEEN lines. */

public class SpriteLine : MonoBehaviour {
	// Components
	[SerializeField] private SpriteRenderer sprite; // the actual line sprite thingy
	// Properties
	private float angle; // in DEGREES.
	private float length;
	private float thickness = 1f;
	// References
	//	private Line line;
	private Vector2 startPos;
	private Vector2 endPos;

	// Getters
	public float Angle { get { return angle; } }
	public float Length { get { return length; } }
	public Vector2 StartPos {
		get { return startPos; }
		set {
			if (startPos == value) { return; }
			startPos = value;
			UpdateAngleLengthPosition ();
		}
	}
	public Vector2 EndPos {
		get { return endPos; }
		set {
			if (endPos == value) { return; }
			endPos = value;
			UpdateAngleLengthPosition ();
		}
	}

	public void SetStartAndEndPos (Vector2 _startPos, Vector2 _endPos) {
		startPos = _startPos;
		endPos = _endPos;
		UpdateAngleLengthPosition ();
	}



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
//	private void Awake () {
//		// Make my SpriteRenderer if it doesn't exist!
//		if (sprite == null) {
//			sprite = this.gameObject.AddComponent<SpriteRenderer> ();
//			Texture2D squareTexture = Resources.Load ("Materials/whiteSquare1") as Texture2D;
//			sprite.sprite = Sprite.Create (squareTexture, new Rect(0,0, squareTexture.width,squareTexture.height), new Vector2(0.5f,0.5f));
//		}
//	}
	public void Initialize () {
		Initialize (Vector2.zero, Vector2.zero);
	}
	public void Initialize (Vector2 _startPos, Vector2 _endPos) {
		startPos = _startPos;
		endPos = _endPos;

		UpdateAngleLengthPosition ();
	}


	// ----------------------------------------------------------------
	//  Update Things
	// ----------------------------------------------------------------
	private void UpdateAngleLengthPosition() {
		// Update values
		angle = LineUtils.GetAngle_Degrees (startPos, endPos);
		length = LineUtils.GetLength (startPos, endPos);
		// Transform sprite!
		if (float.IsNaN (endPos.x)) {
			Debug.LogError ("Ahem! A SpriteLine's endPos is NaN! (Its startPos is " + startPos + ".)");
		}
		this.transform.localPosition = LineUtils.GetCenterPos(startPos, endPos);
		this.transform.localEulerAngles = new Vector3 (0, 0, angle);
		GameUtils.SizeSpriteRenderer(sprite, length, thickness);
	}

	public bool IsVisible {
		get { return sprite.enabled; }
		set {
			sprite.enabled = value;
		}
	}
	public void SetAlpha(float alpha) {
		GameUtils.SetSpriteAlpha (sprite, alpha);
	}
	public void SetColor(Color color) {
		sprite.color = color;
	}
	public void SetSortingOrder(int sortingOrder) {
		sprite.sortingOrder = sortingOrder;
	}
	public void SetThickness(float _thickness) {
		thickness = _thickness;
		GameUtils.SizeSpriteRenderer(sprite, length, thickness);
	}


}




