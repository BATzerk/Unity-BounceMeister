using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesHandler : MonoBehaviour {
	// References!
	[SerializeField] public GameObject imageLine;

	[SerializeField] public GameObject RoomJumpRoomButton;
	[SerializeField] public GameObject MapEditor_RoomTile;
    
    [SerializeField] public GameObject ClustSelListClustRow;
    [SerializeField] public GameObject ClustSelListClustRowSnack;
    [SerializeField] public GameObject ClustSelListWorldView;
    
    [SerializeField] public GameObject ClustSelMapClustTile;
    [SerializeField] public GameObject ClustSelMapRoomView;
    [SerializeField] public GameObject ClustSelMapWorldView;

	[SerializeField] public GameObject backgroundTileSprite;
    [SerializeField] public Sprite s_whiteSquare;
    [SerializeField] public Sprite s_gem0;
    [SerializeField] public Sprite s_gem1;
    [SerializeField] public Sprite s_battery=null;
    [SerializeField] public Sprite s_ground=null;
    [SerializeField] public Sprite s_snack=null;
    [SerializeField] public Sprite s_spikes=null;
    
    [SerializeField] public GameObject Room;
    [SerializeField] public GameObject MiniMapRoomTile;

    [SerializeField] private GameObject Clinga=null;
    [SerializeField] private GameObject Dilata=null;
    [SerializeField] private GameObject Flatline=null;
    [SerializeField] private GameObject Flippa=null;
    [SerializeField] private GameObject Freeza=null;
    [SerializeField] private GameObject Jetta=null;
    [SerializeField] private GameObject Jumpa=null;
    [SerializeField] private GameObject Limo=null;
    [SerializeField] private GameObject Neutrala=null;
    [SerializeField] private GameObject Plunga=null;
    [SerializeField] private GameObject Slippa=null;
    [SerializeField] private GameObject Testa=null;
    [SerializeField] private GameObject Warpa=null;
    
    [SerializeField] public GameObject Dweeb;

    [SerializeField] public GameObject Battery;
    [SerializeField] public GameObject Buzzsaw;
	[SerializeField] public GameObject CameraBounds;
    [SerializeField] public GameObject CharBarrel;
    [SerializeField] public GameObject CharUnlockOrb;
	[SerializeField] public GameObject Coin;
	[SerializeField] public GameObject Crate;
	[SerializeField] public GameObject DispGround;
	[SerializeField] public GameObject Gate;
	[SerializeField] public GameObject GateButton;
	[SerializeField] public GameObject Gem;
	[SerializeField] public GameObject Ground;
    [SerializeField] public GameObject InfoSign;
    [SerializeField] public GameObject Laser;
    [SerializeField] public GameObject Lift;
    [SerializeField] public GameObject Platform;
	[SerializeField] public GameObject PlayerStart;
    [SerializeField] public GameObject ProgressGate;
	[SerializeField] public GameObject RoomDoor;
    [SerializeField] public GameObject Snack;
	[SerializeField] public GameObject Spikes;
	[SerializeField] public GameObject ToggleGround;
	[SerializeField] public GameObject Turret;
	[SerializeField] public GameObject TurretBullet;
    [SerializeField] public GameObject TurretBulletBurst;
    [SerializeField] public GameObject Veil;


    // Getters
    public GameObject Player(PlayerTypes type) {
        switch (type) {
            case PlayerTypes.Clinga: return Clinga;
            case PlayerTypes.Dilata: return Dilata;
            case PlayerTypes.Flatline: return Flatline;
            case PlayerTypes.Flippa: return Flippa;
            case PlayerTypes.Freeza: return Freeza;
            case PlayerTypes.Jetta: return Jetta;
            case PlayerTypes.Jumpa: return Jumpa;
            case PlayerTypes.Limo: return Limo;
            case PlayerTypes.Neutrala: return Neutrala;
            case PlayerTypes.Plunga: return Plunga;
            case PlayerTypes.Slippa: return Slippa;
            case PlayerTypes.Testa: return Testa;
            case PlayerTypes.Warpa: return Warpa;
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
			Destroy(this);
		}
	}
}
