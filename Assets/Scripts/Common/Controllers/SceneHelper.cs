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
            GameManagers.Instance.DataManager.ResetLevelEnterValues();
        }
        // Open the scene.
        SceneManager.LoadScene(sceneName);
    }

    

	static public void OpenGameplayScene(int worldIndex, string levelKey) {
//		GameplaySnapshotController.SetWorldAndLevelToLoad (WorldIndex, levelKey);
        OpenGameplayScene(GameManagers.Instance.DataManager.GetLevelData(worldIndex, levelKey, true));
    }
    static public void OpenGameplayScene(LevelData ld) {
        GameManagers.Instance.DataManager.currLevelData = ld;
		OpenScene(SceneNames.Gameplay);
	}




}
