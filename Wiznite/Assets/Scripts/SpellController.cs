using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellController : MonoBehaviour {

	public Vector3 Velocity;
	public float speed;

	// Use this for initialization
	void Start () {
		Velocity *= speed;
	}
	
	// Update is called once per frame
	void Update () {

		transform.position += Velocity * Time.deltaTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			other.transform.GetComponent<PlayerHealth>().TakeDamage(25f);
			Rigidbody rb = other.GetComponent<Rigidbody>();

			rb.AddForce(transform.forward * 600f);
			Debug.Log(rb.transform);
		}
	}
}
