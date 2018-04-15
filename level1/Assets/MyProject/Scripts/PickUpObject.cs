using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpObject : MonoBehaviour {
	/*

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
			item.GetComponent<Rigidbody>().freezeRotation = true;
			item.GetComponent<Rigidbody>().useGravity = false;
			item.GetComponent<Rigidbody>().detectCollisions = true;
			item.GetComponent<Rigidbody>().isKinematic = false;
			item.transform.parent = tempParent.transform;
			item.transform.position = guide.transform.position;
			if (Input.GetMouseButtonDown(1))
			{
				item.GetComponent<Rigidbody>().freezeRotation = false;
				item.GetComponent<Rigidbody>().AddForce(guide.transform.forward * throwForce);
				isHolding = false;

			}
		}
		else
		{
			item.GetComponent<Rigidbody>().freezeRotation = false;
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
		*/

	float throwForce = 600;
	public Transform guide;
	GameObject mainCamera;
	bool carrying;
	GameObject carriedObject;
	public float distance;
	public float smooth;
	float maximumReachDistance = 3.0f;
	// Use this for initialization

	void Start () {
		mainCamera = GameObject.FindWithTag("MainCamera");
	}

	// Update is called once per frame
	void Update () {
		if(carrying) {
			carry(carriedObject);
			checkDrop();
			if (Input.GetMouseButtonDown (1)) {
				throwObject ();
			}
			//rotateObject();
		} else {
			pickup();
		}
	}

	void rotateObject() {
		carriedObject.transform.Rotate(5,10,15);
	}

	void carry(GameObject o) {
		o.transform.position = Vector3.Lerp (o.transform.position, mainCamera.transform.position + mainCamera.transform.forward * distance, Time.deltaTime * smooth);
		o.transform.rotation = Quaternion.identity;
		//o.GetComponent<Rigidbody>().isKinematic = true;﻿
	}

	void pickup() {
		if(Input.GetKeyDown (KeyCode.E)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				Pickable p = hit.collider.GetComponent<Pickable>();
				if(p!=null && Physics.Raycast(ray, out hit, maximumReachDistance)) {
					carrying = true;
					carriedObject = p.gameObject;
					//p.gameObject.GetComponent<Rigidbody>().isKinematic = true;
					p.gameObject.GetComponent<Rigidbody>().useGravity = false;
				}
			}
		}
	}

	void checkDrop() {
		if(Input.GetKeyDown (KeyCode.E)) {
			dropObject();
		}
	}

	void dropObject() {
		carrying = false;
		//carriedObject.gameObject.GetComponent<Rigidbody>().isKinematic = false;
		carriedObject.gameObject.GetComponent<Rigidbody>().useGravity = true;
		carriedObject = null;
	}

	void throwObject(){
		carrying = false;
		//carriedObject.gameObject.GetComponent<Rigidbody>().freezeRotation = false;
		//carriedObject.gameObject.GetComponent<Rigidbody>().isKinematic = false;
		carriedObject.gameObject.GetComponent<Rigidbody>().useGravity = true;
		carriedObject.gameObject.GetComponent<Rigidbody>().AddForce(guide.transform.forward * throwForce);
		carriedObject = null;
	}


}﻿