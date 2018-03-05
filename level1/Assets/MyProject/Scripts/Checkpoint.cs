using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

   

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetSpawnPoint()
    {
        Invector.vGameController.instance.spawnPoint.position = this.transform.position;
        Invector.vGameController.instance.spawnPoint.rotation = this.transform.rotation;
    }
}
