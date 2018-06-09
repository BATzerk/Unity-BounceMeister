using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWhiskers : PlatformCharacterWhiskers {
	override protected string[] GetLayerMaskNames_LRTB() {
		return new string[]{"Ground", "Enemy"};
	}
	override protected string[] GetLayerMaskNames_B() {
		return new string[]{"Ground", "Enemy", "Platform"};
	}
}
