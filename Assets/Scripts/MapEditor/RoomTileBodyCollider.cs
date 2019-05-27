using UnityEngine;
using System.Collections;

namespace MapEditorNamespace {
/** This guy's just so we can have MULTIPLE colliders that serve multiple purposes in a RoomTile. */
public class RoomTileBodyCollider : MonoBehaviour {
	// Components
	[SerializeField] private BoxCollider2D boxCollider=null;
	// References
	[SerializeField] private RoomTile roomTileRef=null;
        
    // Getters
    public bool IsEnabled { get { return boxCollider.enabled; } }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
//	public void UpdatePosAndSize (float x,float y, float w,float h) {
	public void UpdatePosAndSize(Rect rect) {
		boxCollider.transform.localPosition = new Vector3 (rect.center.x,rect.center.y, 0);
		boxCollider.size = rect.size;
	}
    public void SetIsEnabled(bool val) {
        boxCollider.enabled = val;
    }

	
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
	private void OnMouseEnter() {
		roomTileRef.OnMouseEnterBodyCollider();
	}
	private void OnMouseExit() {
		roomTileRef.OnMouseExitBodyCollider();
	}


}
}
