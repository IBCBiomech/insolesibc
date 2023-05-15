using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularVelocity : MonoBehaviour
{
    float xAngularVelocity = 3;
    float yAngularVelocity = 2.5f;
    float zAngularVelocity = 3.5f;

    
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.angularVelocity = new Vector3(xAngularVelocity, yAngularVelocity, zAngularVelocity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
