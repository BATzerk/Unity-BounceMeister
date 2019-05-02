using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapRoomTile : MonoBehaviour {
    // Components
    [SerializeField] private RectTransform myRectTransform=null;
    [SerializeField] private Image i_back=null;
    [SerializeField] private Image i_snack=null; // hidden if no snack
    private ImageLine[] i_borders; // index corresponds to side. Null if no border.
    // References
    public RoomData MyRoomData { get; private set; }
    
    // Getters
    private Color BackColor(RoomData currRD) {
        bool isCurrRoom = MyRoomData == currRD;
        //bool areEdiblesLeft = MyRoomData.AreEdiblesLeft();
        bool hasPlayerBeenHere = MyRoomData.HasPlayerBeenHere;
        if (isCurrRoom) { return new Color255(215,202,62).ToColor(); }
        if (!hasPlayerBeenHere) { return new Color(0.1f,0.1f,0.1f, 0.5f); }
        //if (areEdiblesLeft) { return new Color255(135,200,210).ToColor(); }
        //return new Color255(105,120,123, 80).ToColor();
        return new Color255(135,200,210).ToColor();
    }
    private bool IsBorderLine(int side) {
        for (int i=0; i<MyRoomData.Openings.Count; i++) {
            if (!MyRoomData.Openings[i].IsRoomTo) { continue; } // No neighboring room? No line.
            if (MyRoomData.Openings[i].side == side) { return false; } // There's an opening at this side!
        }
        return true; // No neighbor at this side?? Yes border line!
    }
    private Line GetBorderLine(int side) {
        Rect offsetedRect = MyRoomData.BoundsLocal;
        offsetedRect.position -= MyRoomData.cameraBoundsData.myRect.center; // hack-y! Just getting to work for now. Works around the rooms' local/global alignment mismatch.
        return MathUtils.GetRectSideLine(offsetedRect, side);
    }
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Transform parentTF, RoomData myRoomData) {
        this.MyRoomData = myRoomData;
        
        // Parent jazz!
        GameUtils.ParentAndReset(this.gameObject, parentTF);
        gameObject.name = "MapTile " + myRoomData.RoomKey;
        
        // Size/position!
        myRectTransform.sizeDelta = myRoomData.BoundsGlobal.size;// - new Vector2(5,5); // TEST shrink 'em all a little
        myRectTransform.anchoredPosition = myRoomData.BoundsGlobal.position;
        
        // Border lines!
        MakeBorderLines();
    }
    private void MakeBorderLines() {
        i_borders = new ImageLine[4];
        for (int side=0; side<i_borders.Length; side++) {
            if (IsBorderLine(side)) {
                MakeBorderLine(side);
            }
        }
    }
    private void MakeBorderLine(int side) {
        Line line = GetBorderLine(side);
        ImageLine imageLine = Instantiate(ResourcesHandler.Instance.imageLine).GetComponent<ImageLine>();
        imageLine.Initialize(this.transform, line.start,line.end);
        imageLine.name = "BorderLine_" + side;
        imageLine.SetThickness(4);
        imageLine.SetColor(new Color(0,0,0, 0.75f));
        i_borders[side] = imageLine;
    }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void UpdateVisuals(RoomData currRD, PlayerTypes currPlayerType) {
        // Not my cluster? Hide.
        bool isMyCluster = currRD.ClusterIndex == MyRoomData.ClusterIndex;
        if (isMyCluster) {
            this.gameObject.SetActive(true);
            // Back color.
            i_back.color = BackColor(currRD);
            // Snack icon.
            i_snack.enabled = MyRoomData.SnackCount.AreSnacks(currPlayerType) && MyRoomData.HasPlayerBeenHere;
            bool areSnacksLeft = MyRoomData.SnackCount.AreUneatenSnacks(currPlayerType);
            i_snack.color = areSnacksLeft ? new Color255(190,230,50).ToColor() : new Color(0,0,0, 0.14f);
        }
        else {
            this.gameObject.SetActive(false);
        }
    }
    
    
    
}
