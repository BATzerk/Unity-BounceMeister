using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class PlayerBody : MonoBehaviour {
    // Components
    [SerializeField] private GameObject go_wallSliding=null;
    [SerializeField] private SpriteRenderer sr_body=null;
    [SerializeField] private ParticleSystem ps_dieBurst=null;
	// Properties
	protected Color bodyColor_neutral = Color.magenta;
	private Color bodyColor;
	private float alpha; // we modify this independently of bodyColor.
	// References
	[SerializeField] protected Player myPlayer=null;


	// ----------------------------------------------------------------
	//  Start
	// ----------------------------------------------------------------
	virtual protected void Start() {
		alpha = 1;
		SetBodyColor(bodyColor_neutral);
        OnStopWallSlide();
	}


	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void SetSize(Vector2 _size) {
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

	public void OnDie() {
		// Cheap way to get a particle burst: Just chuck my ParticleSystem onto my Player's parent transform the moment before we're destroyed!
		ps_dieBurst.gameObject.SetActive(true);
		ps_dieBurst.transform.SetParent(myPlayer.transform.parent);
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

    public void OnStartWallSlide(int wallSlideSide) {
        go_wallSliding.SetActive(true);
        //go_wallSliding.transform.localScale = new Vector3(wallSlideSide, 1,1);
    }
    public void OnStopWallSlide() {
        go_wallSliding.SetActive(false);
    }


    // ----------------------------------------------------------------
    //  Update
    // ----------------------------------------------------------------
    private void Update() {
		if (!myPlayer.DoUpdate()) { return; } // Not updating? No dice.

        // Face correct dir
        this.transform.localScale = new Vector3(myPlayer.DirFacing, 1, 1);

        // Flash from damage
		if (myPlayer.IsPostDamageImmunity) {
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



