using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : CameraPlacement
{
    [SerializeField] private GameObject miniMapIndicator;


    void Update()
    {
        targetPosition = mainCamera.transform.position;
        //miniMapIndicator.transform.position = teleportation.gPointer.touchPosition;
        PlaceCamera(targetPosition);
    }
}
