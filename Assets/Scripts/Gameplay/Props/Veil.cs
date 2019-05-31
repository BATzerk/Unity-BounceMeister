using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Veil : Prop {
    // Components
    [SerializeField] public BoxCollider2D MyCollider=null;
    [SerializeField] private SpriteRenderer sr_body=null;
    // Properties
    private bool IsUnveiled;
    private int myIndex; // for saving.
    
    // Getters
    private Rect MyRect {
        get {
            return new Rect(sr_body.transform.localPosition, sr_body.size);
        }
    }


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, VeilData data, int myIndex) {
        base.BaseInitialize(_myRoom, data);
        this.myIndex = myIndex;
        
        // Size/pos me, broh!
        sr_body.size = data.myRect.size;
        sr_body.transform.localPosition = data.myRect.position;
        
        // Color me right-o.
        sr_body.color = Colors.GroundBaseColor(WorldIndex);

        // Start opaque.
        bool isUnveiled = SaveStorage.GetBool(SaveKeys.IsVeilUnveiled(MyRoom.MyRoomData, myIndex), false);
        SetIsUnveiled(isUnveiled, false);
    }


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D col) {
        Player player = col.gameObject.GetComponent<Player>();
        if (player != null) {
            OnPlayerEnterMe();
        }
    }
    private void OnPlayerEnterMe() {
        // Save that I've been unveiled!
        SaveStorage.SetBool(SaveKeys.IsVeilUnveiled(MyRoom.MyRoomData, myIndex), true);
        SetIsUnveiled(true); // the jig is up! Make me transparent now!
    }



    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetIsUnveiled(bool _isUnveiled, bool doAnimate=true) {
        // Update collider.
        if (MyCollider!=null) { MyCollider.enabled = !_isUnveiled; }
        // Update body/stroke alphas.
        float bodyAlpha = _isUnveiled ? 0.3f : 1;
        float dur = doAnimate ? 0.7f : 0;
        LeanTween.cancel(sr_body.gameObject);
        LeanTween.alpha(sr_body.gameObject, bodyAlpha, dur).setEaseOutQuart();
    }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData ToData() {
        VeilData data = new VeilData {
            myRect = MyRect,
            travelMind = new TravelMindData(travelMind),
        };
        return data;
    }



}
