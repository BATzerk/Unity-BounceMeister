using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour, ISerializableData<LevelData> {
	// References
	private GameController gameControllerRef;
	private LevelData levelDataRef;

	// Getters (Public)
	public LevelData LevelDataRef { get { return levelDataRef; } }
	public WorldData WorldDataRef { get { return levelDataRef.WorldDataRef; } }
	public int WorldIndex { get { return levelDataRef.WorldIndex; } }
	public string LevelKey { get { return levelDataRef.LevelKey; } }
	public Vector2 PosGlobal { get { return levelDataRef.PosGlobal; } }
//	public Vector2 PosWorld { get { return levelDataRef.PosWorld; } }
	public Rect GetCameraBoundsLocal() {
		CameraBounds cameraBounds = GetComponentInChildren<CameraBounds>();
		if (cameraBounds != null) {
			return new Rect(cameraBounds.RectLocal);
		}
		return new Rect(0,0, 20,20);
	}
	public Rect GetCameraBoundsGlobal() {
		Rect rLocal = GetCameraBoundsLocal();
		rLocal.center += PosGlobal; // shift it to global coordinates!
		return rLocal;
	}
	// Getters (Private)
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private Player playerRef { get { return gameControllerRef.Player; } }


	// ----------------------------------------------------------------
	//	Serialization
	// ----------------------------------------------------------------
	public LevelData SerializeAsData () {
		LevelData ld = new LevelData (WorldIndex, LevelKey);

		// -- General Properties --
		ld.SetPosGlobal (PosGlobal, false);
//		ld.SetPosWorld (PosGlobal);//-WorldDataRef.CenterPos); // NOTE: Is this technically redundant? Don't we want to not care about storing this value, and have it always made fresh by our WorldDataRef?
		ld.SetDesignerFlag (levelDataRef.designerFlag, false);
//		ld.hasPlayerBeenHere = hasPlayerBeenHere;
		ld.isConnectedToStart = levelDataRef.isConnectedToStart;

		// -- Props --
		Battery[] batteries = GameObject.FindObjectsOfType<Battery>();
		foreach (Battery obj in batteries) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		CameraBounds cameraBounds = GameObject.FindObjectOfType<CameraBounds>();
		if (cameraBounds != null) { ld.cameraBoundsData = cameraBounds.SerializeAsData(); }

		Crate[] crates = GameObject.FindObjectsOfType<Crate>();
		foreach (Crate obj in crates) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		DamageableGround[] damageableGrounds = GameObject.FindObjectsOfType<DamageableGround>();
		foreach (DamageableGround obj in damageableGrounds) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		Gem[] gems = GameObject.FindObjectsOfType<Gem>();
		foreach (Gem obj in gems) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		Ground[] grounds = GameObject.FindObjectsOfType<Ground>();
		foreach (Ground obj in grounds) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		LevelDoor[] levelDoors = GameObject.FindObjectsOfType<LevelDoor>();
		foreach (LevelDoor obj in levelDoors) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		Lift[] lifts = GameObject.FindObjectsOfType<Lift>();
		foreach (Lift obj in lifts) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		Platform[] platforms = GameObject.FindObjectsOfType<Platform>();
		foreach (Platform obj in platforms) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		PlayerStart[] playerStarts = GameObject.FindObjectsOfType<PlayerStart>();
		foreach (PlayerStart obj in playerStarts) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		Spikes[] spikes = GameObject.FindObjectsOfType<Spikes>();
		foreach (Spikes obj in spikes) { ld.allPropDatas.Add(obj.SerializeAsData()); }

		ToggleGround[] toggleGrounds = GameObject.FindObjectsOfType<ToggleGround>();
		foreach (ToggleGround obj in toggleGrounds) { ld.allPropDatas.Add(obj.SerializeAsData()); }

//		// Streets
//		for (int i=0; i<streets.Count; i++) {
//			System.Type propType = streets[i].GetType();
//			if (propType == typeof(OneWayStreet)) {
//				ld.oneWayStreetDatas.Add ((streets[i] as OneWayStreet).SerializeAsData());
//			}
//			else if (propType == typeof(MovingStreet)) {
//				ld.movingStreetDatas.Add ((streets[i] as MovingStreet).SerializeAsData());
//			}
//			else if (propType == typeof(SegueStreet)) { } // DON'T serialize SegueStreets, yo. It IS inconsistent, BUT it ensures we don't have mismatched wirings between levels when we're editing stuff.
//			else {
//				ld.streetDatas.Add (streets[i].SerializeAsData());
//			}
//		}
//		// Bombs
//		foreach (Bomb prop in bombs) {
//			ld.bombDatas.Add (prop.SerializeAsData());
//		}

		return ld;
	}


	// ----------------------------------------------------------------
	//	Initialize
	// ----------------------------------------------------------------
	public void Initialize (GameController _gameControllerRef, Transform tf_world, LevelData _levelDataRef) {
		gameControllerRef = _gameControllerRef;
		levelDataRef = _levelDataRef;
		this.gameObject.name = levelDataRef.LevelKey;

		this.transform.SetParent (tf_world); // Parent me to my world!
		this.transform.localScale = Vector3.one; // Make sure my scale is 1 from the very beginning.
		this.transform.localPosition = PosGlobal; // Position me!

		// Instantiate my props!
		LevelData ld = levelDataRef;
		ResourcesHandler rh = ResourcesHandler.Instance;

//		// CameraBounds
//		CameraBounds cameraBounds = Instantiate(ResourcesHandler.Instance.CameraBounds).GetComponent<CameraBounds>();
//		cameraBounds.Initialize(this, ld.cameraBoundsData);

		foreach (PropData propData in ld.allPropDatas) {
			// Grounds
			if (propData is CrateData) {
				Crate newProp = Instantiate(rh.Crate).GetComponent<Crate>();
				newProp.Initialize (this, propData as CrateData);
			}
			else if (propData is DamageableGroundData) {
				DamageableGround newProp = Instantiate(rh.DamageableGround).GetComponent<DamageableGround>();
				newProp.Initialize (this, propData as DamageableGroundData);
			}
			else if (propData is ToggleGroundData) {
				ToggleGround newProp = Instantiate(rh.ToggleGround).GetComponent<ToggleGround>();
				newProp.Initialize (this, propData as ToggleGroundData);
			}
			else if (propData is PlatformData) {
				Platform newProp = Instantiate(rh.Platform).GetComponent<Platform>();
				newProp.Initialize (this, propData as PlatformData);
			}
			else if (propData is GroundData) {
				Ground newProp = Instantiate(rh.Ground).GetComponent<Ground>();
				newProp.Initialize (this, propData as GroundData);
			}
			// Everything else!
			else if (propData is BatteryData) {
				Battery newProp = Instantiate(rh.Battery).GetComponent<Battery>();
				newProp.Initialize (this, propData as BatteryData);
			}
			else if (propData is CameraBoundsData) {
				CameraBounds newProp = Instantiate(rh.CameraBounds).GetComponent<CameraBounds>();
				newProp.Initialize (this, propData as CameraBoundsData);
			}
			else if (propData is GemData) {
				Gem newProp = Instantiate(rh.Gem).GetComponent<Gem>();
				newProp.Initialize (this, propData as GemData);
			}
			else if (propData is LevelDoorData) {
				LevelDoor newProp = Instantiate(rh.LevelDoor).GetComponent<LevelDoor>();
				newProp.Initialize (this, propData as LevelDoorData);
			}
			else if (propData is LiftData) {
				Lift newProp = Instantiate(rh.Lift).GetComponent<Lift>();
				newProp.Initialize (this, propData as LiftData);
			}
			else if (propData is PlayerStartData) {
				PlayerStart newProp = Instantiate(rh.PlayerStart).GetComponent<PlayerStart>();
				newProp.Initialize (this, propData as PlayerStartData);
			}
			else if (propData is SpikesData) {
				Spikes newProp = Instantiate(rh.Spikes).GetComponent<Spikes>();
				newProp.Initialize (this, propData as SpikesData);
			}
			else {
				Debug.LogWarning("PropData not recognized: " + propData);
			}
		}

		AutoAddSilentBoundaries();
	}

	/** Slightly sloppy, whatever-it-takes housekeeping to allow us to start up the game with a novel level and edit/play/save it right off the bat. */
	public void InitializeAsPremadeLevel(GameController _gameControllerRef) {
		gameControllerRef = _gameControllerRef;

		// TEMP totes hacky, yo.
		string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		levelDataRef = new LevelData(0, sceneName); // NOTE! Be careful; we can easily overwrite levels like this.
		levelDataRef = SerializeAsData();

		// Initialize things!
		// Player
		PlayerData playerData = new PlayerData();
//		if (GameObject.FindObjectOfType<Player>() == null) {
//			playerRef = Instantiate(ResourcesHandler.Instance.Player).GetComponent<Player>();
//		}
		PlayerStart playerStart = GameObject.FindObjectOfType<PlayerStart>();
		if (playerStart != null) {
			playerData.pos = playerStart.PosLocal;
		}
		playerRef.Initialize(this, playerData);
		// CameraBounds
		if (GameObject.FindObjectOfType<CameraBounds>() == null) {
			CameraBounds cameraBounds = Instantiate(ResourcesHandler.Instance.CameraBounds).GetComponent<CameraBounds>();
			cameraBounds.Initialize(this, cameraBounds.SerializeAsData()); // Strange and hacky: It initializes itself as what it already is. Just to go through other paperwork.
		}
	}

	private void AutoAddSilentBoundaries() {
		Rect camBounds = levelDataRef.cameraBoundsData.myRect;
//		Rect viewRect
		for (int side=0; side<Sides.NumSides; side++) {
			// No level at this side?? Protect me with an InvisiBounds!
			if (WorldDataRef.GetLevelAtSide(levelDataRef, side) == null) {
				BoxCollider2D col = new GameObject().AddComponent<BoxCollider2D>();
				col.transform.SetParent(this.transform);
				col.transform.localScale = Vector3.one;
				col.transform.localEulerAngles = Vector3.zero;
//				col.gameObject.layer = LayerMask.NameToLayer("Ground"); // so our feet stop on it, yanno.
				col.name = "InvisiBounds_Side" + side;
				// Determine the collider's rect, ok?
				Rect rect = new Rect();
				switch(side) { // Ehh nbd. Easier to understand.
					case Sides.L:
						rect.size = new Vector2(1, camBounds.height);
						rect.position = new Vector2(camBounds.xMin-rect.size.x, camBounds.y);
						break;
					case Sides.R:
						rect.size = new Vector2(1, camBounds.height);
						rect.position = new Vector2(camBounds.xMax, camBounds.y);
						break;
					case Sides.B:
						rect.size = new Vector2(camBounds.width, 1);
						rect.position = new Vector2(camBounds.x, camBounds.yMin-rect.size.y);
						break;
					case Sides.T:
						rect.size = new Vector2(camBounds.width, 1);
						rect.position = new Vector2(camBounds.x, camBounds.yMax);
						break;
				}
				// Make it happen!
				col.transform.localPosition = rect.center;
				col.size = rect.size;
			}
		}
	}




	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	//	Editing
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public void FlipHorz() {
		Prop[] allProps = GameObject.FindObjectsOfType<Prop>();
		foreach (Prop prop in allProps) {
			prop.FlipHorz();
		}
	}



}









