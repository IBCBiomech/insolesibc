using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularAcceleration : MonoBehaviour
{
    Rigidbody rb;
    float min = -2.0f;
    float max = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float xTorque = Random.Range(min, max);
        float yTorque = Random.Range(min, max);
        float zTorque = Random.Range(min, max);
        rb.AddTorque(new Vector3(xTorque, yTorque, zTorque));
    }
}
