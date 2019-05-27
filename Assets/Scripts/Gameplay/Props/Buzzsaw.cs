using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(Buzzsaw))]
public class BuzzsawEditor : UnityEditor.Editor {
    // References
    private Buzzsaw myProp;
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (myProp == null) { myProp = (Buzzsaw)target; }
        if (!myProp.HasTravelMind()) {
            if (GUILayout.Button("Add TravelMind")) {
                myProp.AddTravelMind(new TravelMindData(new Vector2(-5,0), new Vector2(5,0), 2, 0));
            }
        }
        else {
            if (GUILayout.Button("Remove TravelMind")) {
                myProp.RemoveTravelMind();
            }
        }
    }
}


public class Buzzsaw : Collidable, ITravelable {
    // Components
    [SerializeField] private CircleCollider2D circleCollider=null;
    [SerializeField] private SpriteRenderer bodySprite=null;

    // Getters
    private Vector2 size { get { return bodySprite.size; } }
    
    // Travelable Stuff
    private PropTravelMind travelMind; // added in Initialize.
    public TravelMindData travelMindData { get { return new TravelMindData(travelMind); } }
    public bool HasTravelMind() { return travelMind != null; }
    public void AddTravelMind(TravelMindData data) {
        if (travelMind != null) { return; } // Safety check.
        travelMind = gameObject.AddComponent<PropTravelMind>();
        travelMind.Initialize(this, data);
    }
    public void RemoveTravelMind() {
        if (travelMind == null) { return; } // Safety check.
        Destroy(travelMind);
        travelMind = null;
    }
    // TODO: Clean this pos-stuff up! Would we want TravelMind on ANY Prop? Make changes accordingly.
    public Vector2 GetPos() { return pos; }
    public void SetPos(Vector2 _pos) { pos = _pos; }
    override protected void OnCreatedInEditor() {
        if (travelMind == null) { travelMind = GetComponent<PropTravelMind>(); } // Safety check for duplicating objects.
    }


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, BuzzsawData data) {
        base.BaseInitialize(_myRoom, data);
        
        if (data.travelMind.IsUsed) { AddTravelMind(data.travelMind); }

        bodySprite.size = data.size;
        circleCollider.radius = (data.size.x/2) * 0.75f; // Note: Shrink collider; be a lil' generous.
        //bodySprite.transform.localPosition = data.myRect.position;
        //bodySprite.color = Colors.Spikes(WorldIndex);
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void Update() {
        UpdateBodyRotation();
    }
    private void UpdateBodyRotation() {
        bodySprite.transform.Rotate(new Vector3(0, 0, -Time.deltaTime*2000));
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    override public void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
        Player player = character as Player;
        if (player != null) {
            player.OnTouchHarm();
        }
    }



    // ----------------------------------------------------------------
    //  Editing
    // ----------------------------------------------------------------
    //public override void FlipHorz() {TODO: Deez.
    //    base.FlipHorz();
    //    posA *= Vector2.left;
    //    posB *= Vector2.left;
    //}
    //public override void FlipVert() {
    //    base.FlipVert();
    //    posA *= Vector2.down;
    //    posB *= Vector2.down;
    //}
    //override public void Move(Vector2 delta) {
    //    base.Move(delta);
    //    posA += delta;
    //    posB += delta;
    //}


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        if (travelMind == null) { travelMind = GetComponent<PropTravelMind>(); } // Make sure we got the reference!
        return new BuzzsawData {
            pos = pos,
            size = size,
            travelMind = new TravelMindData(travelMind),
        };
    }
}

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buzzsaw : Collidable {
    // Components
    [SerializeField] private SpriteRenderer bodySprite=null;
    [SerializeField] private Transform tf_a=null;
    [SerializeField] private Transform tf_b=null;
    [SerializeField] private Transform tf_posContainer=null;
    // Properties
    [SerializeField] private float locOffset=0;
    [SerializeField] private float speed=1;
    private float oscLoc;

    // Getters / Setters
    private Vector2 posA {
        get { return tf_a.localPosition; }
        set { tf_a.localPosition = value; }
    }
    private Vector2 posB {
        get { return tf_b.localPosition; }
        set { tf_b.localPosition = value; }
    }

    // Getters
    private Vector2 size { get { return bodySprite.size; } }



    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Room _myRoom, BuzzsawData data) {
        base.BaseInitialize(_myRoom, data);

        bodySprite.size = data.size;
        //bodySprite.transform.localPosition = data.myRect.position;
        //bodySprite.color = Colors.Spikes(WorldIndex);
        
        locOffset = data.locOffset;
        speed = data.speed;
        posA = data.posA;
        posB = data.posB;
        oscLoc = locOffset; // start with my desired offset!
        
        UpdatePos();
    }


    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        Vector2 prevPos = pos;

        UpdatePos();

        vel = pos - prevPos;
    }
    private void UpdatePos() {
        oscLoc += Time.deltaTime * speed;
        float loc = MathUtils.Sin01(oscLoc);
        pos = Vector2.Lerp(tf_a.localPosition,tf_b.localPosition, loc);
        
        // Offset posContainer, so tf_posA and tf_posB are in the right relative spot.
        tf_posContainer.localPosition = -pos;
    }
    
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    override public void OnTriggerEnter2D(Collider2D col) {
        Player player = col.gameObject.GetComponent<Player>();
        if (player != null) {
            player.OnTouchHarm();
        }
    }

    override public void OnCharacterTouchMe(int charSide, PlatformCharacter character) {
        Player player = character as Player;
        if (player != null) {
            player.OnTouchHarm();
        }
    }


    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        return new BuzzsawData {
            size = size,
            locOffset = locOffset,
            speed = speed,
            posA = posA,
            posB = posB,
        };
    }
}
*/