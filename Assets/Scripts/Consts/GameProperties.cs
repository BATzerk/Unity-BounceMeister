using UnityEngine;
using System.Collections;

public class GameProperties : MonoBehaviour {
//	private static readonly string[] player0Names = { "Tobias Fünke", "Blue Man Group Member #3", "Aggressive Blue Dot", "Sapphire Smasher", "Not Green", "Phthalocyanine Punisher" };//"\"I Blue Myself\"", 
//	private static readonly string[] player1Names = { "Señor Verde", "The Green Slammer", "Verdant Spasm", "Christian Kale", "Lettuce Slave", "Kate Moss", "1 Rupee" };


	// Constants
	public const float UnitSize = 1f; // in Unity units

	public const int NUM_WORLDS = 2;

	public static string GetFirstLevelName(int worldIndex) {
		switch(worldIndex) {
		default: return "Level0";
		}
	}
	public static string GetLastLevelName(int worldIndex) {
		switch(worldIndex) {
		default: return "WorldEnd";
		}
	}



}


