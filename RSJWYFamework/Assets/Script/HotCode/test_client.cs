using System;
using System.Collections;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Default.Manager;
using UnityEngine;

public class test_client : MonoBehaviour
{
    // Start is called before the first frame update
    
    private DefaultTcpClientController col=new ();
    void Start()
    {
        col.Init();
        col.InitTCPClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnApplicationQuit()
    {
        col?.Close();
    }
}
