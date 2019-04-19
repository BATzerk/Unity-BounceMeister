using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBackground : MonoBehaviour {
    // Components
    [SerializeField] private SpriteRenderer sr_fill=null;
    [SerializeField] private SpriteRenderer sr_gradient=null;
    
    // Getters (Private)
    static private List<Color255> GetGradientColors(int worldIndex) {
        switch (worldIndex) {
            //case 0:  return new List<Color255>{
            //    new Color255(242,251,139), new Color255(133,185,255) };
            //case 1:  return new List<Color255>{
            //    new Color255(255,174,166), new Color255(205,240,180) };
            //case 2:  return new List<Color255>{
            //    new Color255(133,255,176), new Color255(255,114,160) };
            //case 3:  return new List<Color255>{
            //    new Color255(255,213,133), new Color255(201,177,255) };
            //case 4:  return new List<Color255>{
            //    new Color255(255,166,193), new Color255(220,174,255) };
            //case 5:  return new List<Color255>{
                //new Color255( 78, 14, 89), new Color255(138,202,191) };
            default: return new List<Color255>{
                new Color255(255,249,246), new Color255(136,190,213) };
            //case 0:  return new List<Color255>{
            //    new Color255(242,251,139), new Color255(133,185,255) };
            //case 1:  return new List<Color255>{
            //    new Color255(255,174,166), new Color255(205,240,180) };
            //case 2:  return new List<Color255>{
            //    new Color255(133,255,176), new Color255(255,114,160) };
            //case 3:  return new List<Color255>{
            //    new Color255(255,213,133), new Color255(201,177,255) };
            //case 4:  return new List<Color255>{
            //    new Color255(255,166,193), new Color255(220,174,255) };
            //case 5:  return new List<Color255>{
            //    new Color255( 78, 14, 89), new Color255(138,202,191) };
            //default: return new List<Color255>{
                //new Color255(255,255,255), new Color255(128,128,128) };
        }
    }


    // ----------------------------------------------------------------
    //  Awake / Destroy
    // ----------------------------------------------------------------
    private void Awake() {
        // Add event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent += OnStartRoom;
    }
    private void OnDestroy() {
        // Remove event listeners!
        GameManagers.Instance.EventManager.StartRoomEvent -= OnStartRoom;
    }
    
    // ----------------------------------------------------------------
    //  Events
    // ----------------------------------------------------------------
    private void OnStartRoom(Room room) {
        SetGradientColors(room.WorldIndex);
    }
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    private void SetGradientColors(int worldIndex) {
        List<Color255> colors = GetGradientColors(worldIndex);
        Color colorA = colors[0].ToColor();
        Color colorB = colors[1].ToColor();
        sr_fill.color = colorA;
        sr_gradient.color = colorB;
    }
    


    /*
	// Components
	private SpriteRenderer[,] tileSprites; // col,row.
	// Properties
	private int numCols,numRows;
	private Vector2 gridSize;
	// References
	[SerializeField] private GameCameraController cameraControllerRef;

	// Getters
	private float unitSize { get { return GameProperties.UnitSize; } }


	private void Start () {
		numCols = 20; // TO DO: set these dynamically
		numRows = 20;
		gridSize = new Vector2(numCols*unitSize, numRows*unitSize);
		tileSprites = new SpriteRenderer[numCols,numRows];
		for (int col=0; col<numCols; col++) {
			for (int row=0; row<numRows; row++) {
				SpriteRenderer newSprite = Instantiate(ResourcesHandler.Instance.backgroundTileSprite).GetComponent<SpriteRenderer>();
				newSprite.transform.SetParent(this.transform);
				newSprite.name = "BGTile_" + col + "," + row;
				GameUtils.SizeSpriteRenderer (newSprite, unitSize,unitSize);
				newSprite.color = new Color(0,0,0, Random.Range(0, 0.4f));
//				newSprite.sortingOrder = 1;
				newSprite.transform.localPosition = new Vector3((col+0.5f)*unitSize, (row)*unitSize) - new Vector3(gridSize.x,gridSize.y)*0.5f;//NOTE: idk why not row+0.5f, too. :P
				tileSprites[col,row] = newSprite;
			}
		}
		PositionTileSprites();
	}

	private void FixedUpdate() {
		PositionTileSprites(); // TO DO: NOT this every frame! Only when the camera enters a new zone. Doy.
	}

	private void PositionTileSprites() {
		if (tileSprites == null) { return; } // Safety check (for runtime compile).
		// Snap MY position to the camera's grid pos!
		Rect viewRect = cameraControllerRef.ViewRect;
		Vector2 viewCenter = viewRect.center;
		this.transform.localPosition = new Vector3(Mathf.Round(viewCenter.x/unitSize)*unitSize, Mathf.Round(viewCenter.y/unitSize)*unitSize);

		float noiseScale = 72.7f;
		Vector3 noiseOffset = new Vector3(9999,7999);
		for (int col=0; col<numCols; col++) {
			for (int row=0; row<numRows; row++) {
				SpriteRenderer tile = tileSprites[col,row];
				Vector2 tilePos = tile.transform.position * noiseScale + noiseOffset;
//				float alpha = (1+Mathf.Sin(tilePos.x*tilePos.y*999.3f))*0.5f * 0.4f;
				float alpha = Mathf.PerlinNoise(tilePos.x, tilePos.y) * 0.13f;
//				Debug.Log(alpha + "  " + tilePos);
				tile.color = new Color(0,0,0, alpha);
			}
		}
//		float x,y;
//		int safetyCount = 0;
//		for (int col=0; col<numCols; col++) {
//			for (int row=0; row<numRows; row++) {
//				x = (col+0.5f)*unitSize;
//				y = (row+0.5f)*unitSize;
////				x += 
//				// Kinda ha cked in for now. While loops not the right way to do this.
//				if (x<viewRect.xMin) {
//					while (x<viewRect.xMin && safetyCount++<99) { x += gridSize.x; }
//				}
//				else {
//					while (x>viewRect.xMax && safetyCount++<99) { x -= gridSize.x; }
//				}
////				if (x<viewRect.xMin) {
////					while (x<viewRect.xMin) { x += gridSize.x; }
////				}
////				else {
////					while (x>viewRect.xMax) { x -= gridSize.x; }
////				}
//				tileSprites[col,row].transform.localPosition = new Vector3(x, y);
//			}
//		}
	}
		*/

}
