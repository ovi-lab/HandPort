using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WIMWidget : MonoBehaviour
{
    [SerializeField] public int anchorType = 0;
    [SerializeField] public Transform[] anchorPoint;

    private Transform display;
    private bool isInitialized = false;
    private CameraManager camManager;

    private void Start()
    {
        InitializeDisplay();
        if (anchorPoint.Length > 0)
        {
            SetDisplayParent(anchorPoint[anchorType]);
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
        foreach (Transform child  in transform)
        { 
            child.gameObject.SetActive(false);
        }
        if (display == null)
        {
            camManager = transform.parent.GetComponent<CameraManager>();
            display = transform.GetChild((int)camManager.resolution);
            display.gameObject.SetActive(true);
        }
    }

    private void SetDisplayParent(Transform newParent)
    {
        if (display != null)
        {
            display.SetParent(newParent, false);
        }
        else
        {
            Debug.LogWarning("Display is not set.");
        }
    }
}