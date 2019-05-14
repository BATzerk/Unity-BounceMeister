public enum PlayerTypes {
    Undefined,
    
    Any, // special type: Wild card. Any player can collect Snack like this.
    //Every, // special type: EVERY 
    
    Dilata,
    Flatline,
    Jetta,
    Jumpa,
    Limo,
    Plunga,
    Slippa,
}


static public class PlayerTypeHelper {
    static public readonly PlayerTypes[] AllTypes = {
        PlayerTypes.Any,
        PlayerTypes.Dilata,
        PlayerTypes.Flatline,
        PlayerTypes.Jetta,
        PlayerTypes.Jumpa,
        PlayerTypes.Limo,
        PlayerTypes.Plunga,
        PlayerTypes.Slippa,
    };

    static public PlayerTypes LoadLastPlayedType() {
        string typeStr = SaveStorage.GetString(SaveKeys.LastPlayedPlayerType, PlayerTypes.Plunga.ToString());
        return TypeFromString(typeStr);
    }
    public static void SaveLastPlayedType(PlayerTypes _type) {
        SaveStorage.SetString(SaveKeys.LastPlayedPlayerType, _type.ToString());
    }
    
    public static PlayerTypes TypeFromString(string str) {
        switch (str) {
            case "Any":      return PlayerTypes.Any;
            case "Dilata":   return PlayerTypes.Dilata;
            case "Flatline": return PlayerTypes.Flatline;
            case "Jetta":    return PlayerTypes.Jetta;
            case "Jumpa":    return PlayerTypes.Jumpa;
            case "Limo":     return PlayerTypes.Limo;
            case "Plunga":   return PlayerTypes.Plunga;
            case "Slippa":   return PlayerTypes.Slippa;
            default: return PlayerTypes.Undefined; // Oops.
        }
    }
}