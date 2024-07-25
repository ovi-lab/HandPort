using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDirection : MonoBehaviour
{
    Quaternion newRotation = Quaternion.Euler(-30, 180, 180); 
    void Start()
    {
        transform.GetChild(0).localPosition = new Vector3(0, 0, -0.04f);
        transform.GetChild(0).rotation = newRotation;
        transform.GetChild(0).localScale = new Vector3(0.02f, 0.002f, 0.015f);
    }
    
}
