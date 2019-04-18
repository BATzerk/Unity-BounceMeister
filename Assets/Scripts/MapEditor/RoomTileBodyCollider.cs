using UnityEngine;
using System.Collections;

/** This guy's just so we can have MULTIPLE colliders that serve multiple purposes in a RoomTile. */
public class RoomTileBodyCollider : MonoBehaviour {
	// Components
	[SerializeField] private BoxCollider2D boxCollider=null;
	// References
	[SerializeField] private RoomTile roomTileRef=null;


//	public void UpdatePosAndSize (float x,float y, float w,float h) {
	public void UpdatePosAndSize(Rect rect) {
		boxCollider.transform.localPosition = new Vector3 (rect.center.x,rect.center.y, 0);
		boxCollider.size = rect.size;
	}
    
    public bool IsEnabled { get { return boxCollider.enabled; } }
    public void SetIsEnabled(bool val) {
        boxCollider.enabled = val;
    }

	
	private void OnMouseEnter() {
		roomTileRef.OnMouseEnterBodyCollider();
	}
	private void OnMouseExit() {
		roomTileRef.OnMouseExitBodyCollider();
	}


}
