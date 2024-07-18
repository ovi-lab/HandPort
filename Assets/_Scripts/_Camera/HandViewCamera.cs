using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandViewCamera : CameraPlacement
{
    public Transform rightHand;
    void Update()
    {
        if (rightHand == null)
        {
            return;
        }

        // Check if CurrentTarget is not null
        if (rightHand.transform == null)
        {
            return;
        }

        targetPosition = rightHand.transform.position;
        PlaceCamera(targetPosition);
    }
}
