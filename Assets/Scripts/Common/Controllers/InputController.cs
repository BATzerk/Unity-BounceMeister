using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;


#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class EditorInputController {
    // Properties
    private static Prop propSel;
    
    // Getters
    private static List<Prop> GetPropsSelected() {
        List<Prop> list = new List<Prop>();
        foreach (GameObject go in Selection.gameObjects) {
            Prop prop = go.GetComponent<Prop>();
            if (prop != null) { list.Add(prop); }
        }
        return list;
    }



    static EditorInputController() {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }


    private static void OnSceneGUI(SceneView sceneView) {
        // Do your general-purpose scene gui stuff here...
        // Applies to all scene views regardless of selection!
 
        // You'll need a control id to avoid messing with other tools!
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        InputController.IsEditorKey_Control = Event.current.modifiers == EventModifiers.Control;
        InputController.IsEditorKey_Shift = Event.current.modifiers == EventModifiers.Shift;

        propSel = Selection.activeGameObject==null ? null : Selection.activeGameObject.GetComponent<Prop>();
        
        if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown) {
            // CONTROL + ___....
            if (InputController.IsEditorKey_Control) {
                // CONTROL + A = Set PropSelected TravelMind PosA!
                if (Event.current.keyCode == KeyCode.A) {
                    PropSelTravelMindSetPosA();
                    Event.current.Use(); // Use the event here.
                }
                // CONTROL + B = Set PropSelected TravelMind PosB!
                if (Event.current.keyCode == KeyCode.B) {
                    PropSelTravelMindSetPosB();
                    Event.current.Use(); // Use the event here.
                }
                // CONTROL + R = Selected Prop: Rotate!
                if (Event.current.keyCode == KeyCode.R) {
                    PropSelRotateCW();
                    Event.current.Use(); // Use the event here.
                }
                //// CONTROL + O = Selected Prop: Add/Remove OnOffer!
                //if (Event.current.keyCode == KeyCode.O) {
                //    PropSelToggleOnOffer();
                //    Event.current.Use(); // Use the event here.
                //}
                // CONTROL + T = Selected Prop: Add/Remove TravelMind!
                if (Event.current.keyCode == KeyCode.T) {
                    PropSelToggleTravelMind();
                    Event.current.Use(); // Use the event here.
                }
            }
            // SHIFT + ...
            if (InputController.IsEditorKey_Shift) {
                // SHIFT + X = Flip Horizontal!
                if (Event.current.keyCode == KeyCode.X) {
                    FlipPropsSelHorz();
                    Event.current.Use(); // Use the event here.
                }
                // SHIFT + Y = Flip Vertical!
                else if (Event.current.keyCode == KeyCode.Y) {
                    FlipPropsSelVert();
                    Event.current.Use(); // Use the event here.
                }
                // SHIFT + [ARROW KEYS] = Move Props selected!
                if (Event.current.keyCode == KeyCode.LeftArrow)  { MovePropsSel(Vector2Int.L); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.RightArrow) { MovePropsSel(Vector2Int.R); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.DownArrow)  { MovePropsSel(Vector2Int.B); Event.current.Use(); }
                else if (Event.current.keyCode == KeyCode.UpArrow)    { MovePropsSel(Vector2Int.T); Event.current.Use(); }
            }
        }
    }
    private static void FlipPropsSelHorz() {
        foreach (Prop prop in GetPropsSelected()) { prop.FlipHorz(); }
    }
    private static void FlipPropsSelVert() {
        foreach (Prop prop in GetPropsSelected()) { prop.FlipVert(); }
    }
    public static void MovePropsSel(Vector2Int dir) {
        MovePropsSel(dir * GameProperties.UnitSize);
    }
    private static void MovePropsSel(Vector2 delta) {
        foreach (Prop prop in GetPropsSelected()) { prop.Move(delta); }
    }
    
    private static void PropSelRotateCW() {
        if (propSel == null) { return; } // Safety check.
        propSel.Debug_RotateCW();
    }
    //private static void PropSelToggleOnOffer() {
        //if (propSel == null) { return; } // Safety check.
        //propSel.ToggleHasOnOffer();
    //}
    private static void PropSelToggleTravelMind() {
        if (propSel == null) { return; } // Safety check.
        propSel.ToggleHasTravelMind();
    }
    private static void PropSelTravelMindSetPosA() {
        if (propSel == null || !propSel.HasTravelMind()) { return; } // Safety check.
        propSel.GetComponent<PropTravelMind>().Debug_SetPosA();
    }
    private static void PropSelTravelMindSetPosB() {
        if (propSel == null || !propSel.HasTravelMind()) { return; } // Safety check.
        propSel.GetComponent<PropTravelMind>().Debug_SetPosB();
    }
}
#endif



