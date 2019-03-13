using UnityEngine;
using System.Collections;

public class LineUtils {


	// ----------------------------------------------------------------
	//  Basic Getters
	// ----------------------------------------------------------------
	static public float GetAngle_Degrees (Vector2 pointA, Vector2 pointB) {
		return Mathf.Rad2Deg * Mathf.Atan2(pointB.y-pointA.y, pointB.x-pointA.x);
	}
	public static float GetLength (Vector2 lineStart,Vector2 lineEnd) {
		return Vector2.Distance(lineStart, lineEnd);
	}
	public static Vector2 GetCenterPos (Vector2 lineStart,Vector2 lineEnd) {
		return Vector2.Lerp (lineStart,lineEnd, 0.5f);
	}


	// ----------------------------------------------------------------
	//  Intersections!
	// ----------------------------------------------------------------
	static public bool GetIntersectionLineToLine (out Vector2 intPos, Line lineA, Line lineB) {
		return GetIntersectionLineToLine (out intPos, lineA.start,lineA.end, lineB.start,lineB.end);
	}
	static public bool GetIntersectionLineToLine (out Vector2 intPos, Vector2 lineAStart,Vector2 lineAEnd, Vector2 lineBStart,Vector2 lineBEnd) {
		return GetIntersectionLineToLine (out intPos, lineAStart.x,lineAStart.y, lineAEnd.x,lineAEnd.y, lineBStart.x,lineBStart.y, lineBEnd.x,lineBEnd.y);
	}
	static public bool GetIntersectionLineToLine (out Vector2 intPos, float x1,float y1, float x2,float y2, float x3,float y3, float x4,float y4) {
		// FIRST, check if these lines end/begin at each other. If so, return THAT point.
		const float samePosThresh = 0.1f; // if any of the poses of these lines are closer than this to each other, SNAP the intersection to EXACTLY that point!
		if ((Mathf.Abs(x1-x3)<samePosThresh && Mathf.Abs(y1-y3)<samePosThresh) // A's START is B's START...
		 || (Mathf.Abs(x1-x4)<samePosThresh && Mathf.Abs(y1-y4)<samePosThresh) // A's START is B's END...
		    ){
			intPos = new Vector2(x1,y1); // NOTE: Maybe try rounding these values? Sometimes, they're not perfect...
			return true;
		}
		if ((Mathf.Abs(x2-x3)<samePosThresh && Mathf.Abs(y2-y3)<samePosThresh) // A's END is B's START...
		 || (Mathf.Abs(x2-x4)<samePosThresh && Mathf.Abs(y2-y4)<samePosThresh) // A's END is B's END...
		    ){
			intPos = new Vector2(x2,y2);
			return true;
		}
		
		double x,y; // intersection location
		double a1,a2, b1, b2, c1, c2;
		double r1, r2 , r3, r4;
		double denom, offset, num;
		
		// Compute a1, b1, c1, where line joining points 1 and 2
		// is "a1 x + b1 y + c1 = 0".
		a1 = y2 - y1;
		b1 = x1 - x2;
		c1 = (x2 * y1) - (x1 * y2);
		
		// Compute r3 and r4.
		r3 = ((a1 * x3) + (b1 * y3) + c1);
		r4 = ((a1 * x4) + (b1 * y4) + c1);
		
		// Check signs of r3 and r4. If both point 3 and point 4 lie on
		// same side of line 1, the line segments do not intersect.
		if ((r3 != 0) && (r4 != 0) && MathUtils.IsSameSign(r3, r4)) {
            intPos = MathUtils.Vector2NaN;
			return false;
		}
		
		// Compute a2, b2, c2
		a2 = y4 - y3;
		b2 = x3 - x4;
		c2 = (x4 * y3) - (x3 * y4);
		
		// Compute r1 and r2
		r1 = (a2 * x1) + (b2 * y1) + c2;
		r2 = (a2 * x2) + (b2 * y2) + c2;
		
		// Check signs of r1 and r2. If both point 1 and point 2 lie
		// on same side of second line segment, the line segments do
		// not intersect.
		if ((r1 != 0) && (r2 != 0) && (MathUtils.IsSameSign(r1, r2))) {
            intPos = MathUtils.Vector2NaN;
			return false;
		}
		
		//Line segments intersect: compute intersection point.
		denom = (a1 * b2) - (a2 * b1);
		
		if (denom == 0) {
            intPos = MathUtils.Vector2NaN;
			return false; // colinear
		}
		
		if (denom < 0) {
			offset = -denom / 2; 
		} 
		else {
			offset = denom / 2 ;
		}
		
		// The denom/2 is to get rounding instead of truncating. It
		// is added or subtracted to the numerator, depending upon the
		// sign of the numerator.
		num = (b1 * c2) - (b2 * c1);
		if (num < 0) {
			x = (num - offset) / denom;
		} 
		else {
			x = (num + offset) / denom;
		}
		
		num = (a2 * c1) - (a1 * c2);
		if (num < 0) {
			y = ( num - offset) / denom;
		} 
		else {
			y = (num + offset) / denom;
		}
		
		// Lines intersect!
//		intPos = new Vector3(Mathf.Round((float)x), Mathf.Round((float)y)); // Round these values... NOTE: I've found that rounding them PREVENTS bugs. No known bugs caused from rounding.
		intPos = new Vector2((float)x,(float)y);
		return true;
    }

    static public bool GetNonEndIntersectionLineToLine (out Vector2 intPos, Line lineA, Line lineB) {
        return GetNonEndIntersectionLineToLine (out intPos, lineA.start,lineA.end, lineB.start,lineB.end);
    }
    static public bool GetNonEndIntersectionLineToLine (out Vector2 intPos, Vector2 lineAStart,Vector2 lineAEnd, Vector2 lineBStart,Vector2 lineBEnd) {
        return GetNonEndIntersectionLineToLine (out intPos, lineAStart.x,lineAStart.y, lineAEnd.x,lineAEnd.y, lineBStart.x,lineBStart.y, lineBEnd.x,lineBEnd.y);
    }
    /** IGNORES intersections at the very tippy ends of lines! Only counts the middle area. */
    static public bool GetNonEndIntersectionLineToLine (out Vector2 intPos, float x1,float y1, float x2,float y2, float x3,float y3, float x4,float y4) {
        // FIRST, check if these lines end/begin at each other. If so, say they DON'T intersect!
        const float samePosThresh = 1f; // if any of the poses of these lines are closer than this to each other, SNAP the intersection to EXACTLY that point!
        if ((Mathf.Abs(x1-x3)<samePosThresh && Mathf.Abs(y1-y3)<samePosThresh) // A's START is B's START...
         || (Mathf.Abs(x1-x4)<samePosThresh && Mathf.Abs(y1-y4)<samePosThresh) // A's START is B's END...
            ){
            intPos = MathUtils.Vector2NaN;
            return false;
        }
        if ((Mathf.Abs(x2-x3)<samePosThresh && Mathf.Abs(y2-y3)<samePosThresh) // A's END is B's START...
         || (Mathf.Abs(x2-x4)<samePosThresh && Mathf.Abs(y2-y4)<samePosThresh) // A's END is B's END...
           ){
            intPos = MathUtils.Vector2NaN;
            return false;
        }
        // Ok, we can just return whatever the usual line-intersection result is.
        return GetIntersectionLineToLine(out intPos, x1,y1, x2,y2, x3,y3, x4,y4);
    }



}




