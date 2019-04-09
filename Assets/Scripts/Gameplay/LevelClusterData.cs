using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelClusterData {
    // Properties
    public int ClusterIndex { get; private set; }
    public int NumGems { get; private set; }
    public int NumGemsCollected { get; private set; }
    public int NumSnacks { get; private set; }
    public int NumSnacksCollected { get; private set; }
    // References
    public List<LevelData> levels=new List<LevelData>();
    
    // Initialize
    public LevelClusterData(int ClusterIndex) {
        this.ClusterIndex = ClusterIndex;
    }
    
    // Doers
    public void UpdateCollectablesCounts() {
        NumGems = 0;
        NumGemsCollected = 0;
        NumSnacks = 0;
        NumSnacksCollected = 0;
        for (int l=0; l<levels.Count; l++) {
            for (int p=0; p<levels[l].allPropDatas.Count; p++) {
                //NumGems += levels[l].gemDatas.Count;
                if (levels[l].allPropDatas[p] is GemData) {
                    NumGems ++;
                }
                else if (levels[l].allPropDatas[p] is SnackData) {
                    NumSnacks ++;
                }
                // TODO: Gems/Snacks collected!
            }
        }
    }
    
    
    
    
    
//  public static int GetNumRegularStarsInLevelsConnectedToStart (Dictionary<string, LevelData> levelDatas) {
//      int total = 0;
//      foreach (LevelData ld in levelDatas.Values) {
//          if (ld.isConnectedToStart) {
//              foreach (StarData starData in ld.starDatas) { if (!starData.isSecretStar && !starData.isNotYours) { total += ld.starDatas.Count; } }
//          }
//      }
//      return total;
//  }
//  public static int GetNumSecretStarsInLevelsConnectedToStart (Dictionary<string, LevelData> levelDatas) {
//      int total = 0;
//      foreach (LevelData ld in levelDatas.Values) {
//          if (ld.isConnectedToStart) {
//              foreach (StarData starData in ld.starDatas) { if (starData.isSecretStar) { total += ld.starDatas.Count; } }
//          }
//      }
//      return total;
//  }
}
