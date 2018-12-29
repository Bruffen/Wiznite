using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithTime : MonoBehaviour
{
    public float min, max, rate;

    private Vector3 size;

    // Use this for initialization
    void Start()
    {
        size = new Vector3(max, this.transform.localScale.y, max);

        this.transform.localScale = size;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (size.x > min)
        {
            size = this.transform.localScale;

            size.x -= Time.deltaTime / rate;
            size.z -= Time.deltaTime / rate;

            this.transform.localScale = size;
        }
    }
}
