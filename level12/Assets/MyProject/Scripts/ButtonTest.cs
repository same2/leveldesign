using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		LevelManager.Instance.LoadLevel (3);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ButtonFunction(){
		Debug.Log("Button Fired");
	}


}
