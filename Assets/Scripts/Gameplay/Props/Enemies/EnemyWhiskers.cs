using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWhiskers : PlatformCharacterWhiskers {
	//Layers.Default, 
	override protected string[] GetLayerMaskNames_LRTB() {
		return new string[]{Layers.Ground};
	}
	override protected string[] GetLayerMaskNames_B() {
		return new string[]{Layers.Ground, Layers.Platform};
	}
}


