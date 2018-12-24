using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed = 7f, rotSpeed = 7f; 
	private Vector3 moveInput;
	private Camera mainCamera;
	private Animator animator;

	public GameObject attack;
	private Vector3 oldPosition;
	// Use this for initialization
	void Start () {

		mainCamera = FindObjectOfType<Camera>();
		animator = GetComponent<Animator>();
		animator.SetBool("Idle", true);
		oldPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
		oldPosition = transform.position;
		//movement
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		moveInput = new Vector3(h, 0.0f, v) * speed;
		transform.position += moveInput * Time.deltaTime;
		
		//rotation
		Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
		float rayLength;

		if(groundPlane.Raycast(cameraRay, out rayLength))
		{
			Vector3 pointToLook = cameraRay.GetPoint(rayLength);
			transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
		}

		Vector3 direction = transform.position - oldPosition;
		float fowardTest = Vector3.Dot(direction.normalized, transform.forward.normalized);
		float sideTest = Vector3.Dot(direction.normalized, Vector3.Cross(transform.forward, transform.up).normalized);

		//animations
		if (fowardTest != 0 || sideTest != 0)
		{
			if (fowardTest != 0)
			{
				animator.SetBool("Idle", false);
				if (fowardTest > 0)
				{
					animator.SetBool("Forward", true);
					animator.SetBool("Back", false);
				}
				else
				{
					animator.SetBool("Forward", false);
					animator.SetBool("Back", true);
				}
			}
			if (sideTest != 0)
			{
				animator.SetBool("Idle", false);
				if (sideTest < 0)
				{
					animator.SetBool("Right", true);
					animator.SetBool("Left", false);
				}
				else
				{
					animator.SetBool("Right", false);
					animator.SetBool("Left", true);
				}
			}
		}
		else if(h == 0 && sideTest == 0)
			animator.SetBool("Idle", true);

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			animator.SetBool("Attacking",true);
			if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attacking")){
				WaitForAnimation();
			}
			GameObject attack1 = Instantiate(attack, this.transform.position, Quaternion.identity);
			attack1.GetComponent<SpellController>().Velocity = this.transform.forward;
		}
		else
			animator.SetBool("Attacking", false);
	}

	private IEnumerator WaitForAnimation()
	{
		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
	}


	/*private void Animations(Vector3 movV, Vector3 movH)
	{
		//animations
		if (h != 0 || v != 0)
		{
			animator.SetBool("Idle", false);
			//Debug.Log("Idle FALSE");
			if (h > 0 && v == 0)
			{
				//Debug.Log("Right");
				animator.SetBool("Right", true);
				animator.SetBool("Left", false);
			}
			else if ((h < 0 && v == 0))
			{
				//Debug.Log("Left");
				animator.SetBool("Right", false);
				animator.SetBool("Left", true);
			}
			else if ((v > 0 && h == 0))
			{
				//Debug.Log("Forward");
				animator.SetBool("Forward", true);
				animator.SetBool("Back", false);
			}
			else if ((v < 0 && h == 0))
			{
				//Debug.Log("Back");
				animator.SetBool("Forward", false);
				animator.SetBool("Back", true);
			}
		}*/
}
