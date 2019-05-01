using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** How to use this class: A PlatformCharacter inits me in its Start, and calls my DependentFixedUpdate at the TOP of its FixedUpdate. That's it! :)
  * I'm used to read our previous velocity a few frames into the past. */
public class PlatformCharacterPVels {
    // Constants
    private const int NumFramesRecorded = 100; // Note: We track currVelIndex, so this size is cheap. 10 vs. 1,000,000, similar processing cost.
    // Properties
    private int currVelIndex;
    private Vector2[] pVels;
    // References
    private PlatformCharacter myPlatformCharacter;
    
    // Getters
    /// param name="framesAgo": 1 return PREVIOUS vel. 2 returns vel TWO FRAMES ago, etc. Note: 0 *would* return current vel, but that doesn't make sense to use.
    public Vector2 PVel(int framesAgo) {
        if (framesAgo >= NumFramesRecorded) { // Safety check.
            Debug.LogWarning("Hey! We're asking for pvels TOO FAR into the past! framesAgo: " + framesAgo + ", NumFramesRecorded: " + NumFramesRecorded);
            return Vector2.zero;
        }
        if (framesAgo == 0) {
            Debug.LogWarning("We're asking PlatformCharacterPVels for vel 0 frames in the past (this doesn't make sense). Why?");
            return myPlatformCharacter.vel;
        }
        int index = currVelIndex - framesAgo;
        if (index < 0) { index += NumFramesRecorded; } // loop back around.
        return pVels[index];
    }
    

    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public PlatformCharacterPVels(PlatformCharacter myPlatformCharacter) {
        this.myPlatformCharacter = myPlatformCharacter;
    
        currVelIndex = 0;
        pVels = new Vector2[NumFramesRecorded];
        for (int i=0; i<pVels.Length; i++) {
            pVels[i] = Vector2.zero;
        }
    }
    

    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    public void DependentFixedUpdate() {
        pVels[currVelIndex] = myPlatformCharacter.vel;
        currVelIndex ++;
        if (currVelIndex >= NumFramesRecorded) { currVelIndex = 0; } // Loop back currVelIndex.
    }
    
    
}
