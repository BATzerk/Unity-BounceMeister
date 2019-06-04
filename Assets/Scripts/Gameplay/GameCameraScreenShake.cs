using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCameraScreenShake : MonoBehaviour {
    // Properties
    private float posXVol; // screen-shake position volume
    private float posYVol; // screen-shake position volume
    //private float posXVolVel; // screen-shake position volume
    //private float posYVolVel; // screen-shake position volume
    //private Vector2 posVolVel; // screen-shake position volume velocity
    private float rotVol; // screen-shake rotation volume
    private float rotVolVel; // screen-shake rotation volume velocity
    
    public float ShakeRot { get; private set; }
    public Vector2 ShakePos { get; private set; }


    // ----------------------------------------------------------------
    //  Start / Destroy
    // ----------------------------------------------------------------
    private void Start() {
        // Add event listeners!
        GameManagers.Instance.EventManager.PlayerDieEvent += OnPlayerDie;
        GameManagers.Instance.EventManager.PlayerUseBatteryEvent += OnPlayerUseBattery;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.PlayerDieEvent -= OnPlayerDie;
        GameManagers.Instance.EventManager.PlayerUseBatteryEvent += OnPlayerUseBattery;
    }
    
    // ----------------------------------------------------------------
    //  Reset
    // ----------------------------------------------------------------
    public void Reset() {
        posXVol = 0;
        posYVol = 0;
        //posXVolVel = 0;
        //posYVolVel = 0;
        rotVol = 0;
        rotVolVel = 0;
        ShakePos = Vector2.zero;
        ShakeRot = 0;
    }
    
    

    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnPlayerDie(Player player) {
        rotVolVel = 0.7f;
//      fullScrim.FadeFromAtoB(Color.clear, new Color(1,1,1, 0.2f), 1f, true);
    }
    private void OnPlayerUseBattery() {
        posXVol = 1f;
        posYVol = 0.4f;
    }
    
    
    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        UpdateShakeRot();
        UpdateShakePos();
    }
    private void UpdateShakePos() {
        if (posXVol==0 && posYVol==0) { return; }
        
        // Calculate posOffset.
        ShakePos = new Vector2(
            //Random.Range(-posXVol, posXVol)*5,
            //posXVol * (Time.frameCount%2==0 ? -1 : 1),
            //posXVol * (Time.time%0.2<0.1f ? -1 : 1),
            //Mathf.Sin(Time.time*60) * posXVol*0.5f,
            //Mathf.Sin(Time.time*60) * 0.3f,
            //Mathf.Sin(posXVol*.4f) * posXVol*0.7f,
            Mathf.Sin(Time.time*40) * posXVol*0.4f,
            //Random.Range(-posXVol, posXVol)*0.7f,
            Mathf.Sin(Time.time*41+4) * posYVol*0.4f);
            
        // Ease posXVol/posYVol to 0.
        posXVol += (0-posXVol) * 0.3f;
        posYVol += (0-posYVol) * 0.3f;
        //posXVol -= 0.08f;
        //posYVol -= 0.08f;
        if (posXVol < 0.1f) { // Almost at 0? Get it!
            posXVol = 0;
            ShakePos = new Vector2(0, ShakePos.y);
        }
        if (posYVol < 0.1f) { // Almost at 0? Get it!
            posYVol = 0;
            ShakePos = new Vector2(ShakePos.x, 0);
        }
    }
    private void UpdateShakeRot() {
        if (rotVol==0 && rotVolVel==0) {
            return;
        }
        rotVol += rotVolVel;
        rotVolVel += (0-rotVol) / 5f;
        rotVolVel *= 0.9f;
        if (rotVol != 0) {
            if (Mathf.Abs (rotVol) < 0.001f && Mathf.Abs (rotVolVel) < 0.001f) {
                rotVol = 0;
                rotVolVel = 0;
            }
        }

        ShakeRot = rotVol;
    }
}
