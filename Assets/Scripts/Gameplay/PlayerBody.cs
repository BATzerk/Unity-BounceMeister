using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour {
	// Components
	[SerializeField] private SpriteRenderer sr_body=null;
	[SerializeField] private SpriteLine sl_aimDir=null;
	// Properties
//	private readonly Color bodyColor_dashInitial = new Color(255/255f, 255/255f, 255/255f);
//	private readonly Color bodyColor_dashing1 = new ColorHSB(183/360f, 66/100f, 90/100f).ToColor();
//	private readonly Color bodyColor_dashing2 = new ColorHSB(272/360f, 66/100f, 90/100f).ToColor();
	private readonly Color bodyColor_neutral = new Color(25/255f, 175/255f, 181/255f);
//	private readonly Color bodyColor_outOfDashes = new Color(128/255f, 128/255f, 128/255f);
//	private float aimDirRadius;
	// References
	[SerializeField] private Player myPlayer=null;

	// Getters
//	private Vector2 AimDir { get { return myPlayer.AimDir; } }



	// ----------------------------------------------------------------
	//  Start / Destroy
	// ----------------------------------------------------------------
	private void Start() {
		sr_body.color = bodyColor_neutral;

		// Add event listeners!
//		GameManagers.Instance.EventManager.PlayerDashEvent += OnPlayerDash;
	}
	private void OnDestroy() {
		// Remove event listeners!
//		GameManagers.Instance.EventManager.PlayerDashEvent -= OnPlayerDash;
	}



	// ----------------------------------------------------------------
	//  Doers
	// ----------------------------------------------------------------
	public void SetSize(Vector2 _size) {
		GameUtils.SizeSpriteRenderer(sr_body, _size);
		sl_aimDir.SetColor(Color.white);
		sl_aimDir.SetThickness(_size.magnitude*0.12f);
		sl_aimDir.StartPos = Vector2.zero;
//		aimDirRadius = Mathf.Min(_size.x,_size.y) * 0.8f;
	}


	// ----------------------------------------------------------------
	//  Events
	// ----------------------------------------------------------------
	public void OnStartBouncing() {
		sr_body.color = Color.green;
	}
	public void OnStopBouncing() {
		sr_body.color = bodyColor_neutral;
	}

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
		sr_body.color = bodyColor_neutral;
	}


	// ----------------------------------------------------------------
	//  Update
	// ----------------------------------------------------------------
	private void Update() {
		if (Time.timeScale == 0) { return; } // No time? No dice.

//		sr_body.color = myPlayer.OnGround ? bodyColor_neutral : Color.yellow;
//		UpdateAimDirLine();
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




}



