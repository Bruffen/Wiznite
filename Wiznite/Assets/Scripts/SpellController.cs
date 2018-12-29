﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellController : MonoBehaviour {

	public Vector3 Velocity;
	public float speed;

	// Use this for initialization
	void Start () {
		Velocity *= speed;
		transform.forward = Velocity;
	}
	
	// Update is called once per frame
	void Update () {

		transform.position += Velocity * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			Vector3 direction = other.transform.position - this.transform.position;
			direction = direction.normalized;

			//other.transform.GetComponent<PlayerController>().KnockBack(direction);
			other.transform.GetComponent<Dummie>().KnockBack(direction);
			Destroy(this.gameObject);
		}
	}
}
