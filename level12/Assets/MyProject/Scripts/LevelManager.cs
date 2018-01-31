/// <summary>
/// This class contains a singletone structure for a level manager.
/// To use create a prefab object with the level manager script attached to it and place it in the initial scene
/// This object will be carried between scenes and allow level data to be saved between scenes. If any data needs to be passed between
/// scenes it is possible to store it in this class via a struct in order to ensure that the data will not be lost between transitions
/// 
/// To use:
/// Access this objetc through it's static accessor via LevelManager.Instance, a reference directly to this objetc should avoid to ensure that all code works properly
/// because this object is passed between scenes any scene dependant references to this object will be lost upon reloading the scene
/// 
/// NOTE: For debugging purposes it is possible to put a new copy inside a scene that is being tested but will not be the initial scene. Keep in mind that the first level manager
/// assigned from the scene that is loaded the first time the game is run will override any additional level managers that are present within the game in other scenes upon scene 
/// transitions
/// </summary>


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	//We create a variable to assign an instance of the singleton manager
	private static LevelManager instance;

	//Create a get only static accessor that allows us to access the singleton from anywhere in code without a direct reference
	//NOTE: Access it via LevelManager.Instance
	public static LevelManager Instance {
		get{ return instance; }
	}

	//On awake assign the level manager
	//NOTE: If another script needs to access the level manager make sure that it is set to run in a message that occurs after scene initialization
	//		for example Update or Start. If you attempt to access the Level Manager in Awake or OnEnable it is possible that it will be null because
	//		it may not be assigned yet
	void Awake(){
		//If we already have an instance of the Level Manager in scene (Usually because one caried over from a previous scene or a duplicate was made)
		//then destroy the non-instance copy
		if (instance != null) {
			Destroy (this.gameObject);
			return;
		}

		//Set this object to not be destroyed upon loading so that it won't be destroyed between scenes
		//and set the current static instance to this object
		DontDestroyOnLoad (this.gameObject);
		instance = this;
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		//Cursor.lockState = CursorLockMode.Confined;
		//Commment this out in the final build
		Cursor.visible = true;
	}

	//A function that accesses the scene manager to load a level with the given ID
	public void LoadLevel(int ID){
		SceneManager.LoadScene (ID);

	}

	//A function that accesses the scene manager to load a level with the given string
	public void LoadLevel(string sceneName){
		SceneManager.LoadScene (sceneName);

	}
}
