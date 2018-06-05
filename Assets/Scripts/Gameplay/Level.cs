using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour, ISerializableData<LevelData> {
	// Components
//	private CameraBounds cameraBounds;//[SerializeField] 
	// References
	public LevelData levelDataRef; //QQQ private
	public WorldData worldDataRef; //QQQ private

	// Getters (Public)
	public LevelData LevelDataRef { get { return levelDataRef; } }
	public WorldData WorldDataRef { get { return worldDataRef; } }
	public int WorldIndex { get { return levelDataRef.WorldIndex; } }
	public string LevelKey { get { return levelDataRef.LevelKey; } }
	public Vector2 PosGlobal { get { return levelDataRef.PosGlobal; } }
	public Vector2 PosWorld { get { return levelDataRef.PosWorld; } }
	public Rect GetCameraBoundsRect() {
		CameraBounds cameraBounds = GetComponentInChildren<CameraBounds>();
		if (cameraBounds != null) {
			return cameraBounds.MyRect;
		}
		return new Rect(0,0, 20,20);
	}
	// Getters (Private)
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }


	// ----------------------------------------------------------------
	//	Serialization
	// ----------------------------------------------------------------
	public LevelData SerializeAsData () {
		LevelData ld = new LevelData (WorldIndex, LevelKey);

		// -- General Properties --
		ld.SetPosGlobal (PosGlobal, false);
		ld.SetPosWorld (PosGlobal-WorldDataRef.CenterPos); // NOTE: Is this technically redundant? Don't we want to not care about storing this value, and have it always made fresh by our WorldDataRef?
		ld.SetDesignerFlag (levelDataRef.designerFlag, false);
//		ld.hasPlayerBeenHere = hasPlayerBeenHere;
		ld.isConnectedToStart = levelDataRef.isConnectedToStart;

		// -- Props --
		Battery[] batteries = GetComponentsInChildren<Battery>();
		foreach (Battery obj in batteries) {
			ld.batteryDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		CameraBounds cameraBounds = GetComponentInChildren<CameraBounds>();
		if (cameraBounds != null) {
			ld.cameraBoundsData = cameraBounds.SerializeAsData();
		}
//		ConditionalGround[] conditionalGrounds = GetComponentsInChildren<ConditionalGround>();
//		foreach (ConditionalGround obj in conditionalGrounds) {
//			ld.conditionalGroundDatas.Add(obj.SerializeAsData());
//		ld.allPropDatas.Add(obj.SerializeAsData());
//		}
		Crate[] crates = GetComponentsInChildren<Crate>();
		foreach (Crate obj in crates) {
			ld.crateDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		DamageableGround[] damageableGrounds = GetComponentsInChildren<DamageableGround>();
		foreach (DamageableGround obj in damageableGrounds) {
			ld.damageableGroundDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		Gem[] gems = GetComponentsInChildren<Gem>();
		foreach (Gem obj in gems) {
			ld.gemDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		Ground[] grounds = GetComponentsInChildren<Ground>();
		foreach (Ground obj in grounds) {
			ld.groundDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		LevelDoor[] levelDoors = GetComponentsInChildren<LevelDoor>();
		foreach (LevelDoor obj in levelDoors) {
			ld.levelDoorDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		Lift[] lifts = GetComponentsInChildren<Lift>();
		foreach (Lift obj in lifts) {
			ld.liftDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		Platform[] platforms = GetComponentsInChildren<Platform>();
		foreach (Platform obj in platforms) {
			ld.platformDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		Spikes[] spikes = GetComponentsInChildren<Spikes>();
		foreach (Spikes obj in spikes) {
			ld.spikesDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}
		ToggleGround[] toggleGrounds = GetComponentsInChildren<ToggleGround>();
		foreach (ToggleGround obj in toggleGrounds) {
			ld.toggleGroundDatas.Add(obj.SerializeAsData());
			ld.allPropDatas.Add(obj.SerializeAsData());
		}

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
//		gameControllerRef = _gameControllerRef;
		levelDataRef = _levelDataRef;
		worldDataRef = dataManager.GetWorldData(levelDataRef.worldIndex);
		this.gameObject.name = levelDataRef.LevelKey;

//		// HACK: If we loaded up a level from save that NEVER had its PosWorld set, then set it here. Otherwise it'll be 0,0.
//		if (levelDataRef.PosWorld == Vector2.zero) {
//			levelDataRef.SetPosWorld (levelDataRef.posGlobal - worldDataRef.CenterPos);
//		}

		this.transform.SetParent (tf_world); // Parent me to my world!
		this.transform.localScale = Vector3.one; // Make sure my scale is 1 from the very beginning.
		this.transform.localPosition = Vector3.zero;//PosWorld; // Position me!


		// Instantiate my props!
		LevelData ld = levelDataRef;
		ResourcesHandler rh = ResourcesHandler.Instance;

		// CameraBounds
		CameraBounds cameraBounds = Instantiate(ResourcesHandler.Instance.CameraBounds).GetComponent<CameraBounds>();
		cameraBounds.Initialize(this, ld.cameraBoundsData);


		foreach (PropData propData in ld.allPropDatas) {
			// Grounds
			if (propData is CrateData) {
				Crate newProp = Instantiate(rh.Crate).GetComponent<Crate>();
				newProp.Initialize (this, propData as CrateData);
			}
//			else if (propData is ConditionalGround) {
//				ConditionalGround newProp = Instantiate(rh.ConditionalGround).GetComponent<ConditionalGround>();
//				newProp.Initialize (this, propData as ConditionalGroundData);
//			}
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
			else if (propData is SpikesData) {
				Spikes newProp = Instantiate(rh.Spikes).GetComponent<Spikes>();
				newProp.Initialize (this, propData as SpikesData);
			}
			else {
				Debug.LogWarning("PropData not recognized: " + propData);
			}
		}

//		// LevelDoor
//		foreach (LevelDoorData data in ld.levelDoorDatas) {
//			LevelDoor newProp = Instantiate(rh.LevelDoor).GetComponent<LevelDoor>();
//			newProp.Initialize (this, data);
//		}
	}

	/** Slightly sloppy, whatever-it-takes housekeeping to allow us to start up the game with a novel level and edit/play/save it right off the bat. */
	public void InitializeAsPremadeLevel(GameController _gameControllerRef) {
		// TEMP
		levelDataRef = new LevelData(0, "Level0");
		worldDataRef = dataManager.GetWorldData(0);

		// Initialize things!
		// CameraBounds
		CameraBounds cameraBounds = Instantiate(ResourcesHandler.Instance.CameraBounds).GetComponent<CameraBounds>();
		cameraBounds.Initialize(this, cameraBounds.SerializeAsData());

	}



}









