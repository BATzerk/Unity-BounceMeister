using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buzzsaw : Collidable, ITravelable {
    // Components
    [SerializeField] private CircleCollider2D circleCollider=null;
    [SerializeField] private SpriteRenderer bodySprite=null;

    // Getters
    private Vector2 size { get { return bodySprite.size; } }


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
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData ToData() {
        return new BuzzsawData {
            pos = pos,
            size = size,
            travelMind = new TravelMindData(travelMind),
        };
    }
}
