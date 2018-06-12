using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiskers : PlatformCharacterWhiskers {
	override protected string[] GetLayerMaskNames_LRTB() {
		return new string[]{Layers.Default, Layers.Ground, Layers.Enemy};
	}
	override protected string[] GetLayerMaskNames_B() {
		return new string[]{Layers.Default, Layers.Ground, Layers.Enemy, Layers.Platform};
	}
}
