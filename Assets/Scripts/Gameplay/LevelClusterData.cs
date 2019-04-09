using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelClusterData {
    // Properties
    public int ClusterIndex { get; private set; }
    // References
    public List<LevelData> levels=new List<LevelData>();
    
    
    public LevelClusterData(int ClusterIndex) {
        this.ClusterIndex = ClusterIndex;
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
