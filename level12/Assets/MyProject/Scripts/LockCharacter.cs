using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCharacter : MonoBehaviour {

	public Vector3 resetPosition;
	public void Destroy(){
	    GameObject player = Invector.CharacterController.vThirdPersonController.instance.gameObject;
		if (player != null) {
	        //Invector.CharacterController.vThirdPersonController.instance.gameObject;
			Invector.vGameController.instance.ResetScene();
		}
	}
	public void Lock(){
		if (Invector.CharacterController.vThirdPersonController.instance != null) {
			Invector.CharacterController.vThirdPersonController.instance.gameObject.SetActive (false);

		}
	}

	public void ResetCharacter(){
		if (Invector.CharacterController.vThirdPersonController.instance != null) {
			Invector.CharacterController.vThirdPersonController.instance.gameObject.SetActive (true);
			Invector.CharacterController.vThirdPersonController.instance.gameObject.transform.position = Invector.vGameController.instance.spawnPoint.position;
			Invector.CharacterController.vThirdPersonController.instance.gameObject.transform.rotation = Invector.vGameController.instance.spawnPoint.rotation;
			Invector.CharacterController.vThirdPersonController.instance.GetComponent<Rigidbody> ().velocity = Vector3.zero;

		}
	}
}