public class InputController : MonoBehaviour {
	// Constants
	private const float QUICK_CLICK_TIME_WINDOW = 0.3f; // quick-click is used for double-clicks (but could be extended to 3 or more).
	// Instance
	static private InputController instance;
	// Properties
	private int numQuickClicks=0; // for double-clicks (or maybe even more).
	private bool isDoubleClick; // reset every frame.
	private float timeWhenNullifyDoubleClick;
    public bool IsAction_Press { get; private set; }
    public bool IsAction_Release { get; private set; }
    public bool IsCycleChar_Press { get; private set; }
    public bool IsJump_Held { get; private set; }
    public bool IsJump_Press { get; private set; }
    public bool IsJump_Release { get; private set; }
    //public bool IsLPush { get; private set; } // NOTE: Updated every FIXEDUPDATE! NOTE NOTE: *NOT* reliable for others to use. Registers for Player like 97% of the time. :P
    //public bool IsRPush { get; private set; }
    //public bool IsLRelease { get; private set; }
    //public bool IsRRelease { get; private set; }
    public Vector2 LeftStick { get; private set; }
    public Vector2 LeftStickRaw { get; private set; }
    //private Vector2 pLeftStick;
    //public static bool IsButtonDown_Down { get; private set; }
    //public static bool IsButtonDown_Held { get; private set; }
	private Vector2 mousePosDown;
    // References
    private InputDevice ad; // activeDevice. Updated at start of every Update.

    // Getters
    static public InputController Instance {
		get {
//			if (instance==null) { return this; } // Note: This is only here to prevent errors when recompiling code during runtime.
			return instance;
		}
	}
    //public bool IsButtonDown_Held { get { return PlayerInput.y < -0.7f; } }
	public bool IsDoubleClick { get { return isDoubleClick; } }
	public Vector3 MousePosScreen { get { return (Input.mousePosition - new Vector3(Screen.width,Screen.height,0)*0.5f) / ScreenHandler.ScreenScale; } }
    public Vector2 MousePosWorld { get { return Camera.main.ScreenToWorldPoint(Input.mousePosition); } }

    static public int GetMouseButtonDown() {
		if (Input.GetMouseButtonDown(0)) return 0;
		if (Input.GetMouseButtonDown(1)) return 1;
		if (Input.GetMouseButtonDown(2)) return 2;
		return -1;
	}
	static public int GetMouseButtonUp() {
		if (Input.GetMouseButtonUp(0)) return 0;
		if (Input.GetMouseButtonUp(1)) return 1;
		if (Input.GetMouseButtonUp(2)) return 2;
		return -1;
	}
	static public bool IsMouseButtonDown () {
		return GetMouseButtonDown() != -1;
	}
	static public bool IsMouseButtonUp () {
		return GetMouseButtonUp() != -1;
	}

