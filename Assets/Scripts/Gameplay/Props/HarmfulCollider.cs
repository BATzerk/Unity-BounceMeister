using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this to anything that has a Collider2D and should hurt the Player.
/// </summary>
public class HarmfulCollider : MonoBehaviour {


    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnCollisionEnter2D(Collision2D col) {
        Player player = col.gameObject.GetComponent<Player>();
        if (player != null) {
            player.OnTouchHarm();
        }
    }
    private void OnTriggerEnter2D(Collider2D col) {
        Player player = col.gameObject.GetComponent<Player>();
        if (player != null) {
            player.OnTouchHarm();
        }
    }
    
    
}
