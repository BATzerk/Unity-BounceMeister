using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesHandler : MonoBehaviour {
	// References!
	[SerializeField] public GameObject imageLine;

	[SerializeField] public GameObject levelSelectLevelButton;

	[SerializeField] public GameObject backgroundTileSprite;

//	[SerializeField] public GameObject Level;
	[SerializeField] public GameObject CameraBounds;

	[SerializeField] public GameObject Battery;
	[SerializeField] public GameObject Coin;
	[SerializeField] public GameObject Crate;
	[SerializeField] public GameObject DamageableGround;
	[SerializeField] public GameObject Gem;
	[SerializeField] public GameObject Ground;
	[SerializeField] public GameObject LevelDoor;
	[SerializeField] public GameObject Lift;
	[SerializeField] public GameObject Player;
	[SerializeField] public GameObject Spikes;
	[SerializeField] public GameObject ToggleGround;


	// Instance
	static private ResourcesHandler instance;
	static public ResourcesHandler Instance { get { return instance; } }

	// Awake
	private void Awake () {
		// There can only be one (instance)!
		if (instance == null) {
			instance = this;
		}
		else {
			GameObject.Destroy (this);
		}
	}
}
