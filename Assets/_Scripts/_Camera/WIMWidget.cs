using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraAnchor
{
    HMDAnchor = 0,
    HandAnchor = 1,
}
public class WIMWidget : MonoBehaviour
{
    public readonly Dictionary<string, CameraAnchor> stringToEnumConverter = new Dictionary<string, CameraAnchor>()
    {
        { "HMDAnchor", CameraAnchor.HMDAnchor },
        { "HandAnchor", CameraAnchor.HandAnchor },

    };
    
    [SerializeField] private Transform[] anchorPoint;
    [SerializeField] private CameraAnchor anchor;

    private Transform display;
    private bool isInitialized = false;
    private float initialZPosition;

    private void Start()
    {
        InitializeDisplay();
        if (anchorPoint.Length > 0)
        {
            SetDisplayParent(anchorPoint[(int)anchor]);
            // if (anchor == CameraAnchor.HandAnchor)
            // {
            //     initialZPosition = display.localPosition.z;
            //     if (anchor == CameraAnchor.HandAnchor)
            //     {
            //         initialZPosition = display.localPosition.z;
            //     }
            // }
        }
        isInitialized = true;
    }
    
    private void Update()
    {
        // if (anchor == CameraAnchor.HandAnchor && display != null)
        // {
        //     Vector3 newPosition = anchorPoint[(int)anchor].position;
        //     newPosition.z = initialZPosition;
        //     display.position = newPosition;
        // }
    }

    private void OnValidate()
    {
        if (!isInitialized) return;

        InitializeDisplay();
        // if (anchorPoint.Length > (int)anchor && anchorPoint[(int)anchor] != null)
        // {
        //     SetDisplayParent(anchorPoint[(int)anchor]);
        // }
        // else
        // {
        //     Debug.LogWarning("Invalid anchorType or anchorPoint.");
        // }
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
            if (anchor == CameraAnchor.HandAnchor)
            {
                Vector3 localPosition = display.localPosition;
                localPosition.x = 0;
                localPosition.y = 1.5f;
                localPosition.z = 2;
                
                display.localPosition = localPosition;
            }
            else
            {
                Vector3 localPosition = display.localPosition;
                localPosition.x = 1;
                localPosition.y = 1;
                localPosition.z = 3.5f;
                display.localPosition = localPosition;
            }
        }
        else
        {
            Debug.LogWarning("Display is not set.");
        }
    }
}