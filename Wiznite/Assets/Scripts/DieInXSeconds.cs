using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieInXSeconds : MonoBehaviour
{
    // Use this for initialization
    public float lifetime;

    // Update is called once per frame
    void Update()
    {
        lifetime -= Time.deltaTime;

        if (lifetime < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
