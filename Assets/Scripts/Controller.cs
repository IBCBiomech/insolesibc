using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Controller : MonoBehaviour
{
    public TextMeshProUGUI output;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnScan()
    {
        Debug.Log("Scan Click");
    }
    public void OnConnect()
    {
        Debug.Log("Connect Click");
    }
    public void OnStart()
    {
        Debug.Log("Start Click");
    }
    public void OnStop()
    {
        Debug.Log("Stop Click");
    }
    private void ChangeOutput(string text)
    {
        output.text = text;
    }
}
