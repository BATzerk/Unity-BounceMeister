using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneHelper {
    // Getters (Public)
    public static bool IsGameplayScene() { return SceneManager.GetActiveScene().name == SceneNames.Gameplay; }


    // ----------------------------------------------------------------
    //  Doers
    // ----------------------------------------------------------------
    static public void ReloadScene() { OpenScene(SceneManager.GetActiveScene().name); }
    static public void OpenScene(string sceneName) {
        // Opening NON-Gameplay scene? Reset static values!
        if (sceneName != SceneNames.Gameplay) {
            GameManagers.Instance.DataManager.ResetRoomEnterValues();
        }
        // Open the scene.
        SceneManager.LoadScene(sceneName);
    }

    
    static public void OpenGameplayScene(RoomClusterData clust) {
        // Default to the first Room in the cluster.
        RoomData room = clust.rooms[0];
        // Did we last play this cluster? Start in the last room we were in!
        string lastPlayedRoomKey = SaveStorage.GetString(SaveKeys.LastPlayedRoomKey(clust.WorldIndex));
        for (int i=0; i<clust.rooms.Count; i++) {
            if (lastPlayedRoomKey == clust.rooms[i].RoomKey) {
                room = clust.rooms[i];
                break;
            }
        }
        OpenGameplayScene(room);
    }
	static public void OpenGameplayScene(int worldIndex, string roomKey) {
//		GameplaySnapshotController.SetWorldAndRoomToLoad (WorldIndex, roomKey);
        OpenGameplayScene(GameManagers.Instance.DataManager.GetRoomData(worldIndex, roomKey, true));
    }
    static public void OpenGameplayScene(RoomData rd) {
        GameManagers.Instance.DataManager.currRoomData = rd;
		OpenScene(SceneNames.Gameplay);
	}




}
