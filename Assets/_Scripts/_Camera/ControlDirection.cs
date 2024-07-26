using System;
using UnityEngine;

public class ControlDirection : MonoBehaviour
{
    void Start()
    {
        if (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            if (child != null)
            {
                child.localPosition = new Vector3(0, 0, -0.04f);
                // child.localScale = new Vector3(0.02f, 0.002f, 0.015f);
            }
        }
    }

    void Update()
    {
        if (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            if (child != null)
            {
                Vector3 parentRotation = transform.rotation.eulerAngles;
                child.rotation = Quaternion.Euler(-parentRotation.x + 20, parentRotation.y - 180, 0f);
            }
        }
    }

    // Quaternion newRotation = Quaternion.Euler(-30, 180, 180); 
    // void Start()
    // {
    //     transform.GetChild(0).localPosition = new Vector3(0, 0, -0.04f);
    //     transform.GetChild(0).rotation = newRotation;
    //     transform.GetChild(0).localScale = new Vector3(0.02f, 0.002f, 0.015f);
    // }
    
}
