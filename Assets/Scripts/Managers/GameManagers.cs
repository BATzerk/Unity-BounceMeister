using UnityEngine;
using System.Collections;

public class GameManagers {
	// Managers
	private DataManager dataManager;
	private EventManager eventManager;
	// Getters
	public DataManager DataManager { get { return dataManager; } }
	public EventManager EventManager { get { return eventManager; } }
	// Properties
	private static bool isInitializing = false;



	// Constructor / Initialize
	private GameManagers () {
		dataManager = new DataManager ();
		eventManager = new EventManager ();
	}



	// Instance
	static private GameManagers instance;
	static public GameManagers Instance {
		get {
			if (instance==null) {
				// We're ALREADY initializing?? Uh-oh. Return null, or we'll be caught in an infinite loop of recursion!
				if (isInitializing) {
					Debug.LogError ("GameManagers access loop infinite recursion error! It's trying to access itself before it's done being initialized.");
					return null; // So the program doesn't freeze.
				}
				else {
					isInitializing = true;
					instance = new GameManagers();
				}
			}
			else {
				isInitializing = false; // Don't HAVE to update this value at all, but it's nice to for accuracy.
			}
			return instance;
		}
	}

}
