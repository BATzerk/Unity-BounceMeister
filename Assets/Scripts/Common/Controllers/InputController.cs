using UnityEngine;
using System.Collections;
using InControl;

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
    public bool IsJump_Press { get; private set; }
    public bool IsJump_Release { get; private set; }
    //public bool IsLPush { get; private set; } // NOTE: Updated every FIXEDUPDATE! NOTE NOTE: *NOT* reliable for others to use. Registers for Player like 97% of the time. :P
    //public bool IsRPush { get; private set; }
    //public bool IsLRelease { get; private set; }
    //public bool IsRRelease { get; private set; }
    public Vector2 LeftStick { get; private set; }
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

    static public bool IsKeyDown_alt { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }
    static public bool IsKeyDown_shift { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }
    static public bool IsKeyDown_control { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); } }




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
        //print(Time.frameCount + " PlayerInput: " + PlayerInput);
        
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


