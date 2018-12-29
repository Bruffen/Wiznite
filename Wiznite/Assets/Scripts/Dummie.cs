using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummie : MonoBehaviour {

	public LayerMask layer;

	private bool isknockback = false;
	private float knockBackForce = 1000f;
	private float knockBackTime = 1f;
	private float knockBackCounter = 0.0f;
	Rigidbody impactTarget;
	Vector3 impact;

	// Use this for initialization
	void Start () {
		impactTarget = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

		//Check if is grounded or not
		if (isGrounded())
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
		else
		{
			GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;
			FallForce();
		}

		if (Time.time < knockBackCounter)
		{
			isknockback = true;
			impactTarget.AddForce(impact, ForceMode.VelocityChange);
		}
		else
			isknockback = false;
	}

	private bool isGrounded()
	{
		return Physics.Linecast(transform.position, transform.position + Vector3.down, layer);
	}

	void FallForce()
	{
		float fallY;
		float airVelocity = -9.8f;
		Vector3 gravity = Physics.gravity * Time.deltaTime;
		airVelocity += gravity.y;
		fallY = airVelocity * Time.deltaTime;

		transform.position = Vector3.MoveTowards(transform.position, transform.position - new Vector3(0f, fallY, 0f), Time.deltaTime * airVelocity);
	}

	public void KnockBack(Vector3 direction)
	{
		impact = new Vector3(direction.x, 0.0f, direction.z) * knockBackForce * Time.deltaTime;
		knockBackCounter = Time.time + 0.25f;
	}
}
