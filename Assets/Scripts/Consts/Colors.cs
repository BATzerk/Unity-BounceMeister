using UnityEngine;

public class Colors {
	static public Color GetBeamColor (int worldIndex, int channel) {
		switch (worldIndex) {
			// -- World 0
			case 0:
				switch (channel) {
					case 0: return ColorUtils.HexToColor("F92070");
					case 1: return ColorUtils.HexToColor("1FF0A1");
					case 2: return ColorUtils.HexToColor("91FA20");
					case 3: return ColorUtils.HexToColor("F0FE21");
				}
				break;
			// -- World 1
			case 1:
				switch (channel) {
					case 0: return new ColorHSB (318/360f, 62/100f, 69/100f).ToColor();
					case 1: return new ColorHSB ( 51/360f, 75/100f, 97/100f).ToColor();
					case 2: return new ColorHSB ( 84/360f, 75/100f, 96/100f).ToColor();
					case 3: return new ColorHSB (  8/360f, 77/100f, 80/100f).ToColor();
				}
				break;
			// -- World 2
			case 2:
				switch (channel) {
					case 0: return ColorUtils.HexToColor("8F2FF0");
					case 1: return ColorUtils.HexToColor("28B2EE");
					case 2: return ColorUtils.HexToColor("FFC721");
					case 3: return ColorUtils.HexToColor("3F3FF1");
				}
				break;
			// -- World 3
			case 3:
				switch (channel) {
					case 0: return ColorUtils.HexToColor("8F2FF0");
					case 1: return ColorUtils.HexToColor("28B2EE");
					case 2: return ColorUtils.HexToColor("FFC721");
					case 3: return ColorUtils.HexToColor("3F3FF1");
				}
				break;
			// -- Default
			default:
				switch (channel) {
					case 0: return new ColorHSB (353/360f, 74/100f, 49/100f).ToColor();
					case 1: return new ColorHSB ( 33/360f, 75/100f, 88/100f).ToColor();
					case 2: return new ColorHSB (320/360f, 74/100f, 49/100f).ToColor();
					case 3: return new ColorHSB (  8/360f, 77/100f, 80/100f).ToColor();
				}
				break;
		}
		return Color.black; // oops
	}


	static public Color GetBoardSpaceColor (int worldIndex, bool isSpaceEven) {
//		Color returnColor = Color.red;
		float hue=0;
		switch (worldIndex) {
			case 0: hue = 220/360f; break;
			case 1: hue = 30/360f; break;
			case 2: hue = 114/360f; break;
			case 3: hue = 280/360f; break;
			case 4: hue = 320/360f; break;
		}
		// Every other space is a little darker!
		if (isSpaceEven) {
			return new ColorHSB (hue, 0.12f, 0.96f).ToColor();
//			returnColor = new Color (returnColor.r*0.97f, returnColor.g*0.97f, returnColor.b*0.97f, returnColor.a);
		}
		return new ColorHSB (hue, 0.02f, 1f).ToColor();
//		return returnColor;
	}

	public static Color GetPortalColor (int channelID) {
		switch (channelID) {
			case 0: return new Color (0.6f,1f,0.6f);
			case 1: return new Color (1f,0.6f,1f);
			default: return Color.red; // Oops.
		}
	}
}
