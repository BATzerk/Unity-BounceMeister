using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : BaseGround {
    // Constants
    [SerializeField] private Color c_canDropThru=Color.white;
    [SerializeField] private Color c_cannotDropThru=Color.white;
    // Properties
    [SerializeField] bool canDropThru = true;
    // References
    [SerializeField] Sprite s_canDropThru=null;
    [SerializeField] Sprite s_cannotDropThru=null;
    
    // Getters
    public bool CanDropThru { get { return canDropThru; } }



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Room _myRoom, PlatformData data) {
		base.BaseGroundInitialize(_myRoom, data);
        canDropThru = data.canDropThru;
        
        // Color me rights.
        bodySprite.color = canDropThru ? c_canDropThru : c_cannotDropThru;
        bodySprite.sprite = canDropThru ? s_canDropThru : s_cannotDropThru;
	}

	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
    override public PropData SerializeAsData() {
        PlatformData data = new PlatformData {
            myRect = MyRect(),
            mayPlayerEat = MayPlayerEatHere,
            isPlayerRespawn = IsPlayerRespawn,
            canDropThru = canDropThru,
        };
        return data;
	}
}
