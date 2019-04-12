public enum PlayerTypes {
    Undefined,
    
    Plunga,
    Slippa,
    Jetta,
}


static public class PlayerTypeHelper {

    static public PlayerTypes LoadLastPlayedType() {
        string typeStr = SaveStorage.GetString(SaveKeys.LastPlayedPlayerType, PlayerTypes.Plunga.ToString());
        return TypeFromString(typeStr);
    }
    public static void SaveLastPlayedType(PlayerTypes _type) {
        SaveStorage.SetString(SaveKeys.LastPlayedPlayerType, _type.ToString());
    }
    
    public static PlayerTypes TypeFromString(string str) {
        switch (str) {
            case "Jetta":  return PlayerTypes.Jetta;
            case "Plunga": return PlayerTypes.Plunga;
            case "Slippa": return PlayerTypes.Slippa;
            default: return PlayerTypes.Undefined; // Oops.
        }
    }
}