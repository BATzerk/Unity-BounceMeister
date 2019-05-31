using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BaseGridSnap : MonoBehaviour {
    

	protected float UnitSize { get { return GameProperties.UnitSize; } }
	protected Vector3 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
	protected float rotation { get { return this.transform.localEulerAngles.z; } }


}
