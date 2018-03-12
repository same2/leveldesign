using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour {

	float throwForce = 600;
	public bool canHold = true;
	public GameObject item;
	public GameObject tempParent;
	public Transform guide;
	public bool isHolding = false;
	float distance;
	Renderer rend;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

		distance = Vector3.Distance(item.transform.position, guide.transform.position);

		if (isHolding==true)
		{
			item.GetComponent<Rigidbody>().useGravity = false;
			item.GetComponent<Rigidbody>().detectCollisions = true;
			item.GetComponent<Rigidbody>().isKinematic = false;
			item.transform.parent = tempParent.transform;
			item.transform.position = guide.transform.position;
			if (Input.GetMouseButtonDown(1))
			{
				item.GetComponent<Rigidbody>().AddForce(guide.transform.forward * throwForce);
				isHolding = false;
			}
		}
		else
		{
			item.GetComponent<Rigidbody>().useGravity = true;
			item.GetComponent<Rigidbody>().isKinematic = false;
			item.transform.parent = null;
		}
	}

	void OnMouseDown()
	{
		if (distance <= 1.5f)
		{
			isHolding = true;
			//guide.transform.position = item.transform.position;
		}
	}
	void OnMouseUp()
	{
		isHolding = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		isHolding = false;
	}
		
	void OnMouseExit()
	{
		Renderer rend = GetComponent<Renderer>();
		rend.material.color = Color.white;
	}

	void OnMouseOver()
	{
		if (distance <= 1.5f) {
			Renderer rend = GetComponent<Renderer> ();
			rend.material.color = Color.yellow;
		} else {
			Renderer rend = GetComponent<Renderer> ();
			rend.material.color = Color.white;
		}
	}

}﻿