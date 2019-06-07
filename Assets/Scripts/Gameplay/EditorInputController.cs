using UnityEngine;
using System.Collections.Generic;
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
                // SHIFT + [ARROW KEYS] = Move Props selected! TODO: Fix this. It don't work.
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