using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MiniMapCameraPlacement : MonoBehaviour
{
    [SerializeField] protected float heightOffset;
    [SerializeField] protected float horizontalOffset;
    [SerializeField] protected float cameraLookAngle;
    protected Vector3 targetPosition;
    protected Camera mainCamera;

    

    protected void OnEnable()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        targetPosition = MiniMapCameraManager.ParabolaPosition;
        PlaceCamera(targetPosition);
    }

    

    protected void PlaceCamera(Vector3 target)
    {
        target.y += heightOffset;
        target.z -= horizontalOffset;
        Vector3 cameraLook = target - mainCamera.transform.position;
        transform.forward = cameraLook;
        transform.rotation = Quaternion.Euler(cameraLookAngle,0,0);
        transform.position = target;
    }
}
