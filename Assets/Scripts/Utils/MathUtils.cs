using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MathUtils {

	public static float GetPitchShiftFromKeyShift (float keyShift) {
		return Mathf.Pow (1.05946f, keyShift); // Science.
	}

	static public bool IsSameSign (float a, float b) { return a*b >= 0; }
	static public bool IsSameSign (double a, double b) { return a*b >= 0; }
	static public int Sign(float value, bool doAllow0=true) {
		if (value < 0) return -1;
		if (value > 0) return 1;
		if (doAllow0) return 0;
		return 1; // We can specify to prevent returning 0. Very useful for any variable with "dir" in the name.
	}

	/** Will turn numbers like 0.00008279 to a nice, fair 0! */
	static public float RoundTo4DPs (float _value) {
		return Mathf.Round (_value * 1000f) / 1000f;
	}
	
	static public float GetDifferenceBetweenAnglesDegrees (float angleA, float angleB) {
		// Keep both angles between -180 and 180.
		float difference = angleA - angleB;
		if (difference < -180) difference += 360;
		else if (difference > 180) difference -= 360;
		return difference;
	}
	static public float GetDifferenceBetweenAnglesRadians (float angleA, float angleB) {
		// Keep both angles between -PI and PI.
		float difference = angleA - angleB;
		if (difference < -Mathf.PI) difference += Mathf.PI*2;
		else if (difference > Mathf.PI) difference -= Mathf.PI*2;
		return difference;
	}
	static public float GetVector2AngleRadians (Vector2 vector) { return Mathf.Atan2 (-vector.x,vector.y); }
	static public float GetVector2AngleDegrees (Vector2 vector) { return GetVector2AngleRadians (vector) * Mathf.Rad2Deg; }


	/** easing: Higher is SLOWER. */
	public static void EaseRect (ref Rect rect, Rect rectTarget, float easing) {
		rect.xMin += (rectTarget.xMin-rect.xMin) / easing;
		rect.xMax += (rectTarget.xMax-rect.xMax) / easing;
		rect.yMin += (rectTarget.yMin-rect.yMin) / easing;
		rect.yMax += (rectTarget.yMax-rect.yMax) / easing;
	}
	public static Rect LerpRect (Rect rectA, Rect rectB, float t) {
		return new Rect (Vector2.Lerp (rectA.position,rectB.position, t), Vector2.Lerp (rectA.size,rectB.size, t));
	}
	public static float InverseLerpRect (Rect rectA, Rect rectB, Rect rectC) {
		float lerpPosX = Mathf.InverseLerp (rectA.position.x, rectB.position.x, rectC.position.x);
		float lerpPosY = Mathf.InverseLerp (rectA.position.y, rectB.position.y, rectC.position.y);
		float lerpSizeX = Mathf.InverseLerp (rectA.size.x, rectB.size.x, rectC.size.x);
		float lerpSizeY = Mathf.InverseLerp (rectA.size.y, rectB.size.y, rectC.size.y);
		// Return the average of all the sides' inverse lerps!
		float lerpAverage = (lerpPosX+lerpPosY+lerpSizeX+lerpSizeY) / 4f;
		return lerpAverage;
	}

	public static bool AreRectsAboutEqual (Rect rectA, Rect rectB, float threshold=0.1f) {
		return Mathf.Abs (rectA.center.x-rectB.center.x)<threshold
			&& Mathf.Abs (rectA.center.y-rectB.center.y)<threshold
			&& Mathf.Abs (rectA.size.x-rectB.size.x)<threshold
			&& Mathf.Abs (rectA.size.y-rectB.size.y)<threshold;
	}
	/** 0 top, 1 right, 2 bottom, 3 left. E.g. If the second point is mostly to the RIGHT of the first, this'll return 1. */
	public static int GetSideRectIsOn (Rect rectA, Rect rectB) {
		// Because levels aren't always perfectly in line, determine WHICH direction they're more different by. Use that.
		// Whichever value of these is the GREATEST, that's the side rectB is on.
		float diffL = rectA.xMin - rectB.xMax;
		float diffR = rectB.xMin - rectA.xMax;
		float diffB = rectA.yMin - rectB.yMax;
		float diffT = rectB.yMin - rectA.yMax;
		// Sort 'em!
		float[] diffs = { diffL, diffR, diffB, diffT };
		System.Array.Sort (diffs);
		// WHICH is the LARGEST value??
		float largestValue = diffs [diffs.Length - 1];
		if (largestValue == diffL) { return Sides.L; }
		if (largestValue == diffR) { return Sides.R; }
		if (largestValue == diffB) { return Sides.B; }
		if (largestValue == diffT) { return Sides.T; }
		return -1; // impossibru!!
	}
	public static int GetSidePointIsOn (Rect rect, Vector2 point) {
		// Because levels aren't always perfectly in line, determine WHICH direction they're more different by. Use that.
		// Whichever value of these is the GREATEST, that's the side rectB is on.
		float diffL = rect.xMin - point.x;
		float diffR = point.x - rect.xMax;
		float diffB = rect.yMin - point.y;
		float diffT = point.y - rect.yMax;
		// Sort 'em!
		float[] diffs = { diffL, diffR, diffB, diffT };
		System.Array.Sort (diffs);
		// WHICH is the LARGEST value??
		float largestValue = diffs [diffs.Length - 1];
		if (largestValue == diffL) { return Sides.L; }
		if (largestValue == diffR) { return Sides.R; }
		if (largestValue == diffB) { return Sides.B; }
		if (largestValue == diffT) { return Sides.T; }
		return -1; // impossibru!!
	}

	public static Vector2 AbsVector2 (Vector2 v) {
		return new Vector2 (Mathf.Abs (v.x), Mathf.Abs (v.y));
	}
	public static Vector2 GetRotatedVector2Rad (Vector2 v, float radians) {
		return GetRotatedVector2Deg (v, radians*Mathf.Rad2Deg);
	}
	public static Vector2 GetRotatedVector2Deg (Vector2 v, float degrees) {
		return Quaternion.Euler (0, 0, degrees) * v;
	}
	public static Vector3 GetRotatedVector3Deg (Vector3 v, float degrees) {
		return Quaternion.Euler (0, 0, degrees) * v;
	}
	/** 0 is UP, PI is RIGHT. */
	public static Vector2 GetVectorFromAngleRad (float radians) {
		return new Vector2 (Mathf.Sin (radians), Mathf.Cos (radians));
	}
	/** 0 is UP, 90 degrees is RIGHT. */
	public static Vector2 GetVectorFromAngleDeg (float degrees) { return GetVectorFromAngleRad (degrees*Mathf.Deg2Rad); }


	public static Vector2Int GetDir (int side) {
		switch (side) {
		case Sides.L: return Vector2Int.L;
		case Sides.R: return Vector2Int.R;
		case Sides.B: return Vector2Int.B;
		case Sides.T: return Vector2Int.T;
		case Sides.TL: return Vector2Int.TL;
		case Sides.TR: return Vector2Int.TR;
		case Sides.BL: return Vector2Int.BL;
		case Sides.BR: return Vector2Int.BR;
		default: throw new UnityException ("Whoa, " + side + " is not a valid side. Try 0 through 7.");
		}
	}
	public static int GetSide (Vector2Int dir) {
		if (dir == Vector2Int.L) { return Sides.L; }
		if (dir == Vector2Int.R) { return Sides.R; }
		if (dir == Vector2Int.B) { return Sides.B; }
		if (dir == Vector2Int.T) { return Sides.T; }
		if (dir == Vector2Int.TL) { return Sides.TL; }
		if (dir == Vector2Int.TR) { return Sides.TR; }
		if (dir == Vector2Int.BL) { return Sides.BL; }
		if (dir == Vector2Int.BR) { return Sides.BR; }
		return Sides.Undefined; // Whoops.
	}
	public static int GetOppositeSide (Vector2Int dir) { return GetOppositeSide(GetSide(dir)); }
	public static int GetOppositeSide (int side) {
		return Sides.GetOpposite(side);
	}
	public static Vector2Int GetOppositeDir (int side) { return GetDir(GetOppositeSide(side)); }
	/** Useful for flipping dirEntering to dirExiting, for example. Just returns the original value * -1. */
	public static Vector2Int GetOppositeDir (Vector2Int dir) { return new Vector2Int(-dir.x, -dir.y); }


	/*
	public static Vector3 SnapVector2ToGrid (Vector3 _vector) { return SnapVector2ToGrid (_vector.x, _vector.y); }
	public static Vector2 SnapVector2ToGrid (float _x, float _y) {
		float gridSize = EditModeGrid.gridSize;
		return new Vector2 (
			Mathf.Round (_x/gridSize) * gridSize,
			Mathf.Round (_y/gridSize) * gridSize);
	}
//	public static Vector3 SnapVector3ToGrid (Vector3 _vector) { return SnapVector3ToGrid (_vector.x, _vector.y, _vector.z); }
	public static Vector3 RoundVector3ToInts (Vector3 _vector) { return SnapVector3ToGrid (_vector, 1); }
	public static Vector3 SnapVector3ToGrid (Vector3 _vector) { return SnapVector3ToGrid (_vector, EditModeGrid.gridSize); }
	public static Vector3 SnapVector3ToGrid (Vector3 _vector, int _gridSize) {
		return new Vector3 (
			Mathf.Round (_vector.x/_gridSize) * _gridSize,
			Mathf.Round (_vector.y/_gridSize) * _gridSize,
			_vector.z);
	}
	*/

	public static void UpdateRectFromPoint(ref Rect rect, Vector2 point) {
		if (rect.xMin > point.x) { // LEFT
			rect.xMin = point.x;
		}
		if (rect.xMax < point.x) { // RIGHT
			rect.xMax = point.x;
		}
		if (rect.yMin > point.y) { // TOP
			rect.yMin = point.y;
		}
		if (rect.yMax < point.y) { // BOTTOM
			rect.yMax = point.y;
		}
	}

//	public static Rect GetMinScreenRectangle (Rect sourceRect) {
//		return GetMinScreenRectangle (sourceRect, Vector2.zero);
////		return GetCompoundRectangle (sourceRect, new Rect (-GameProperties.ORIGINAL_SIZE*0.5f, GameProperties.ORIGINAL_SIZE));
//	}
	/** Returns a rectangle at LEAST as big as our standard dimensions (1024x768).
	 sourceCenterPos: The center of the 1024x768 rectangle. So we can use this function for both locally and globally positioned rectangles. */
//	public static Rect GetMinScreenRectangle (Rect sourceRect, Vector2 sourceCenterPos) {
//		return GetCompoundRectangle (sourceRect, new Rect (sourceCenterPos-GameProperties.ORIGINAL_SIZE*0.5f, GameProperties.ORIGINAL_SIZE));
//	}

	public static Rect GetCompoundRectangle (Rect rectA, Rect rectB) {
		// FIRST, check if either of these rectangles are total 0's. If one IS, we want to NOT include it in the return value, so simply return the OTHER rectangle. So we don't include the origin (0,0) accidentally.
		if (rectA == new Rect ()) {
			return rectB;
		}
		if (rectB == new Rect ()) {
			return rectA;
		}
		// Otherwise, make a compound rectangle of the two :)
		Rect returnRect = new Rect (rectA);
		UpdateRectFromPoint (ref returnRect, rectB.max);
		UpdateRectFromPoint (ref returnRect, rectB.min);
		return returnRect;
	}
	
	/** t: current time
	 * Default all values to going from 0 to 1.
	 */
	static public float EaseInOutQuadInverse (float t) {
		t *= 2f;
		if (t < 1f) return Mathf.Pow (t, 1/3f) * 0.5f;
		t -= 2f;
		t *= -1f;
		return 1f - Mathf.Pow (t, 1/3f) * 0.5f;
	}
	
	/** t: current time
	 * Default all values to going from 0 to 1.
	 */
	static public float EaseInOutQuad (float t) {
		//		return EaseInOutQuad (t, 0,1,1);
		t *= 2f;
		if (t < 1f) return t*t*t * 0.5f;
		t -= 2f;
		return 1f + t*t*t * 0.5f;
	}
	/** t: current time
	 *  b: start value
	 *  c: change in value
	 *  d: duration
	 * */
	static public float EaseInOutQuad (float t, float b, float c, float d) {
		t /= d/2f;
		if (t < 1f) return c/2f*t*t*t + b;
		t -= 2f;
		return -c/2f * (t*t*t - 2f) + b;
	}
	
	/** t: current time
	 * Default all values to going from 0 to 1.
	 */
	static public float EaseInOutQuart (float t) {
		return EaseInOutQuart (t, 0,1,1);
	}
	/** t: current time
	 *  b: start value
	 *  c: change in value
	 *  d: duration
	 * */
	static public float EaseInOutQuart (float t, float b, float c, float d) {
		t /= d/2f;
		if (t < 1f) return c/2f*t*t*t*t + b;
		t -= 2f;
		return -c/2f * (t*t*t*t - 2f) + b;
	}


	
	/*
	public static int GetSelectableObjectIndexInDir (int currentlySelectedObjIndex, List<Vector2> objPoses, Vector2 dir) {
		// FIRST, if we've got NO object selected, return -1!
		if (currentlySelectedObjIndex == -1) {
			return -1;
		}
		float angle = Mathf.Atan2 (dir.y, dir.x);
		Vector2 currentObjPos = objPoses [currentlySelectedObjIndex];
		float bestFitAngleDifference = 999999; // just send it way up there
		float bestFitDistance = 9999999; // just send it way up there
		int bestFitObjIndex = -1;
		for (int i=0; i<objPoses.Count; i++) {
			if (i == currentlySelectedObjIndex) continue; // Don't check the one already selected hehe.
			Vector2 thisObjPos = objPoses [i];
			float thisDistance = Vector2.Distance(currentObjPos, thisObjPos);
			float thisAngle = Mathf.Atan2 (thisObjPos.y-currentObjPos.y, thisObjPos.x-currentObjPos.x);
			float angleDifference = Mathf.Abs(angle-thisAngle);
			if (angleDifference > Mathf.PI) angleDifference -= Mathf.PI*2; // Keep looped between -PI and PI, brah.
			// If this angleDifference is too large to matter, then fuggedaboudit!
			if (Mathf.Abs(angleDifference) > Mathf.PI*0.46f) continue;
			// If this object is CLOSER than the best-fit object...
			if (bestFitDistance > thisDistance) {
				// If the angle between my input and this object is CLOSER than for the best-fit object...
				if (bestFitAngleDifference > angleDifference) {
					bestFitDistance = thisDistance;
					bestFitAngleDifference = angleDifference;
					bestFitObjIndex = i;
				}
			}
		}
		return bestFitObjIndex;
	}
	*/

}




