using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapLevelTile : MonoBehaviour {
    // Components
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private Image i_back;
    [SerializeField] private Image i_stroke;
    // References
    public LevelData MyLevelData { get; private set; }
    
    // Getters
    private Color BackColor(LevelData currLD) {
        bool isCurrLevel = MyLevelData == currLD;
        bool areEdiblesLeft = MyLevelData.AreEdiblesLeft();
        if (isCurrLevel) { return new Color255(215,202,62).ToColor(); }
        if (areEdiblesLeft) { return new Color255(105,120,123).ToColor(); }
        return new Color255(105,120,123, 80).ToColor();
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
    }
    
    
    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    public void UpdateVisuals(LevelData currLD) {
        // Not my cluster? Hide.
        bool isMyCluster = currLD.ClusterIndex == MyLevelData.ClusterIndex;
        print(currLD.ClusterIndex + "  " + MyLevelData.ClusterIndex);
        if (isMyCluster) {
            this.gameObject.SetActive(true);
            i_back.color = BackColor(currLD);
        }
        else {
            this.gameObject.SetActive(false);
        }
    }
    
    
    
}
