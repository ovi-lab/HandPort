using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandViewCamera : CameraPlacement
{
    public Transform rightHand;
    private GoGoDetachAdapterStable3 teleportAdapter;

    private void Start()
    {
        teleportAdapter = FindObjectOfType<GoGoDetachAdapterStable3>( true);
    }

    void Update()
    {
        if (rightHand == null)
        {
            return;
        }

        
        if (teleportAdapter == null)
        {
            Debug.LogError("GoGoDetachAdapterStable3 component is missing. Please attach it to the GameObject.");
            return;
        }

        // Ensure forwardDirection is set
        Vector3 forwardDirection = teleportAdapter.shoulderToWristDirection;
        if (forwardDirection == Vector3.zero)
        {
            forwardDirection = rightHand.forward;
        }

        // Set target position and place the camera
        Vector3 targetPosition = rightHand.position;
        PlaceCamera(targetPosition, forwardDirection);
    }
}
