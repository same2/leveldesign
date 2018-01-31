﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Invector.CharacterController;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.AI;

public class chase : MonoBehaviour {
	public float trackingDistance = 10.0f; //How far ahead our gameObject can see 
	private float lookSpeed = 5.0f; //How fast our gameObject will rotate
	public float stopDistance = 0.8f; //How far from our Target will our gameObject stop
	//public float moveSpeed = 0.03f; //How fast our gameObject can move
	public float lookAngle = 180.0f; //The radius our gameObject is able to see
	private Animator anim; //We will need the Animator component attached to our gameObject // Use this for initialization
	public int damage = -10;
	private float timer;
	public float timeBetweenAttacks = 0.2f;
	public float movementspd = 5.0f;
	public float accel  = 8.0f;
	public float pSpeed = 0.01f;

	string state = "patrol";
	public GameObject[] waypoints;
	int currentWP = 0;
	float accuracyWP = 2.0f;
	private NavMeshAgent agent;

	void Start () {
		anim = GetComponent<Animator>();
		//target = GameObject.FindGameObjectWithTag ("Player").transform;
		agent = this.gameObject.GetComponent<NavMeshAgent> ();
	}
		
	void Update () {

		//Find the direction we wish to look at
		Vector3 direction = Invector.vGameController.instance.currentPlayer.transform.position - this.transform.position;
		//Find the angle of our gameObject
		float angle = Vector3.Angle(direction, this.transform.forward);
		if(state == "patrol" && waypoints.Length > 0)
		{
			anim.SetBool("isIdle",false);
			anim.SetBool("isWalking",true);
			if(Vector3.Distance(waypoints[currentWP].transform.position, transform.position) < accuracyWP)
			{
				currentWP = Random.Range(0,waypoints.Length);

			}

			//rotate towards waypoint
			agent.SetDestination(waypoints[currentWP].transform.position);
			//direction = waypoints[currentWP].transform.position - transform.position;
			//this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), lookSpeed * Time.deltaTime);
			//this.transform.Translate(0, 0, pSpeed);

		}
		//If the distance to our Target is less than our trackingDistance
		if (Vector3.Distance(Invector.vGameController.instance.currentPlayer.transform.position, this.transform.position) < trackingDistance && angle < lookAngle)
		{
			state = "pursuing";
			//Freeze the y axis to prevent our gameObject from moving vertically
			//direction.y = 0;
			//Turn our gameObject to look at our target
			//this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), lookSpeed * Time.deltaTime);
			//If our gameObject is farther than our designated stopDistance
			if (direction.magnitude > stopDistance) {
				//Move our gameObject towards our Target, until we reach our stopDistance
				//this.transform.Translate (0, 0, moveSpeed);
				agent.SetDestination(Invector.vGameController.instance.currentPlayer.transform.position);
				agent.speed = movementspd;
				agent.acceleration = accel;


				anim.SetBool ("isWalking", true);
				anim.SetBool ("isAttacking", false);
			} 

			else if (direction.magnitude < stopDistance) {

				anim.SetBool ("isAttacking", true);
				anim.SetBool ("isWalking", false);

				timer += Time.deltaTime;
				if(timer >= timeBetweenAttacks){
					hit();
				}
			} 
		}

		else {
			state = "patrol";
			anim.SetBool ("isWalking", true);
			anim.SetBool ("isAttacking", false);
		}

	}
		
	void hit(){
		// Reset the timer.
		timer = 0f;
		Invector.CharacterController.vThirdPersonController.instance.ChangeHealth (damage);
	}
}﻿