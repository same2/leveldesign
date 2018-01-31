using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;


public class Teleport : MonoBehaviour {
	
	public void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player")) {
			var gc =FindObjectOfType<vGameController>();
			gc.spawnPoint = this.transform;
			Debug.Log("Spawn Point = " + gc.spawnPoint);
		}
	}
}

