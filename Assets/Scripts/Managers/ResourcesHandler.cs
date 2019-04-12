using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesHandler : MonoBehaviour {
	// References!
	[SerializeField] public GameObject imageLine;

	[SerializeField] public GameObject LevelJumpLevelButton;
	[SerializeField] public GameObject MapEditor_LevelTile;

	[SerializeField] public GameObject backgroundTileSprite;
    [SerializeField] public Sprite s_whiteSquare;
    [SerializeField] public Sprite s_gem0;
    [SerializeField] public Sprite s_gem1;
    
    [SerializeField] public GameObject Level;
    [SerializeField] public GameObject MiniMapLevelTile;

    [SerializeField] private GameObject Plunga;
	[SerializeField] private GameObject Slippa;
	[SerializeField] private GameObject Jetta;

	[SerializeField] public GameObject Battery;
	[SerializeField] public GameObject CameraBounds;
    [SerializeField] public GameObject CharBarrel;
	[SerializeField] public GameObject Coin;
	[SerializeField] public GameObject Crate;
	[SerializeField] public GameObject DamageableGround;
	[SerializeField] public GameObject Gate;
	[SerializeField] public GameObject GateButton;
	[SerializeField] public GameObject Gem;
	[SerializeField] public GameObject Ground;
	[SerializeField] public GameObject LevelDoor;
	[SerializeField] public GameObject Lift;
    [SerializeField] public GameObject Platform;
	[SerializeField] public GameObject PlayerStart;
    [SerializeField] public GameObject ProgressGate;
    [SerializeField] public GameObject Snack;
	[SerializeField] public GameObject Spikes;
	[SerializeField] public GameObject ToggleGround;


    // Getters
    public GameObject Player(PlayerTypes type) {
        switch (type) {
            case PlayerTypes.Jetta: return Jetta;
            case PlayerTypes.Plunga: return Plunga;
            case PlayerTypes.Slippa: return Slippa;
            default:
                Debug.LogError("Whoa! Player type totally not recognized: " + type);
                return null;
        }
    }
    public Sprite GetGemSprite(int type) {
        switch (type) {
            case 0: return s_gem0;
            case 1: return s_gem1;
            default: Debug.LogWarning("No sprite for Gem type: " + type); return null;
        }
    }
    public GameObject GetDecor(string prefabName) {
        return Resources.Load<GameObject>("Prefabs/Gameplay/Decor/" + prefabName);
    }


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
