﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slave : MonoBehaviour {

	public float speed = 7f;
	Animator animator;
	Vector3 oldPosition;
	GameObject attack;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void GoToPosition(Vector3 pos)
	{
		oldPosition = transform.position;
		transform.position += (pos * speed) * Time.deltaTime;
		MakeAnimation();
	}

	public void MakeAnimation()
	{
		Vector3 direction = transform.position - oldPosition;
		float forwardTest = Vector3.Dot(direction.normalized, transform.forward.normalized);
		float sideTest = Vector3.Dot(direction.normalized, Vector3.Cross(transform.forward, transform.up).normalized);

		//animations
		if (forwardTest != 0 || sideTest != 0)
		{
			if (forwardTest != 0)
			{
				animator.SetBool("Idle", false);
				if (forwardTest > 0)
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
		else if (forwardTest == 0 && sideTest == 0)
			animator.SetBool("Idle", true);
	}

	public void MakeAttack()
	{
		animator.SetBool("Attacking", true);
	}

	private void Fire()
	{
		GameObject attack1 = Instantiate(attack, this.transform.position, Quaternion.identity);
		attack1.GetComponent<SpellController>().Velocity = this.transform.forward;
	}
}
