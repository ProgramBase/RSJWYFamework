using System.Collections;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Main;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Main.Instance.GetHashCode();
        GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
