using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Sides {
	public const int Undefined = -1;
	public const int T = 0;
	public const int R = 1;
	public const int B = 2;
	public const int L = 3;
	public const int TL = 4;
	public const int TR = 5;
	public const int BL = 6;
	public const int BR = 7;

	public const int Min = 0;
	public const int Max = 8;


	static public int GetOpposite(int side) {
		switch (side) {
			case Sides.L: return Sides.R;
			case Sides.R: return Sides.L;
			case Sides.B: return Sides.T;
			case Sides.T: return Sides.B;
			case Sides.TL: return Sides.BR;
			case Sides.TR: return Sides.BL;
			case Sides.BL: return Sides.TR;
			case Sides.BR: return Sides.TL;
			default: throw new UnityException ("Whoa, " + side + " is not a valid side. Try 0 through 7.");
		}
	}
	static public int GetHorzFlipped(int side) {
		switch (side) {
			case L: return R;
			case R: return L;
			case TL: return TR;
			case TR: return TL;
			case BL: return BR;
			case BR: return BL;
			default: return side; // this side isn't affected by a flip.
		}
	}
	static public int GetVertFlipped(int side) {
		switch (side) {
			case T: return B;
			case B: return T;
			case TL: return BL;
			case TR: return BR;
			case BL: return TL;
			case BR: return TR;
			default: return side; // this side isn't affected by a flip.
		}
	}
}
