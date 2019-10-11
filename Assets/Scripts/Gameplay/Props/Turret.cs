using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Prop {
    // Components
    [SerializeField] private SpriteRenderer sr_body=null;
    // Properties
    [SerializeField] private float interval = 1; // bullet interval in SECONDS.
    [SerializeField] private float speed = 0.05f; // bullet speed per FRAME.
    [SerializeField] private float startOffset = 0f; // delay before first bullet.
    private float timeUntilShoot; // counts down to 0, in SECONDS.
    private Vector3 bodyScaleNeutral;

    // Getters
    public float Interval { get { return interval; } }
    public float Speed { get { return speed; } }


    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    protected override void Start() {
        bodyScaleNeutral = sr_body.transform.localScale;
        base.Start();
    }

    public void Initialize(Room _myRoom, TurretData data) {
        base.InitializeAsProp(_myRoom, data);

        this.interval = data.interval;
        this.speed = data.speed;
        this.startOffset = data.startOffset;
        
        SetTimeUntilShoot(startOffset);
    }
    override protected void OnCreatedInEditor() {
        base.OnCreatedInEditor();
        SetTimeUntilShoot(0);// + interval);
    }



    // ----------------------------------------------------------------
    //  FixedUpdate
    // ----------------------------------------------------------------
    private void FixedUpdate() {
        timeUntilShoot -= GameTimeController.RoomDeltaTime;
        MaybeShoot();
    }

    private void MaybeShoot() {
        if (timeUntilShoot <= 0) {
            Shoot();
        }
    }
    

    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetTimeUntilShoot(float _time) {
        timeUntilShoot = _time;
        // Prep pre-shoot tween!
        float delay = Mathf.Max(0, interval-0.2f);
        Invoke("PreShootTween", delay);
    }
    private void Shoot() {
        // Shoot a bullet!
        TurretBullet bullet = Instantiate(ResourcesHandler.Instance.TurretBullet).GetComponent<TurretBullet>();
        bullet.Initialize(MyRoom, this);

        // Tween!
        LeanTween.cancel(sr_body.gameObject);
        sr_body.transform.localScale = new Vector3(bodyScaleNeutral.x*0.5f, bodyScaleNeutral.y*1.2f, 1f);
        LeanTween.moveLocal(sr_body.gameObject, Vector3.zero, 0.4f).setEaseOutQuart();
        LeanTween.scale(sr_body.gameObject, bodyScaleNeutral, 0.4f).setEaseOutQuart();

        // Plan next shot!
        SetTimeUntilShoot(timeUntilShoot + interval);
    }
    private void PreShootTween() {
        const float dur = 0.15f;
        Vector3 posTo = new Vector3(0, -0.3f, 0);
        LeanTween.moveLocal(sr_body.gameObject, posTo, dur).setEaseInBack();
        //Vector3 scaleTo = new Vector3(bodyScaleNeutral.x*0.7f, bodyScaleNeutral.y*1.2f, 1f);
        //LeanTween.scale(sr_body.gameObject, scaleTo,dur).setEaseInBack();
    }
    
    // ----------------------------------------------------------------
    //  Serializing
    // ----------------------------------------------------------------
    override public PropData ToData() {
        return new TurretData {
            pos = pos,
            rotation = rotation,
            interval = interval,
            speed = speed,
            startOffset = startOffset,
            travelMind = new TravelMindData(travelMind),
        };
    }

}
