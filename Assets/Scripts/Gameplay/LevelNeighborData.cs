using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelNeighborData {
    // References
    public LevelData LevelTo { get; private set; }
    public LevelOpening OpeningFrom { get; private set; }
    // Getters
    public bool IsLevelTo { get { return LevelTo != null; } } // Some NeighborDatas DON'T have neighbors, while we're moving around/editing levels.

    // Initialize
    public LevelNeighborData(LevelData levelTo, LevelOpening openingFrom) {
        this.LevelTo = levelTo;
        this.OpeningFrom = openingFrom;
    }
}
