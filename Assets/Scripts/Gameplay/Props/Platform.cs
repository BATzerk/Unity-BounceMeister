using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : BaseGround, ISerializableData<PlatformData> {
    // Constants
    [SerializeField] private Color c_canDropThru=Color.white;
    [SerializeField] private Color c_cannotDropThru=Color.white;
    // Properties
    [SerializeField] bool canDropThru = true;
    
    // Getters
    public bool CanDropThru { get { return canDropThru; } }



	// ----------------------------------------------------------------
	//  Initialize
	// ----------------------------------------------------------------
	public void Initialize(Level _myLevel, PlatformData data) {
		base.BaseGroundInitialize(_myLevel, data);
        canDropThru = data.canDropThru;
        
        // Color me rights.
        bodySprite.color = canDropThru ? c_canDropThru : c_cannotDropThru;
	}

	// ----------------------------------------------------------------
	//  Serializing
	// ----------------------------------------------------------------
	public PlatformData SerializeAsData() {
        PlatformData data = new PlatformData {
            myRect = MyRect(),
            canEatGems = CanEatEdibles,
            isPlayerRespawn = IsPlayerRespawn,
            canDropThru = canDropThru,
        };
        return data;
	}
}
