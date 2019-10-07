using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiskers : PlatformCharacterWhiskers {
	override protected string[] GetLayerMaskNames_LRTB() {
		return new string[]{Layers.Ground, Layers.Obstacle, Layers.Enemy, Layers.Coin};
	}
	override protected string[] GetLayerMaskNames_B() {
		return new string[]{Layers.Ground, Layers.Obstacle, Layers.Enemy, Layers.Platform, Layers.Coin};
	}
}
