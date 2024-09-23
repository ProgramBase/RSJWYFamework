using System;
using System.Collections;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Default.Manager;
using UnityEngine;

public class Test_server : MonoBehaviour
{
    private DefaultTcpServerController col=new ();
    // Start is called before the first frame update
    private void Awake()
    {
        col.Init();
        col.InitServer();
    }

    void Start()
    {
        
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
