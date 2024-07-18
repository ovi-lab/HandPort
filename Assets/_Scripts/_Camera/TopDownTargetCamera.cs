using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownTargetCamera : CameraPlacement
{
    void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        // Check if CurrentTarget is not null
        if (GameManager.Instance.CurrentTarget == null)
        {
            return;
        }
        targetPosition = GameManager.Instance.CurrentTarget.transform.position;
        PlaceCamera(targetPosition);
    }
}
