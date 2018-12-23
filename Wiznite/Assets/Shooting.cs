using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public GameObject parent;
    public GameObject spawn;
    public int speed = 100;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            var a = Instantiate(parent, spawn.transform.position, spawn.transform.rotation);

            var mouse_pos = Input.mousePosition;
            var object_pos = Camera.main.WorldToScreenPoint(transform.position);
            mouse_pos.x = mouse_pos.x - object_pos.x;
            mouse_pos.y = mouse_pos.y - object_pos.y;
            var angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;

            a.GetComponent<Rigidbody>().velocity = Quaternion.Euler(new Vector3(0, -angle, 0)) * new Vector3(1, 0, 0) * speed;
        }
    }

}
