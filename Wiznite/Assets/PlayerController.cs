using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed = 7f, rotSpeed = 7f; 
	private Vector3 moveInput;
	private Camera mainCamera;

	public ParticleSystem attack;
	// Use this for initialization
	void Start () {

		mainCamera = FindObjectOfType<Camera>();	
	}
	
	// Update is called once per frame
	void Update () {

		moveInput = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
		transform.position += moveInput * speed * Time.deltaTime;

		Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
		float rayLength;

		if(groundPlane.Raycast(cameraRay, out rayLength))
		{
			Vector3 pointToLook = cameraRay.GetPoint(rayLength);
			transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
		}

		if(Input.GetKeyDown(KeyCode.Mouse0))
		{
			attack.Emit(1);
		}
	}
}
