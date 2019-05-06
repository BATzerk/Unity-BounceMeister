using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerUtils {
    public static bool IsLayerInLayermask(int layer, LayerMask layermask) {
        return layermask == (layermask | (1 << layer));
    }
}

//      // Combine our bitmask arrays into single ones for easy access.
//      lm_B = 0;
//      lm_LRTB = 0;
//      string[] names_B = GetLayerMaskNames_B();
//      string[] names_LRTB = GetLayerMaskNames_LRTB();
//      foreach (string name in names_B) {
//          LayerMask mask = LayerMask.NameToLayer(name);
//          lm_B = lm_B | mask; // Add each one from the bottom-masks array to the single bottom-bitmask.
//      }
//      foreach (string name in names_LRTB) {
//          LayerMask mask = LayerMask.NameToLayer(name);
//          lm_LRTB = lm_LRTB | mask; // Add each one from the all-sides-masks array to the single all-sides-bitmask.
//          lm_B = lm_B | mask; // ALSO add this all-sides-mask to the bottom-masks array, too! (In case we make a mistake and forget to specify this mask in both arrays in the editor.)
//      }