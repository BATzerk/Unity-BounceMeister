using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour {
    // Components
    [SerializeField] private RoomGizmos roomGizmos = null;
    [SerializeField] private RoomShutters shutters=null;
    // Properties
    private GateChannel[] gateChannels;
    private List<CharBarrel> charBarrels = new List<CharBarrel>();
    private List<Gem> gems = new List<Gem>();
    private List<Snack> snacks = new List<Snack>();
    // References
    private GameController gameControllerRef;
    public RoomData MyRoomData { get; private set; }

    // Getters (Public)
    public WorldData MyWorldData { get { return MyRoomData.MyWorldData; } }
    public int WorldIndex { get { return MyRoomData.WorldIndex; } }
    public string RoomKey { get { return MyRoomData.RoomKey; } }
    public RoomClusterData MyClusterData { get { return MyRoomData.MyCluster; } }
    public Vector2 PosGlobal { get { return MyRoomData.PosGlobal; } }
    public GameController GameController { get { return gameControllerRef; } }
    public Player Player { get { return gameControllerRef.Player; } }
    public GateChannel[] GateChannels { get { return gateChannels; } }
    public Rect GetCameraBoundsLocal() {
        CameraBounds cameraBounds = GetComponentInChildren<CameraBounds>();
        if (cameraBounds != null) {
            return new Rect(cameraBounds.RectLocal);
        }
        return new Rect(0, 0, 20, 20);
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
    public RoomData ToData() {
        RoomData rd = new RoomData(MyWorldData, RoomKey);

        // -- General Properties --
        rd.SetPosGlobal(PosGlobal);
        rd.SetDesignerFlag(MyRoomData.DesignerFlag);
        rd.SetIsSecret(MyRoomData.IsSecret);
        rd.isClustStart = MyRoomData.isClustStart;
        //rd.hasPlayerBeenHere = hasPlayerBeenHere;

        // -- Props --
        Prop[] allProps = FindObjectsOfType<Prop>();
        foreach (Prop prop in allProps) {
            if (!prop.DoSaveInRoomFile()) { continue; } // This type of Prop doesn't save to Room file? Skip it.
            rd.AddPropData(prop.ToData());
        }
        // Reverse the propDatas list so it's saved in the same order each time. (Kinda weird, but this is the easy solution.)
        rd.allPropDatas.Reverse();

        // HACKy-ish: Find CameraBounds manually.
        CameraBounds cameraBounds = FindObjectOfType<CameraBounds>();
        if (cameraBounds != null) { rd.cameraBoundsData = cameraBounds.ToData() as CameraBoundsData; }

        return rd;
    }


    // ----------------------------------------------------------------
    //	Initialize
    // ----------------------------------------------------------------
    public void Initialize(GameController _gameControllerRef, Transform tf_world, RoomData _roomData) {
        gameControllerRef = _gameControllerRef;
        MyRoomData = _roomData;
        this.gameObject.name = MyRoomData.RoomKey;

        GameUtils.ParentAndReset(this.gameObject, tf_world);
        this.transform.localPosition = PosGlobal; // Position me!

        // Initialize channels!
        gateChannels = new GateChannel[5];
        for (int i = 0; i < gateChannels.Length; i++) { gateChannels[i] = new GateChannel(this, i); }

        // Instantiate my props!
        RoomData rd = MyRoomData;
        ResourcesHandler rh = ResourcesHandler.Instance;
        int numProgressGates = 0; // for deteriming their indexes.
        int numVeils = 0; // for determining their indexes.

        foreach (PropData propData in rd.allPropDatas) {
            System.Type pt = propData.GetType();
            // Grounds
            if (pt == typeof(CrateData)) {
                Crate newProp = Instantiate(rh.Crate).GetComponent<Crate>();
                newProp.Initialize(this, propData as CrateData);
            }
            else if (pt == typeof(DispGroundData)) {
                DispGround newProp = Instantiate(rh.DispGround).GetComponent<DispGround>();
                newProp.Initialize(this, propData as DispGroundData);
            }
            else if (pt == typeof(EnemyData)) {
                Enemy newProp = Instantiate(rh.Enemy).GetComponent<Enemy>();
                newProp.Initialize(this, propData as EnemyData);
            }
            else if (pt == typeof(GateData)) {
                Gate newProp = Instantiate(rh.Gate).GetComponent<Gate>();
                newProp.Initialize(this, propData as GateData);
                gateChannels[newProp.ChannelID].AddGate(newProp);
            }
            else if (pt == typeof(ToggleGroundData)) {
                ToggleGround newProp = Instantiate(rh.ToggleGround).GetComponent<ToggleGround>();
                newProp.Initialize(this, propData as ToggleGroundData);
            }
            else if (pt == typeof(PlatformData)) {
                Platform newProp = Instantiate(rh.Platform).GetComponent<Platform>();
                newProp.Initialize(this, propData as PlatformData);
            }
            else if (pt == typeof(GroundData)) {
                Ground newProp = Instantiate(rh.Ground).GetComponent<Ground>();
                newProp.Initialize(this, propData as GroundData);
            }
            // Everything else!
            else if (pt == typeof(BatteryData)) {
                Battery newProp = Instantiate(rh.Battery).GetComponent<Battery>();
                newProp.Initialize(this, propData as BatteryData);
            }
            else if (pt == typeof(BuzzsawData)) {
                Buzzsaw newProp = Instantiate(rh.Buzzsaw).GetComponent<Buzzsaw>();
                newProp.Initialize(this, propData as BuzzsawData);
            }
            else if (pt == typeof(CharBarrelData)) {
                CharBarrel newProp = Instantiate(rh.CharBarrel).GetComponent<CharBarrel>();
                newProp.Initialize(this, propData as CharBarrelData, charBarrels.Count);
                charBarrels.Add(newProp);
            }
            else if (pt == typeof(CharUnlockOrbData)) {
                CharUnlockOrb newProp = Instantiate(rh.CharUnlockOrb).GetComponent<CharUnlockOrb>();
                newProp.Initialize(this, propData as CharUnlockOrbData);
            }
            else if (pt == typeof(CameraBoundsData)) {
                CameraBounds newProp = Instantiate(rh.CameraBounds).GetComponent<CameraBounds>();
                newProp.Initialize(this, propData as CameraBoundsData);
            }
            else if (pt == typeof(GateButtonData)) {
                GateButton newProp = Instantiate(rh.GateButton).GetComponent<GateButton>();
                newProp.Initialize(this, propData as GateButtonData);
                gateChannels[newProp.ChannelID].AddButton(newProp);
            }
            else if (pt == typeof(GemData)) {
                Gem newProp = Instantiate(rh.Gem).GetComponent<Gem>();
                newProp.Initialize(this, propData as GemData, gems.Count);
                gems.Add(newProp);
            }
            else if (pt == typeof(InfoSignData)) {
                InfoSign newProp = Instantiate(rh.InfoSign).GetComponent<InfoSign>();
                newProp.Initialize(this, propData as InfoSignData);
            }
            else if (pt == typeof(LaserData)) {
                Laser newProp = Instantiate(rh.Laser).GetComponent<Laser>();
                newProp.Initialize(this, propData as LaserData);
            }
            else if (pt == typeof(LiftData)) {
                Lift newProp = Instantiate(rh.Lift).GetComponent<Lift>();
                newProp.Initialize(this, propData as LiftData);
            }
            else if (pt == typeof(PlayerStartData)) {
                PlayerStart newProp = Instantiate(rh.PlayerStart).GetComponent<PlayerStart>();
                newProp.Initialize(this, propData as PlayerStartData);
            }
            else if (pt == typeof(ProgressGateData)) {
                ProgressGate newProp = Instantiate(rh.ProgressGate).GetComponent<ProgressGate>();
                newProp.Initialize(this, propData as ProgressGateData, numProgressGates++);
            }
            else if (pt == typeof(RoomDoorData)) {
                RoomDoor newProp = Instantiate(rh.RoomDoor).GetComponent<RoomDoor>();
                newProp.Initialize(this, propData as RoomDoorData);
            }
            else if (pt == typeof(SnackData)) {
                Snack newProp = Instantiate(rh.Snack).GetComponent<Snack>();
                newProp.Initialize(this, propData as SnackData, snacks.Count);
                snacks.Add(newProp);
            }
            else if (pt == typeof(SpikesData)) {
                Spikes newProp = Instantiate(rh.Spikes).GetComponent<Spikes>();
                newProp.Initialize(this, propData as SpikesData);
            }
            else if (pt == typeof(TurretData)) {
                Turret newProp = Instantiate(rh.Turret).GetComponent<Turret>();
                newProp.Initialize(this, propData as TurretData);
            }
            else if (pt == typeof(VeilData)) {
                Veil newProp = Instantiate(rh.Veil).GetComponent<Veil>();
                newProp.Initialize(this, propData as VeilData, numVeils++);
            }
            else {
                Debug.LogWarning("PropData not recognized: " + propData);
            }
        }

        AddHardcodedRoomElements();

        // For development, add bounds so we don't fall out of unconnected rooms!
        AutoAddSilentBoundaries();
        roomGizmos.Initialize(this);
        shutters.Initialize(this);
    }

    /** Slightly sloppy, whatever-it-takes housekeeping to allow us to start up the game with a novel room and edit/play/save it right off the bat. */
    public void InitializeAsPremadeRoom(GameController _gameControllerRef) {
        gameControllerRef = _gameControllerRef;

        // TEMP totes hacky, yo.
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        MyRoomData = new RoomData(dataManager.CurrWorldData, sceneName); // NOTE! Be careful; we can easily overwrite rooms like this.
        MyRoomData = ToData();

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
            cameraBounds.Initialize(this, cameraBounds.ToData() as CameraBoundsData); // Strange and hacky: It initializes itself as what it already is. Just to go through other paperwork.
        }
    }
    private void AutoAddSilentBoundaries() {
        for (int i = 0; i < MyRoomData.Openings.Count; i++) {
            // No room here?? Protect me with an InvisiBounds!
            RoomOpening rod = MyRoomData.Openings[i];
            if (!rod.IsRoomTo) {
                BoxCollider2D col = new GameObject().AddComponent<BoxCollider2D>();
                col.transform.SetParent(this.transform);
                col.transform.localScale = Vector3.one;
                col.transform.localEulerAngles = Vector3.zero;
                col.gameObject.layer = LayerMask.NameToLayer("Ground"); // so our feet stop on it, yanno.
                col.name = "Invisibounds" + rod.side;
                // Determine the collider's rect, ok? Promise?
                float thickness = 10f;
                Rect rect = new Rect {
                    size = GetInvisiboundSize(rod, thickness),
                    center = rod.posCenter + MathUtils.GetDir(rod.side)*thickness*0.5f,
                };
                // Make it happen!
                col.transform.localPosition = rect.center;
                col.size = rect.size;
            }
        }
    }
    private Vector2 GetInvisiboundSize(RoomOpening lo, float thickness) {
        if (lo.side == Sides.L || lo.side == Sides.R) { return new Vector2(thickness, lo.length); }
        return new Vector2(lo.length, thickness);
    }
    private void AddHardcodedRoomElements() {
        //// CANDO: If this function starts getting big, make new Prop, Decor. Has prefabName, pos, rotation, scale. :)
        if (RoomKey == "IntroPlunge") {
            AddDecor("PlungeImplicationGhost", new Vector2(4.5f, -3.7f), new Vector2(8,8));
        }
        //else if (RoomKey == "IntroHover") {
        //    AddDecor("InstructsHover", new Vector2(0, 11));
        //}
    }
    private void AddDecor(string prefabName, Vector2 _pos, Vector2 _scale) {
        GameObject go = Instantiate(ResourcesHandler.Instance.GetDecor(prefabName));
        if (go == null) { Debug.LogError("Can't find Decor prefab: " + prefabName); return; }
        GameUtils.ParentAndReset(go, this.transform);
        go.transform.localPosition = _pos;
        go.transform.localScale = _scale;
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    public void OnPlayerExitMe() {
        // Tell all GateChannels!
        for (int i=0; i<gateChannels.Length; i++) { gateChannels[i].OnPlayerExitMyRoom(); }
    }




    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	Debug
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //private void OnDrawGizmos() {
        //Vector2 posGlobal = MyRoomData.posGlobal;
        //foreach (RoomOpening ro in MyRoomData.Openings) {
        //    Gizmos.color = ro.RoomTo!=null ? new Color(0.5f,1,0) : new Color(1,0.5f,0);
        //    Gizmos.DrawLine(posGlobal+ro.posStart, posGlobal+ro.posEnd);
        //}
    //}



    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //	Editing
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void FlipHorz() {
        foreach (Prop prop in FindObjectsOfType<Prop>()) { prop.FlipHorz(); }
    }
    public void FlipVert() {
        foreach (Prop prop in FindObjectsOfType<Prop>()) { prop.FlipVert(); }
    }
    public void MoveAllProps(Vector2Int dir) {
        MoveAllProps(dir * GameProperties.UnitSize);
    }
    public void MoveAllProps(Vector2 delta) {
        Prop[] allProps = FindObjectsOfType<Prop>();
        foreach (Prop prop in allProps) {
            if (prop is CameraBounds) { continue; } // Note: Don't move CameraBounds.
            prop.Move(delta);
        }
    }



}




/*
private void AutoAddSilentBoundaries() {
    Rect camBounds = roomDataRef.cameraBoundsData.myRect;
    for (int side=0; side<Sides.NumSides; side++) {
        // No room at this side?? Protect me with an InvisiBounds!
        if (WorldDataRef.Debug_GetSomeRoomAtSide(roomDataRef, side) == null) {
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

