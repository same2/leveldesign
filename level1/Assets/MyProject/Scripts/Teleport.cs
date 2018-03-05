using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;


public class Teleport : MonoBehaviour {

	public GameObject nextTeleport; //attach a gameobject where you would like to teleport to

	public void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player")) {
			var gc =FindObjectOfType<vGameController>();
			gc.spawnPoint = nextTeleport.transform;
			Debug.Log("Spawn Point = " + gc.spawnPoint);

			if (Invector.CharacterController.vThirdPersonController.instance != null)  {
				Invector.CharacterController.vThirdPersonController.instance.gameObject.transform.position = nextTeleport.transform.position;
				Invector.CharacterController.vThirdPersonController.instance.gameObject.transform.rotation = nextTeleport.transform.rotation;
				Invector.CharacterController.vThirdPersonController.instance.GetComponent<Rigidbody> ().velocity = Vector3.zero;

			}
		}
	}
}

