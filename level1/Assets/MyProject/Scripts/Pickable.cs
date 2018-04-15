using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour {
	float distance;
	public Transform guide;
	public GameObject item;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		distance = Vector3.Distance(item.transform.position, guide.transform.position);
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
}
