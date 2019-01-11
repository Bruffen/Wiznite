using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellController : MonoBehaviour
{

    public Vector3 Velocity;
    public float speed;
    private float lifetime = 5f;

    // Use this for initialization
    void Start()
    {
        Velocity *= speed;
        transform.forward = Velocity;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Velocity * Time.deltaTime;
        lifetime -= Time.deltaTime;

        if (lifetime < 0f)
        {
            Destroy(this.gameObject);
        }
    }
}
