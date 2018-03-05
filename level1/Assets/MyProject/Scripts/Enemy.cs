using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Invector.CharacterController;
using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour {
	//public Transform player;
	public float timeBetweenAttacks = 0.2f;
	float distancefrom_player; 
	public float look_range = 13.0f;
	public float agro_range= 8.0f;
	public float move_speed= 5.0f;
	public float damping = 6.0f;
	public float touching = 1.35f;
	public int damage = -10;
	public float timer;
	Renderer rend;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer>();
		rend.material.color = Color.green;
	}

	// Update is called once per frame
	void Update () 
	{
		distancefrom_player = Vector3.Distance (Invector.vGameController.instance.currentPlayer.transform.position, transform.position);

		if (distancefrom_player < look_range ) {
			Renderer rend = GetComponent<Renderer>();
			rend.material.color = Color.yellow;
			transform.LookAt(Invector.vGameController.instance.currentPlayer.transform);
		}

		if (distancefrom_player < agro_range) {
			Renderer rend = GetComponent<Renderer>();
			rend.material.color = Color.red;
			attack();
		}

		if (distancefrom_player < touching) {
			// Add the time since Update was last called to the timer.
			timer += Time.deltaTime;

			// If the timer exceeds the time between attacks, the player is in range and this enemy is alive...
			if(timer >= timeBetweenAttacks)
			{
				// ... attack.
				damaged ();
			}
		}

	}

	void lookAt()
	{
		Quaternion rotation = Quaternion.LookRotation (Invector.vGameController.instance.currentPlayer.transform.position - transform.position);
		transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * damping);
	}

	void attack()
	{
		transform.Translate (Vector3.forward * move_speed * Time.deltaTime);
	}

	void damaged(){
		// Reset the timer.
		timer = 0f;
		Invector.CharacterController.vThirdPersonController.instance.ChangeHealth (damage);
	}
}﻿