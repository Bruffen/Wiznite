using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

	public float Health = 100;
	private float maxHealth;
	public Image HealthBar;
	private Quaternion canvasRotation;
	private Animator animator;

	// Use this for initialization
	void Start () {
		maxHealth = Health;
		canvasRotation = transform.Find("Canvas").rotation;
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

		transform.Find("Canvas").rotation = canvasRotation;
	}

	public void TakeDamage(float damage)
	{
		Health -= damage;
		HealthBar.fillAmount = Health / 100f;
		//GetComponent<PlayerController>().KnockBack(direction);

		if (Health <= 0)
		{
			animator.SetTrigger("Die");
			Destroy(this.gameObject);
		}
	}
}
