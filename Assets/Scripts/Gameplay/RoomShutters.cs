using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomShutters : MonoBehaviour {
    // Components
    [SerializeField] private SpriteRenderer sr_l=null;
    [SerializeField] private SpriteRenderer sr_r=null;
    [SerializeField] private SpriteRenderer sr_b=null;
    [SerializeField] private SpriteRenderer sr_t=null;
    
    
    public void Initialize(Room myRoom) {
        const float thickness = 50; // how far away from the screen we go. Should be at least 2 for the camera shake. It's more 'cause some rooms can be smaller than the screen.
        Rect camBounds = myRoom.GetCameraBoundsLocal();
        GameUtils.SizeSpriteRenderer(sr_l, thickness, camBounds.height+thickness*2);
        GameUtils.SizeSpriteRenderer(sr_r, thickness, camBounds.height+thickness*2);
        GameUtils.SizeSpriteRenderer(sr_b, camBounds.width+thickness*2, thickness);
        GameUtils.SizeSpriteRenderer(sr_t, camBounds.width+thickness*2, thickness);
        sr_l.transform.localPosition = new Vector3(camBounds.xMin-thickness*0.5f, camBounds.center.y);
        sr_r.transform.localPosition = new Vector3(camBounds.xMax+thickness*0.5f, camBounds.center.y);
        sr_b.transform.localPosition = new Vector3(camBounds.center.x, camBounds.yMin-thickness*0.5f);
        sr_t.transform.localPosition = new Vector3(camBounds.center.x, camBounds.yMax+thickness*0.5f);
    }
    
    
}
