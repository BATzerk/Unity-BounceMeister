public enum PlayerTypes {
    Undefined,
    
    Any, // special type: Wild card. Any player can collect Snack like this.
    //Every, // special type: EVERY 
    
    Clinga,
    Dilata,
    Flatline,
    Flippa,
    Jetta,
    Jumpa,
    Limo,
    Plunga,
    Slippa,
    Warpa,
}


static public class PlayerTypeHelper {
    static public readonly PlayerTypes[] AllTypes = {
        PlayerTypes.Any,
        PlayerTypes.Clinga,
        PlayerTypes.Dilata,
        PlayerTypes.Flatline,
        PlayerTypes.Flippa,
        PlayerTypes.Jetta,
        PlayerTypes.Jumpa,
        PlayerTypes.Limo,
        PlayerTypes.Plunga,
        PlayerTypes.Slippa,
        PlayerTypes.Warpa,
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
            case "Clinga":   return PlayerTypes.Clinga;
            case "Dilata":   return PlayerTypes.Dilata;
            case "Flatline": return PlayerTypes.Flatline;
            case "Flippa":   return PlayerTypes.Flippa;
            case "Jetta":    return PlayerTypes.Jetta;
            case "Jumpa":    return PlayerTypes.Jumpa;
            case "Limo":     return PlayerTypes.Limo;
            case "Plunga":   return PlayerTypes.Plunga;
            case "Slippa":   return PlayerTypes.Slippa;
            case "Warpa":    return PlayerTypes.Warpa;
            default: return PlayerTypes.Undefined; // Oops.
        }
    }
}