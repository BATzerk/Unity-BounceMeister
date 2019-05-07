using UnityEngine;

public struct RoomAddress {
    // Statics
    static public readonly RoomAddress undefined = new RoomAddress(-1, -1, "undefined");
    //static public readonly RoomAddress zero = new RoomAddress(0,0,"undefined");
    // Properties
    public int world;
    public int clust; // -1 is no Cluster.
    public string room; // everything we use to reference this room! Including the room's file name (minus the .txt suffix).

    public RoomAddress(int world, int clust) {
        this.world = world;
        this.clust = clust;
        this.room = "undefined";
    }
    public RoomAddress(int world, int clust, string room) {
        this.world = world;
        this.clust = clust;
        this.room = room;
    }

    //public RoomAddress NextLevel { get { return new RoomAddress(mode, collection, pack, level+1); } }
    //public RoomAddress PreviousLevel { get { return new RoomAddress(mode, collection, pack, level-1); } }
    //public RoomAddress NoNegatives() {
    //    return new RoomAddress(
    //        Mathf.Max(0, mode),
    //        Mathf.Max(0, collection),
    //        Mathf.Max(0, pack),
    //        Mathf.Max(0, level));
    //}

    //public override string ToString() { return "World " + world + ", Clust " + clust + ", " + room; }
    public string ToStringClust() { return world + "," + clust; }
    public override string ToString() { return world + "," + clust + "," + room; }
    static public RoomAddress FromString(string str) {
        string[] array = str.Split(',');
        if (array.Length >= 4) {
            return new RoomAddress(int.Parse(array[0]), int.Parse(array[1]), array[2]);
        }
        return RoomAddress.undefined; // Hmm.
    }


    //public static RoomAddress operator + (RoomAddress a, RoomAddress b) {
    //    return new RoomAddress(
    //        a.mode+b.mode,
    //        a.collection+b.collection,
    //        a.pack+b.pack,
    //        a.level+b.level);
    //}

    public override bool Equals(object obj) { return base.Equals (obj); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).
    public override int GetHashCode() { return base.GetHashCode(); } // NOTE: Just added these to appease compiler warnings. I don't suggest their usage (because idk what they even do).

    public static bool operator == (RoomAddress a, RoomAddress b) {
        return a.Equals(b);
    }
    public static bool operator != (RoomAddress a, RoomAddress b) {
        return !a.Equals(b);
    }
}
