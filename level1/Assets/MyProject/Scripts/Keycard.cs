using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.CharacterController;

public class Keycard : MonoBehaviour {
	public GameObject keycard;
	private bool playernexttokey = false;
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.E) && playernexttokey == true) {
			keycard.SetActive (false);

		}
		if (Input.GetKeyDown (KeyCode.E) && playernexttokey == true) {
			vHUDController.instance.ShowText("Security keycard Aquired");
		}
	}

	void OnTriggerEnter(Collider collider){
		if (collider.tag == "Player") {
			playernexttokey = true;
			var tpInput = collider.GetComponent<vThirdPersonInput>();
		}
	}

	void OnTriggerExit(Collider collider){
		if (collider.tag == "Player") {
			playernexttokey = false;

		}
	}

	void OnGUI(){
		if (playernexttokey) {
			GUI.Box (new Rect (500, 400, 200, 25), "Press E to pickup");
		}
	}
}
