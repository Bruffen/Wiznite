using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Movement : MonoBehaviour
{
    public int speed = 5;
    private Rigidbody _rigidbody;
    // Use this for initialization
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");

        var mouse_pos = Input.mousePosition;
        var object_pos = Camera.main.WorldToScreenPoint(transform.position);
        mouse_pos.x = mouse_pos.x - object_pos.x;
        mouse_pos.y = mouse_pos.y - object_pos.y;
        var angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0));

        _rigidbody.velocity = new Vector3(hor, 0, ver) * speed;
    }
}
