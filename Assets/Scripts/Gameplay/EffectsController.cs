using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsController : MonoBehaviour {
	// Components
	[SerializeField] private ParticleSystem ps_playerBurst;
	
	private void Start () {
		// Add event listeners!
	}
	private void OnDestroy() {
		// Add event listeners!
	}


//	private void OnPlayerStompPlayer(Player p0, Player p1) {
//		ps_playerBurst.transform.localPosition = p1.transform.localPosition;
//		GameUtils.SetParticleSystemColor(ps_playerBurst, GameProperties.PlayerColor(p1.PlayerIndex));
//		ps_playerBurst.Emit(40);
//	}
}
