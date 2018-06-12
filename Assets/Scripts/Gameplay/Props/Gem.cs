using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : Prop, ISerializableData<GemData> {
	// Components
	[SerializeField] private BoxCollider2D myCollider=null;
	[SerializeField] private ParticleSystem ps_collectedBurst=null;
	[SerializeField] private SpriteRenderer sr_body=null;


	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, GemData data) {
		base.BaseInitialize(_myLevel, data);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	private void OnTriggerEnter2D(Collider2D otherCol) {
		// Ground??
		if (LayerMask.LayerToName(otherCol.gameObject.layer) == Layers.Player) {
			GetCollected();
		}
	}
	private void GetCollected() {
		myCollider.enabled = false;
		sr_body.enabled = false;
		ps_collectedBurst.Emit(15);
	}



	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public GemData SerializeAsData() {
		GemData data = new GemData();
		data.pos = pos;
		return data;
	}

}