    static public bool IsKey_alt { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }
    static public bool IsKey_control { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); } }
    static public bool IsKeyUp_control { get { return Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl); } }
    static public bool IsKeyDown_control { get { return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl); } }
    static public bool IsKey_shift { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }
    static public bool IsKeyUp_shift { get { return Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift); } }
    static public bool IsKeyDown_shift { get { return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift); } }

    static public bool IsEditorKey_Control; // Assigned by EditorInputController.
    static public bool IsEditorKey_Shift; // Assigned by EditorInputController.




    // ----------------------------------------------------------------
    //  Awake
    // ----------------------------------------------------------------
    private void Awake () {
		// There can only be one (instance)!!
		if (instance != null) {
			Destroy (this.gameObject);
			return;
		}
		instance = this;
	}

	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update () {
		if (instance == null) { instance = this; } // Safety check (for runtime compile).
        ad = InputManager.ActiveDevice;
        
        RegisterMouse();
        RegisterButtons();
        RegisterJoystick();
	}

    private void RegisterMouse() {
        isDoubleClick = false; // I'll say otherwise in a moment.

        if (Input.GetMouseButtonDown(0)) {
            // Up how many clicks we got.
            numQuickClicks ++;
            // This is the SECOND click??
            if (numQuickClicks == 2) { // to-do: Discount if mouse pos is too far from first down pos.
                isDoubleClick = true;
            }
            // Reset the timer to count another quick-click!
            timeWhenNullifyDoubleClick = Time.time + QUICK_CLICK_TIME_WINDOW;
        }
        // Have we nullified our double-click by waiting too long?
        if (Time.time >= timeWhenNullifyDoubleClick) {
            numQuickClicks = 0;
        }
    }
	private void RegisterButtons() {
        IsCycleChar_Press = ad.Action3.WasPressed || Input.GetKeyDown(KeyCode.Tab);
        IsJump_Held = ad.Action1.IsPressed || Input.GetButton("Jump");
        IsJump_Press = ad.Action1.WasPressed || Input.GetButtonDown("Jump");
        IsJump_Release = ad.Action1.WasReleased || Input.GetButtonUp("Jump");
        IsAction_Press =
               //ad.Action3.WasPressed
               ad.LeftTrigger.WasPressed
            || ad.RightTrigger.WasPressed
            || Input.GetButtonDown("Action");
        IsAction_Release =
               //ad.Action3.WasReleased
               ad.LeftTrigger.WasReleased
            || ad.RightTrigger.WasReleased
            || Input.GetButtonUp("Action");
        
        //// Update IsButtonDown_Down/Held.
        //if (IsButtonDown_Held) {
        //    IsButtonDown_Down &= PlayerInput.y <= -0.7f; // NOT pushing down? Make false.
        //}
        //else {
        //    IsButtonDown_Down |= PlayerInput.y < -0.7f; // YES pushing down? Make true!
        //}
    }
    private void RegisterJoystick() {
        LeftStick = new Vector2(ad.LeftStickX, ad.LeftStickY);
        LeftStick += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Mathf.Abs(LeftStick.x) < 0.1f) { LeftStick = new Vector2(0, LeftStick.y); } // TEST! Add dead zone.
        
        LeftStickRaw = new Vector2(ad.LeftStickX, ad.LeftStickY);
        LeftStickRaw += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        // In editor? Disregard input if ALT or CONTROL keys are down!
        #if UNITY_EDITOR
        if (IsKey_alt || IsKey_control) {
            LeftStick = Vector2.zero;
            LeftStickRaw = Vector2.zero;
        }
        #endif
        
        //IsLPush = false;
        //IsRPush = false;
        //IsLRelease = false;
        //IsRRelease = false;
        //if (LeftStick.x < -0.1f && pLeftStick.x >= -0.1f) { IsLPush = true; }
        //if (LeftStick.x >  0.1f && pLeftStick.x <=  0.1f) { IsRPush = true; }
        //if (LeftStick.x >= -0.1f && pLeftStick.x < -0.1f) { IsLRelease = true; }
        //if (LeftStick.x  <  0.1f && pLeftStick.x >= 0.1f) { IsRRelease = true; }
        
        //pLeftStick = LeftStick;
    }
    

}


