using UnityEngine;
using System.Collections;

namespace MapEditorNamespace {
public class RoomTileDesignerFlag : MonoBehaviour {
	// Constants
    private const int NUM_FLAG_TYPES = 5; // Dev, Idea, In Progress, Good, Great!
	private const float SPRITE_ALPHA_DEFAULT = 0.9f;
	private const float SPRITE_ALPHA_MOUSE_OVER = 0.4f;
	// Components
	[SerializeField] private SpriteRenderer flagSprite=null;
	// References
	[SerializeField] private RoomTile roomTileRef=null;
	[SerializeField] private Sprite[] designerFlagSprites=null;


	private void Start () {
		GameUtils.SetSpriteAlpha (flagSprite, SPRITE_ALPHA_DEFAULT);
	}
	
	public void UpdateDesignerFlagButtonVisuals() {
		flagSprite.sprite = designerFlagSprites [roomTileRef.MyRoomData.DesignerFlag];
	}
	public void ApplyPosAndSize (Rect boundsBL) {
		const float w = 16;
		const float h = 16;
		float x = boundsBL.center.x - (boundsBL.size.x-w)*0.5f;
		float y = boundsBL.center.y + (boundsBL.size.y-h)*0.5f;
		this.transform.localPosition = new Vector3 (x, y, -1); // Move closer to camera than the main contents' bodyCollider.
		GameUtils.SizeSpriteRenderer (flagSprite, w,h, true);
	}
	
	
	
	private void OnMouseEnter() {
		GameUtils.SetSpriteAlpha (flagSprite, SPRITE_ALPHA_MOUSE_OVER);
	}
	private void OnMouseExit() {
		GameUtils.SetSpriteAlpha (flagSprite, SPRITE_ALPHA_DEFAULT);
	}
	private void OnMouseDown() {
        RoomData rd = roomTileRef.MyRoomData;
		// Determine the new value of our flag!
		int newDesignerFlagValue = rd.DesignerFlag + 1;
		if (newDesignerFlagValue >= NUM_FLAG_TYPES) { newDesignerFlagValue = 0; } // Loop back to 0.
		// Set and save!
		rd.SetDesignerFlag(newDesignerFlagValue);
		RoomSaverLoader.UpdateRoomPropertiesInRoomFile(rd);
		// Update the designerFlag button
		UpdateDesignerFlagButtonVisuals();
	}


}
}




