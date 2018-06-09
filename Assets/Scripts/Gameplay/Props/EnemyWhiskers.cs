using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWhiskers : PlatformCharacterWhiskers {
	override protected string[] GetLayerMaskNames_LRTB() {
		return new string[]{"Ground"};
	}
	override protected string[] GetLayerMaskNames_B() {
		return new string[]{"Ground", "Platform"};
	}
}


