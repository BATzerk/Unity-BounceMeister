using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Prop : MonoBehaviour, ITravelable {
    // Overrideables
    virtual public bool DoSaveInRoomFile() { return true; } // by default, ALL Props wanna get saved into the Room text file. But some (e.g. Player) do NOT.
    // Properties
    protected bool IsInitialized { get; private set; } // Used to set default values when drag prefab out in Editor.
    protected int FrameCountWhenBorn { get; private set; }
    // References
    public Room MyRoom { get; private set; }

	// Getters
    protected string RoomKey { get { return MyRoom.RoomKey; } }
    protected int WorldIndex { get { return MyRoom.WorldIndex; } }
	public Vector2 PosLocal { get { return pos; } }
	public Vector2 PosGlobal {
		get {
			if (MyRoom==null) { return PosLocal; } // Safety check.
			return PosLocal + MyRoom.PosGlobal;
		}
	}
	protected Vector2 pos {
		get { return this.transform.localPosition; }
		set { this.transform.localPosition = value; }
	}
	public float rotation {
		get { return this.transform.localEulerAngles.z; }
		protected set { this.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, value); }
	}
    public Vector2 vel { get; private set; }
    
    public void SetVel(float _x,float _y) { SetVel(new Vector2(_x,_y)); }
    public void SetVel(Vector2 _vel) { vel = _vel; }
    
    // Travelable Stuff
    protected PropTravelMind travelMind { get; private set; } // added in Initialize.
    public TravelMindData travelMindData { get { return new TravelMindData(travelMind); } }
    public bool HasTravelMind() { return travelMind != null; }
    public void AddTravelMind(TravelMindData data) {
        if (travelMind != null) { return; } // Safety check.
        travelMind = gameObject.AddComponent<PropTravelMind>();
        travelMind.Initialize(this, data);
        DisableSnappingScript();
    }
    public void RemoveTravelMind() {
        if (travelMind == null) { return; } // Safety check.
        Destroy(travelMind);
        travelMind = null;
        EnableSnappingScript();
    }
    public void ToggleHasTravelMind() {
        if (HasTravelMind()) { RemoveTravelMind(); }
        else { AddTravelMind(TravelMindData.Default); }
    }

    public Vector2 GetPos() { return pos; }
    public void SetPos(Vector2 _pos) { pos = _pos; }
    virtual protected void OnCreatedInEditor() {
        if (travelMind == null) { travelMind = GetComponent<PropTravelMind>(); } // Safety check for duplicating objects.
    }
    
    private void EnableSnappingScript() {
        BaseGridSnap script = GetComponent<BaseGridSnap>();
        if (script != null) { script.enabled = true; }
    }
    private void DisableSnappingScript() {
        BaseGridSnap script = GetComponent<BaseGridSnap>();
        if (script != null) { script.enabled = false; }
    }
    


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
	virtual protected void BaseInitialize(Room myRoom, PropData data) {
		this.MyRoom = myRoom;
        GameUtils.ParentAndReset(this.gameObject, myRoom.transform);

		this.transform.localPosition = data.pos; // note that this is just a convenience default. Any grounds will set their pos from their rect.
		rotation = data.rotation;
        if (data.travelMind.IsUsed) { AddTravelMind(data.travelMind); }
        
        IsInitialized = true;
        FrameCountWhenBorn = Time.frameCount;
	}
    virtual protected void Start() {
        #if UNITY_EDITOR
        // No Room ref?? We've been pulled out from the Editor!
        if (MyRoom == null) {
            // Set my Room ref!
            MyRoom = GetComponentInParent<Room>();
            if (MyRoom == null) { MyRoom = FindObjectOfType<Room>(); } // Also check the whole scene, just in case.
            this.transform.SetParent(MyRoom.transform);
            //if (myRoom != null) { // Safety check.
            //    PropData data = ToData();
            //    BaseInitialize(myRoom, data);
            //}
            // Unpack me as a Prefab!
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this.gameObject)) {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(this.gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
            }
            OnCreatedInEditor();
        }
        #endif
    }


    // ----------------------------------------------------------------
    //  Debug
    // ----------------------------------------------------------------
    public void Debug_RotateCW() { Debug_Rotate(-90); }
    private void Debug_Rotate(float delta) {
        rotation = Mathf.Round(rotation + delta);
    }
    public void Debug_SetRotation(float _rot) {
        rotation = Mathf.Round(_rot);
    }
    virtual public void FlipHorz() {
        pos = new Vector2(-pos.x, pos.y);
        rotation = -rotation;
        if (travelMind != null) { travelMind.FlipHorz(); }
    }
    virtual public void FlipVert() {
        pos = new Vector2(pos.x, -pos.y);
        if (travelMind != null) { travelMind.FlipVert(); }
    }
    virtual public void Move(Vector2 delta) {
        pos += delta;
        if (travelMind != null) { travelMind.Move(delta); }
    }

    
    
    // ----------------------------------------------------------------
    //  Serialize
    // ----------------------------------------------------------------
    abstract public PropData ToData();


}
