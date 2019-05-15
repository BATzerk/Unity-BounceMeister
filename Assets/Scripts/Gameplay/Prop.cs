using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Prop : MonoBehaviour {
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

	virtual protected void BaseInitialize(Room myRoom, PropData data) {
		this.MyRoom = myRoom;
        GameUtils.ParentAndReset(this.gameObject, myRoom.transform);

		this.transform.localPosition = data.pos; // note that this is just a convenience default. Any grounds will set their pos from their rect.
		rotation = data.rotation;
        
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
            //GameUtils.ParentAndReset(this.gameObject, myRoom.transform);
            this.transform.SetParent(MyRoom.transform);
            //if (myRoom != null) { // Safety check.
            //    PropData data = SerializeAsData();
            //    BaseInitialize(myRoom, data);
            //}
        }
        #endif
    }
    
    
    
    

	virtual public void FlipHorz() {
		pos = new Vector2(-pos.x, pos.y);
		rotation = -rotation;
	}
    virtual public void Move(Vector2 delta) {
        pos += delta;
    }
    
    abstract public PropData SerializeAsData();

}
