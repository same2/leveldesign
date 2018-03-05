/// <summary>
/// This scripts contains the out of class hook that allows other objects to access the load functionality of the level manager singleton class
/// This script can be assigned to any object that needs to load a level via the level manager without a direct reference to the level manager
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelHook : MonoBehaviour {

	public void LoadLevel(int ID){
		LevelManager.Instance.LoadLevel (ID);
	}

	public void LoadLevel(string name){
		LevelManager.Instance.LoadLevel (name);
	}
	public void ExitGame() {
		Application.Quit();
	}
}
