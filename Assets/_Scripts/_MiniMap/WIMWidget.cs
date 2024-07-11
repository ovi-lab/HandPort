using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WIMWidget : MonoBehaviour
{
    [SerializeField] private int anchorType;
    [SerializeField] private Transform[] anchorPoint;

    private Transform display;
    private bool isInitialized = false;

    private void Start()
    {
        InitializeDisplay();
        if (anchorPoint.Length > 0)
        {
            SetDisplayParent(anchorPoint[0]);
        }
        isInitialized = true;
    }

    private void OnValidate()
    {
        if (!isInitialized) return;

        InitializeDisplay();
        if (anchorPoint.Length > anchorType && anchorPoint[anchorType] != null)
        {
            SetDisplayParent(anchorPoint[anchorType]);
        }
        else
        {
            Debug.LogWarning("Invalid anchorType or anchorPoint.");
        }
    }

    private void InitializeDisplay()
    {
        if (display == null)
        {
            if (transform.childCount > 0)
            {
                display = transform.GetChild(0);
            }
            else
            {
                Debug.LogWarning("No child objects found in the Transform.");
            }
        }
    }

    private void SetDisplayParent(Transform newParent)
    {
        if (display != null)
        {
            display.SetParent(newParent, false);
            //display.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning("Display is not set.");
        }
    }
}