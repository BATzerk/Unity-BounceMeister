using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezaBody : PlayerBody {
    // Components
    [SerializeField] private SpriteRenderer sr_highlight;
    
    
    public void OnSetIsRoomFrozen(bool IsFrozen) {
        sr_highlight.enabled = IsFrozen;
    }
    
    
}
