using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour, ISerializableData<LevelData> {
	// Properties
	private GateChannel[] gateChannels;
    private List<Gem> gems=new List<Gem>();
    private List<Snack> snacks=new List<Snack>();
    // References
    private GameController gameControllerRef;
    public LevelData LevelDataRef { get; private set; }

    // Getters (Public)
    public WorldData WorldDataRef { get { return LevelDataRef.WorldDataRef; } }
	public int WorldIndex { get { return LevelDataRef.WorldIndex; } }
	public string LevelKey { get { return LevelDataRef.LevelKey; } }
	public Vector2 PosGlobal { get { return LevelDataRef.PosGlobal; } }
//	public Vector2 PosWorld { get { return levelDataRef.PosWorld; } }
	public GateChannel[] GateChannels { get { return gateChannels; } }
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
	public Vector2 Debug_PlayerStartPosLocal() {
		PlayerStart playerStart = GetComponentInChildren<PlayerStart>();
		if (playerStart != null) { return playerStart.PosLocal; }
		return Vector2.zero; // no PlayerStart? Ok, default to my center.
	}
	// Getters (Private)
	private DataManager dataManager { get { return GameManagers.Instance.DataManager; } }
	private Player playerRef { get { return gameControllerRef.Player; } }


	// ----------------------------------------------------------------
	//	Serialization
	// ----------------------------------------------------------------
	public LevelData SerializeAsData () {
		LevelData ld = new LevelData(WorldDataRef, LevelKey);

		// -- General Properties --
		ld.SetPosGlobal(PosGlobal);
		ld.SetDesignerFlag(LevelDataRef.designerFlag);
//		ld.hasPlayerBeenHere = hasPlayerBeenHere;
		ld.isConnectedToStart = LevelDataRef.isConnectedToStart;

		// -- Props --
        Prop[] allProps = FindObjectsOfType<Prop>();
        foreach (Prop prop in allProps) {
            if (!prop.DoSaveInLevelFile()) { continue; } // This type of Prop doesn't save to Level file? Skip it.
            ld.allPropDatas.Add(prop.SerializeAsData());
        }
        // HACK TEMP TODO: clean this up
        CameraBounds cameraBounds = FindObjectOfType<CameraBounds>();
        if (cameraBounds != null) { ld.cameraBoundsData = cameraBounds.SerializeAsData() as CameraBoundsData; }
        
        // Reverse the propDatas list so it's saved in the same order each time. (Kinda weird, but this is the easy solution.)
        ld.allPropDatas.Reverse();

		return ld;
	}


	// ----------------------------------------------------------------
	//	Initialize
	// ----------------------------------------------------------------
	public void Initialize (GameController _gameControllerRef, Transform tf_world, LevelData _levelDataRef) {
		gameControllerRef = _gameControllerRef;
		LevelDataRef = _levelDataRef;
		this.gameObject.name = LevelDataRef.LevelKey;

		this.transform.SetParent (tf_world); // Parent me to my world!
		this.transform.localScale = Vector3.one; // Make sure my scale is 1 from the very beginning.
		this.transform.localPosition = PosGlobal; // Position me!

		// Initialize channels!
		gateChannels = new GateChannel[5];
		for (int i=0; i<gateChannels.Length; i++) { gateChannels[i] = new GateChannel(this, i); }

		// Instantiate my props!
		LevelData ld = LevelDataRef;
		ResourcesHandler rh = ResourcesHandler.Instance;

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
			else if (propData is GateData) {
				Gate newProp = Instantiate(rh.Gate).GetComponent<Gate>();
				newProp.Initialize (this, propData as GateData);
				gateChannels[newProp.ChannelID].AddGate(newProp);
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
			else if (propData is GateButtonData) {
				GateButton newProp = Instantiate(rh.GateButton).GetComponent<GateButton>();
				newProp.Initialize (this, propData as GateButtonData);
				gateChannels[newProp.ChannelID].AddButton(newProp);
			}
			else if (propData is GemData) {
				Gem newProp = Instantiate(rh.Gem).GetComponent<Gem>();
				newProp.Initialize (this, propData as GemData, gems.Count);
                gems.Add(newProp);
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
            else if (propData is ProgressGateData) {
                ProgressGate newProp = Instantiate(rh.ProgressGate).GetComponent<ProgressGate>();
                newProp.Initialize (this, propData as ProgressGateData);
            }
            else if (propData is SnackData) {
                Snack newProp = Instantiate(rh.Snack).GetComponent<Snack>();
                newProp.Initialize (this, propData as SnackData, snacks.Count);
                snacks.Add(newProp);
            }
			else if (propData is SpikesData) {
				Spikes newProp = Instantiate(rh.Spikes).GetComponent<Spikes>();
				newProp.Initialize (this, propData as SpikesData);
			}
			else {
				Debug.LogWarning("PropData not recognized: " + propData);
			}
		}

		// For development, add bounds so we don't fall out of unconnected levels!
		AutoAddSilentBoundaries();
        AddHardcodedLevelElements();

		//// Reset channels!
		//foreach (GateChannel channel in gateChannels) { channel.Reset(); }
	}

	/** Slightly sloppy, whatever-it-takes housekeeping to allow us to start up the game with a novel level and edit/play/save it right off the bat. */
	public void InitializeAsPremadeLevel(GameController _gameControllerRef) {
		gameControllerRef = _gameControllerRef;

		// TEMP totes hacky, yo.
		string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		LevelDataRef = new LevelData(WorldDataRef, sceneName); // NOTE! Be careful; we can easily overwrite levels like this.
		LevelDataRef = SerializeAsData();

		// Initialize things!
		// Player
		PlayerData playerData = new PlayerData();
		PlayerStart playerStart = FindObjectOfType<PlayerStart>();
		if (playerStart != null) {
			playerData.pos = playerStart.PosLocal;
		}
		playerRef.Initialize(this, playerData);
		// CameraBounds
		if (FindObjectOfType<CameraBounds>() == null) {
			CameraBounds cameraBounds = Instantiate(ResourcesHandler.Instance.CameraBounds).GetComponent<CameraBounds>();
			cameraBounds.Initialize(this, cameraBounds.SerializeAsData() as CameraBoundsData); // Strange and hacky: It initializes itself as what it already is. Just to go through other paperwork.
		}
	}
    /*
	private void AutoAddSilentBoundaries() {
		Rect camBounds = levelDataRef.cameraBoundsData.myRect;
		for (int side=0; side<Sides.NumSides; side++) {
			// No level at this side?? Protect me with an InvisiBounds!
			if (WorldDataRef.Debug_GetSomeLevelAtSide(levelDataRef, side) == null) {
				BoxCollider2D col = new GameObject().AddComponent<BoxCollider2D>();
				col.transform.SetParent(this.transform);
				col.transform.localScale = Vector3.one;
				col.transform.localEulerAngles = Vector3.zero;
				col.gameObject.layer = LayerMask.NameToLayer("Ground"); // so our feet stop on it, yanno.
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
    */
	private void AutoAddSilentBoundaries() {
		for (int i=0; i<LevelDataRef.Neighbors.Count; i++) {
			// No level here?? Protect me with an InvisiBounds!
            LevelNeighborData lnd = LevelDataRef.Neighbors[i];
            if (!lnd.IsLevelTo) {
				BoxCollider2D col = new GameObject().AddComponent<BoxCollider2D>();
				col.transform.SetParent(this.transform);
				col.transform.localScale = Vector3.one;
				col.transform.localEulerAngles = Vector3.zero;
				col.gameObject.layer = LayerMask.NameToLayer("Ground"); // so our feet stop on it, yanno.
				col.name = "Invisibounds";
                // Determine the collider's rect, ok?
                Rect rect = new Rect {
                    size = GetInvisiboundSize(lnd.OpeningFrom),
                    center = lnd.OpeningFrom.posCenter
                };
                // Make it happen!
                col.transform.localPosition = rect.center;
				col.size = rect.size;
			}
		}
	}
    private Vector2 GetInvisiboundSize(LevelOpening lo) {
        float thickness = 1f;
        if (lo.side==Sides.L || lo.side==Sides.R) { return new Vector2(thickness, lo.length); }
        return new Vector2(lo.length, thickness);
    }
    private void AddHardcodedLevelElements() {
        // CANDO: If this function starts getting big, make new Prop, Decor. Has prefabName, pos, rotation, scale. :)
        if (LevelKey == "IntroPlunge") {
            AddDecor("InstructsPlunge", new Vector2(0, 11));
        }
    }
    private void AddDecor(string prefabName, Vector2 _pos) {
        GameObject go = Instantiate(ResourcesHandler.Instance.GetDecor(prefabName));
        if (go == null) { Debug.LogError("Can't find Decor prefab: " + prefabName); return; }
        GameUtils.ParentAndReset(go, this.transform);
        go.transform.localPosition = _pos;
    }
    

    
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	Debug
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void OnDrawGizmos() {
        Vector2 posGlobal = LevelDataRef.posGlobal;
        foreach (LevelNeighborData lnd in LevelDataRef.Neighbors) {
            Gizmos.color = lnd.LevelTo!=null ? new Color(0.5f,1,0) : new Color(1,0.5f,0);
            Gizmos.DrawLine(posGlobal+lnd.OpeningFrom.posStart, posGlobal+lnd.OpeningFrom.posEnd);
        }
    }



    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	Editing
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void FlipHorz() {
		Prop[] allProps = FindObjectsOfType<Prop>();
		foreach (Prop prop in allProps) {
			prop.FlipHorz();
		}
	}



}




        /*
        Battery[] batteries = FindObjectsOfType<Battery>();
        foreach (Battery obj in batteries) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        CameraBounds cameraBounds = FindObjectOfType<CameraBounds>();
        if (cameraBounds != null) { ld.cameraBoundsData = cameraBounds.SerializeAsData(); }

        Crate[] crates = FindObjectsOfType<Crate>();
        foreach (Crate obj in crates) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        DamageableGround[] damageableGrounds = FindObjectsOfType<DamageableGround>();
        foreach (DamageableGround obj in damageableGrounds) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        Gate[] gates = FindObjectsOfType<Gate>();
        foreach (Gate obj in gates) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        GateButton[] gateButtons = FindObjectsOfType<GateButton>();
        foreach (GateButton obj in gateButtons) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        Gem[] _gems = FindObjectsOfType<Gem>();
        foreach (Gem obj in _gems) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        Ground[] grounds = FindObjectsOfType<Ground>();
        foreach (Ground obj in grounds) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        LevelDoor[] levelDoors = FindObjectsOfType<LevelDoor>();
        foreach (LevelDoor obj in levelDoors) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        Lift[] lifts = FindObjectsOfType<Lift>();
        foreach (Lift obj in lifts) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        Platform[] platforms = FindObjectsOfType<Platform>();
        foreach (Platform obj in platforms) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        PlayerStart[] playerStarts = FindObjectsOfType<PlayerStart>();
        foreach (PlayerStart obj in playerStarts) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        Snack[] _snacks = FindObjectsOfType<Snack>();
        foreach (Snack obj in _snacks) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        Spikes[] spikes = FindObjectsOfType<Spikes>();
        foreach (Spikes obj in spikes) { ld.allPropDatas.Add(obj.SerializeAsData()); }

        ToggleGround[] toggleGrounds = FindObjectsOfType<ToggleGround>();
        foreach (ToggleGround obj in toggleGrounds) { ld.allPropDatas.Add(obj.SerializeAsData()); }
        */





