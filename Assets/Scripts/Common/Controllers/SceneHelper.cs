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
    static public void OpenScene(string sceneName) { SceneManager.LoadScene(sceneName); }


}
