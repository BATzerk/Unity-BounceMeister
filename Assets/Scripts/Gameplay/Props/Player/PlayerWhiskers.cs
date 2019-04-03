using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiskers : PlatformCharacterWhiskers {
	//Layers.Default, 
	override protected string[] GetLayerMaskNames_LRTB() {
		return new string[]{Layers.Ground, Layers.Enemy};
	}
	override protected string[] GetLayerMaskNames_B() {
		return new string[]{Layers.Ground, Layers.Enemy, Layers.Platform};
	}
}
