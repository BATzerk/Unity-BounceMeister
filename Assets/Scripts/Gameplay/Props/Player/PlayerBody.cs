using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class PlayerBody : MonoBehaviour {
    // Components
    [SerializeField] private GameObject go_wallSliding=null;
    [SerializeField] private ParticleSystem ps_dieBurst=null;
    [SerializeField] protected PlayerBodyEyes eyes=null;
    [SerializeField] private SpriteRenderer sr_body=null;
	// Properties
	protected Color bodyColor_neutral = Color.magenta;
	private Color bodyColor;
	private float alpha; // we modify this independently of bodyColor.
	// References
	protected Player myBasePlayer=null; // set in Awake.

    static public Color GetBodyColorNeutral(PlayerTypes playerType) {
        switch (playerType) {
            case PlayerTypes.Any: return new ColorHSB(60/360f, 0.67f, 1f).ToColor();
            case PlayerTypes.Dilata: return new ColorHSB(190/360f, 0.67f, 0.87f).ToColor();
            case PlayerTypes.Flatline: return new ColorHSB(160/360f, 0.67f, 0.87f).ToColor();
            case PlayerTypes.Flippa: return new ColorHSB(330/360f, 0.8f, 0.82f).ToColor();
            case PlayerTypes.Jetta: return new ColorHSB(290/360f, 0.7f, 0.7f).ToColor();
            case PlayerTypes.Jumpa: return new ColorHSB(100/360f, 0.6f, 0.7f).ToColor();
            case PlayerTypes.Limo: return new ColorHSB(140/360f, 0.05f, 0.5f).ToColor();
            case PlayerTypes.Plunga: return new Color255(25, 175, 181).ToColor();
            case PlayerTypes.Slippa: return new Color255(220, 160, 40).ToColor();
            case PlayerTypes.Warpa: return new ColorHSB(250/360f, 0.5f, 0.9f).ToColor();
            default: Debug.LogWarning("PlayerBody color not defined: " + playerType + "."); return Color.magenta; // Oops.
        }
    }


    // ----------------------------------------------------------------
    //  Start
    // ----------------------------------------------------------------
    virtual protected void Awake() {
        myBasePlayer = GetComponentInParent<Player>();
    }
    virtual protected void Start() {
		bodyColor_neutral = GetBodyColorNeutral(myBasePlayer.PlayerType());

		alpha = 1;
		SetBodyColor(bodyColor_neutral);
        SetVisualScale(Vector2.one);
        OnStopWallSlide();
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	//virtual public void SetSize(Vector2 _size) {
	//	GameUtils.SizeSpriteRenderer(sr_body, _size);
	//}
    /// Changes the visuals of our body components without affecting collisions.
    virtual protected void SetVisualScale(Vector2 _scale) {
        Vector2 _size = myBasePlayer.Size * _scale;
        GameUtils.SizeSpriteRenderer(sr_body, _size);
    }
	protected void SetBodyColor(Color color) {
		bodyColor = color;
		ApplyBodyColor();
	}
	private void ApplyBodyColor() {
		sr_body.color = new Color(bodyColor.r,bodyColor.g,bodyColor.b, bodyColor.a*alpha);
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnEndPostDamageImmunity() {
		alpha = 1f;
		ApplyBodyColor();
	}
    public void OnDropThruPlatform() {
        LeanTween.cancel(this.gameObject);
        Vector2 _scale = new Vector2(0.5f, 1.2f);
        SetVisualScale(_scale);
        LeanTween.value(this.gameObject, SetVisualScale, _scale,Vector2.one, 0.2f).setDelay(0.1f).setEaseOutQuart();
    }
    public void OnEatEdiblesHolding() {
        eyes.OnEatEdiblesHolding();
    }

	public void OnDie() {
		// Cheap way to get a particle burst: Just chuck my ParticleSystem onto my Player's parent transform the moment before we're destroyed!
		ps_dieBurst.gameObject.SetActive(true);
		ps_dieBurst.transform.SetParent(myBasePlayer.transform.parent);
		ps_dieBurst.Emit(40);
//		print("Enabled? " + ps_dieBurst.inheritVelocity.enabled);
		//// Give all the particles the velocity of my Player!
		//ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps_dieBurst.particleCount];
		//ps_dieBurst.GetParticles(particles);
		//for (int i=0; i<particles.Length; i++) {
		//	ParticleSystem.Particle particle = particles[i];
		//	float velScale = Random.Range(6f, 12f);
		//	particle.velocity += new Vector3(myPlayer.Vel.x, myPlayer.Vel.y, 0) * velScale;
		//	particles[i] = particle;
		//}
		//// Set the array back to the particleSystem
		//ps_dieBurst.SetParticles(particles, particles.Length);
	}

    public void OnStartWallSlide() {
        go_wallSliding.SetActive(true);
    }
    public void OnStopWallSlide() {
        go_wallSliding.SetActive(false);
    }


    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
		if (!myBasePlayer.DoUpdate()) { return; } // Not updating? No dice.

        // Face correct dir
        this.transform.localScale = new Vector3(myBasePlayer.DirFacing, 1, 1);

        // Flash from damage
		if (myBasePlayer.IsPostDamageImmunity) {
			alpha = Random.Range(0.2f, 0.6f);
			ApplyBodyColor();
		}
	}




}

/*
	public void OnDash() {
//		Color color;
//		switch (myPlayer.NumDashesSinceGround) {
//			case 1: color = bodyColor_dashing1; break;
////			case 2: color = bodyColor_dashing2; break;
//			default: color = bodyColor_outOfDashes; break;
//		}
//		sr_body.color = color;
	}
	public void OnDashEnd() {
//		sr_body.color = bodyColor_neutral;
//		Color color;
//		switch (myPlayer.NumDashesSinceGround) {
//		case 0: color = bodyColor_neutral; break;
//		case Player.MaxDashes-2: color = bodyColor_dashing1; break;
//		case Player.MaxDashes-1: color = bodyColor_dashing1; break;
//		case Player.MaxDashes:   color = bodyColor_outOfDashes; break;
//		default: color = bodyColor_neutral; break;
//		}
//		sr_body.color = color;
	}
	public void OnRechargeDash() {
		bodyColor = bodyColor_neutral;
		ApplyBodyColor();
	}
//	private void UpdateAimDirLine() {
//		sl_aimDir.StartPos = Vector2.zero;
//		sl_aimDir.EndPos = AimDir * aimDirRadius;
//	}


//	private void FixedUpdate() {
//		UpdateBodyColor();
//	}
//	private void UpdateBodyColor() {
//		if (myPlayer.IsDashing) {
//			sr_body.color = Color.Lerp(sr_body.color, bodyColor_dashing, 0.3f); // Ease FAST into our dashing color.
//		}
//	}
	*/



