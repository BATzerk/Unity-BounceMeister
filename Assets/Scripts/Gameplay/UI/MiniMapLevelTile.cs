using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapLevelTile : MonoBehaviour {
    // Components
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private Image i_back;
    //[SerializeField] private Image i_stroke;
    [SerializeField] private Image i_snack; // hidden if no snack
    private ImageLine[] i_borders; // index corresponds to side. Null if no border.
    // References
    public LevelData MyLevelData { get; private set; }
    
    // Getters
    private Color BackColor(LevelData currLD) {
        bool isCurrLevel = MyLevelData == currLD;
        //bool areEdiblesLeft = MyLevelData.AreEdiblesLeft();
        bool hasPlayerBeenHere = MyLevelData.HasPlayerBeenHere;
        if (isCurrLevel) { return new Color255(215,202,62).ToColor(); }
        if (!hasPlayerBeenHere) { return new Color(0.1f,0.1f,0.1f, 0.5f); }
        //if (areEdiblesLeft) { return new Color255(135,200,210).ToColor(); }
        //return new Color255(105,120,123, 80).ToColor();
        return new Color255(135,200,210).ToColor();
    }
    private bool IsBorderLine(int side) {
        for (int i=0; i<MyLevelData.Neighbors.Count; i++) {
            if (!MyLevelData.Neighbors[i].IsLevelTo) { continue; } // No neighboring level? No line.
            if (MyLevelData.Neighbors[i].OpeningFrom.side == side) { return false; } // There's a neighbor at this side!
        }
        return true; // No neighbor at this side?? Yes border line!
    }
    private Line GetBorderLine(int side) {
        return MathUtils.GetRectSideLine(MyLevelData.BoundsLocal, side);
    }
    
    
    // ----------------------------------------------------------------
    //  Initialize
    // ----------------------------------------------------------------
    public void Initialize(Transform parentTF, LevelData myLevelData) {
        this.MyLevelData = myLevelData;
        
        // Parent jazz!
        GameUtils.ParentAndReset(this.gameObject, parentTF);
        gameObject.name = "MapTile " + myLevelData.levelKey;
        
        // Size/position!
        myRectTransform.sizeDelta = myLevelData.BoundsGlobal.size;// - new Vector2(5,5); // TEST shrink 'em all a little
        myRectTransform.anchoredPosition = myLevelData.BoundsGlobal.position;
        
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
    public void UpdateVisuals(LevelData currLD) {
        // Not my cluster? Hide.
        bool isMyCluster = currLD.ClusterIndex == MyLevelData.ClusterIndex;
        if (isMyCluster) {
            this.gameObject.SetActive(true);
            // Back color.
            i_back.color = BackColor(currLD);
            // Snack icon.
            i_snack.enabled = MyLevelData.NumSnacksTotal > 0 && MyLevelData.HasPlayerBeenHere;
            bool areSnacksLeft = MyLevelData.NumSnacksCollected < MyLevelData.NumSnacksTotal;
            i_snack.color = areSnacksLeft ? Color.white : new Color(0,0,0, 0.14f);
        }
        else {
            this.gameObject.SetActive(false);
        }
    }
    
    
    
}
