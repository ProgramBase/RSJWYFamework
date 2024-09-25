using System;
using System.Collections;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Default.Manager;
using UnityEngine;

public class Test_server : MonoBehaviour
{
    private DefaultTcpServerController col=new ();
    // Start is called before the first frame update
    private void OnEnable()
    {
        col.Init();
        col.InitServer();
    }

    private void OnDisable()
    {
        col?.Close();
    }

}
