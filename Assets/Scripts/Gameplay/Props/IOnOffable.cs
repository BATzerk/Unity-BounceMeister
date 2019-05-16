using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Add this to Props we want to allow to turn on/off in a pattern, like Lasers or Spikes. */
public interface IOnOffable {
    // Getters
    bool IsOn();
    bool HasOnOffer();
    // Doers
    void AddOnOffer(OnOfferData data);
    void RemoveOnOffer();
    void SetIsOn(bool isOn);
    void UpdateAlmostOn(float timeUntilToggle);
}
