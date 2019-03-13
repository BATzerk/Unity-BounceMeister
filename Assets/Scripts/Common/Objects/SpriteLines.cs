using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class SpriteLines : MonoBehaviour {
	// Components
	private List<SpriteLine> lines;
	// Properties
	private Color lineColor;
	private float lineThickness;
	private List<Vector2> points;

	// Getters
	//	public List<Vector2> Points { get { return points; } }
	public int NumPoints { get { return points.Count; } }

	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize (Transform tf_parent) {
		this.transform.SetParent (tf_parent);
		this.transform.localScale = Vector3.one;
		this.transform.localPosition = Vector3.zero;
		this.transform.localEulerAngles = Vector3.zero;
		this.gameObject.name = "SpriteLines";

		lines = new List<SpriteLine>();
		points = new List<Vector2> ();
	}


	// ----------------------------------------------------------------
	//  Add/Remove/Set
	// ----------------------------------------------------------------
	public void AddPoint (Vector2 _point) {
		points.Add (_point);
		// If we have at least 2 points (to make a line)!...
		if (points.Count > 1) {
			AddLine (points[NumPoints-2], points[NumPoints-1]);
		}
	}
	public void RemovePoint () {
		points.RemoveAt (NumPoints-1);
		// If we at least have any lines left...
		if (lines.Count > 0) {
			RemoveLine ();
		}
	}
	private void AddLine (Vector2 startPos, Vector2 endPos) {
		SpriteLine newLine = new GameObject ().AddComponent<SpriteLine> ();
		newLine.gameObject.name = "SpriteLine" + lines.Count;
		newLine.transform.SetParent (this.transform);
		newLine.Initialize (startPos, endPos);
		newLine.SetColor (lineColor);
		newLine.SetThickness (lineThickness);
		lines.Add (newLine);
	}
	private void RemoveLine () {
		SpriteLine sl = lines[lines.Count-1];
		lines.Remove (sl);
		Destroy (sl.gameObject);
	}

	public void SetPoint (int index, Vector2 _point) {
		points[index] = _point;
		// Update the corresponding lines!
		if (index>0) {
			lines[index-1].EndPos = _point;
		}
		if (index<NumPoints-1) {
			lines[index].StartPos = _point;
		}
	}

	// ----------------------------------------------------------------
	//  Visuals
	// ----------------------------------------------------------------
	public void SetActive (bool _isActive) {
		this.gameObject.SetActive (_isActive);
	}
	public void SetColor (Color _color) {
		lineColor = _color;
		foreach (SpriteLine sl in lines) {
			sl.SetColor (lineColor);
		}
	}
	public void SetThickness (float _thickness) {
		lineThickness = _thickness;
		foreach (SpriteLine sl in lines) {
			sl.SetThickness (lineThickness);
		}
	}

}
*/