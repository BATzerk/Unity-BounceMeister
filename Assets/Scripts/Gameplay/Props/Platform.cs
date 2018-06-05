using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : BaseGround, ISerializableData<PlatformData> {



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, PlatformData data) {
		base.BaseGroundInitialize(_myLevel, data);
	}

	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public PlatformData SerializeAsData() {
		PlatformData data = new PlatformData();
		data.myRect = MyRect;
		return data;
	}
}
