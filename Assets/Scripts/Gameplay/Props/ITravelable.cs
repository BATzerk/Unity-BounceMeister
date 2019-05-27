using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Add this to Props we want to move from PosA to PosB. */
public interface ITravelable {
    // Getters
    bool HasTravelMind();
    // Doers
    void AddTravelMind(TravelMindData data);
    void RemoveTravelMind();
    Vector2 GetPos();
    void SetPos(Vector2 _pos);
}
