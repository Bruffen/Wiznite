﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {

	public float Health = 100;
	private float maxHealth;
	public Image HealthBar;
	private Quaternion canvasRotation;
	// Use this for initialization
	void Start () {
		maxHealth = Health;
		canvasRotation = transform.Find("Canvas").rotation;
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.F))
			TakeDamage(25);
		transform.Find("Canvas").rotation = canvasRotation;
		Debug.Log(maxHealth);
	}

	public void TakeDamage(float damage)
	{
		Health -= damage;
		HealthBar.fillAmount = Health / 100f;

		if (Health <= 0)
		{
			Destroy(this.gameObject);
		}
	}
}